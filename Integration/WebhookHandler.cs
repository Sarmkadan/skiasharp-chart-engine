// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Events;

namespace SkiaSharpChartEngine.Integration;

/// <summary>
/// Handles webhook subscriptions and deliveries
/// Allows external services to be notified of chart events
/// </summary>
public class WebhookHandler : IChartEventSubscriber
{
    private readonly Dictionary<string, WebhookSubscription> _subscriptions = new();
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebhookHandler> _logger;
    private readonly object _lock = new();

    public WebhookHandler(ILogger<WebhookHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// Registers a webhook for an event type
    /// </summary>
    public string RegisterWebhook(string eventType, string webhookUrl, WebhookOptions? options = null)
    {
        if (string.IsNullOrEmpty(eventType) || string.IsNullOrEmpty(webhookUrl))
            throw new ArgumentException("Event type and webhook URL are required");

        var subscription = new WebhookSubscription
        {
            Id = Guid.NewGuid().ToString(),
            EventType = eventType,
            WebhookUrl = webhookUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Options = options ?? new WebhookOptions()
        };

        lock (_lock)
        {
            _subscriptions[subscription.Id] = subscription;
        }

        _logger.LogInformation("Webhook registered: {SubscriptionId} for {EventType}", subscription.Id, eventType);
        return subscription.Id;
    }

    /// <summary>
    /// Unregisters a webhook
    /// </summary>
    public bool UnregisterWebhook(string subscriptionId)
    {
        if (string.IsNullOrEmpty(subscriptionId))
            return false;

        lock (_lock)
        {
            if (_subscriptions.Remove(subscriptionId))
            {
                _logger.LogInformation("Webhook unregistered: {SubscriptionId}", subscriptionId);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all webhook subscriptions
    /// </summary>
    public List<WebhookSubscription> GetSubscriptions()
    {
        lock (_lock)
        {
            return new List<WebhookSubscription>(_subscriptions.Values);
        }
    }

    public async Task OnChartCreatedAsync(ChartCreatedEvent @event)
    {
        await DeliverWebhook("chart.created", @event);
    }

    public async Task OnChartUpdatedAsync(ChartUpdatedEvent @event)
    {
        await DeliverWebhook("chart.updated", @event);
    }

    public async Task OnChartDeletedAsync(ChartDeletedEvent @event)
    {
        await DeliverWebhook("chart.deleted", @event);
    }

    public async Task OnChartRenderedAsync(ChartRenderedEvent @event)
    {
        await DeliverWebhook("chart.rendered", @event);
    }

    public async Task OnChartExportedAsync(ChartExportedEvent @event)
    {
        await DeliverWebhook("chart.exported", @event);
    }

    public async Task OnErrorAsync(ChartErrorEvent @event)
    {
        await DeliverWebhook("chart.error", @event);
    }

    private async Task DeliverWebhook(string eventType, ChartEvent chartEvent)
    {
        List<WebhookSubscription> matchingSubscriptions;
        lock (_lock)
        {
            matchingSubscriptions = _subscriptions.Values
                .Where(s => s.IsActive && s.EventType == eventType)
                .ToList();
        }

        foreach (var subscription in matchingSubscriptions)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    subscriptionId = subscription.Id,
                    eventType = eventType,
                    eventId = chartEvent.EventId,
                    timestamp = chartEvent.Timestamp,
                    data = chartEvent
                });

                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

                if (subscription.Options?.Headers != null)
                {
                    foreach (var header in subscription.Options.Headers)
                    {
                        content.Headers.Add(header.Key, header.Value);
                    }
                }

                var response = await _httpClient.PostAsync(subscription.WebhookUrl, content);

                subscription.LastDeliveryAt = DateTime.UtcNow;
                subscription.DeliveryCount++;
                subscription.IsHealthy = response.IsSuccessStatusCode;

                _logger.LogInformation(
                    "Webhook delivered: {SubscriptionId}, StatusCode: {StatusCode}",
                    subscription.Id,
                    response.StatusCode);
            }
            catch (Exception ex)
            {
                subscription.IsHealthy = false;
                subscription.LastErrorMessage = ex.Message;

                _logger.LogError(ex, "Error delivering webhook {SubscriptionId}", subscription.Id);
            }
        }
    }
}

/// <summary>
/// Webhook subscription configuration
/// </summary>
public class WebhookSubscription
{
    public string Id { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsHealthy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastDeliveryAt { get; set; }
    public int DeliveryCount { get; set; }
    public string? LastErrorMessage { get; set; }
    public WebhookOptions Options { get; set; } = new();
}

/// <summary>
/// Webhook delivery options
/// </summary>
public class WebhookOptions
{
    public Dictionary<string, string>? Headers { get; set; }
    public bool RetryOnFailure { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
}
