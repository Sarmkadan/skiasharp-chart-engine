// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Events;

/// <summary>
/// Publishes chart events to registered subscribers
/// Implements publish-subscribe pattern for loose coupling
/// </summary>
public class ChartEventPublisher : IChartEventPublisher
{
    private readonly List<IChartEventSubscriber> _subscribers = new();
    private readonly ILogger<ChartEventPublisher> _logger;
    private readonly object _subscriberLock = new();

    public ChartEventPublisher(ILogger<ChartEventPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Subscribes to chart events
    /// </summary>
    public void Subscribe(IChartEventSubscriber subscriber)
    {
        if (subscriber == null)
            throw new ArgumentNullException(nameof(subscriber));

        lock (_subscriberLock)
        {
            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
                _logger.LogDebug("Subscriber registered: {Type}", subscriber.GetType().Name);
            }
        }
    }

    /// <summary>
    /// Unsubscribes from chart events
    /// </summary>
    public void Unsubscribe(IChartEventSubscriber subscriber)
    {
        if (subscriber == null)
            return;

        lock (_subscriberLock)
        {
            if (_subscribers.Remove(subscriber))
            {
                _logger.LogDebug("Subscriber unregistered: {Type}", subscriber.GetType().Name);
            }
        }
    }

    /// <summary>
    /// Publishes a chart created event
    /// </summary>
    public async Task PublishChartCreatedAsync(ChartCreatedEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        await PublishAsync(@event, e => e.OnChartCreatedAsync(@event as ChartCreatedEvent)!);
    }

    /// <summary>
    /// Publishes a chart updated event
    /// </summary>
    public async Task PublishChartUpdatedAsync(ChartUpdatedEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        await PublishAsync(@event, e => e.OnChartUpdatedAsync(@event as ChartUpdatedEvent)!);
    }

    /// <summary>
    /// Publishes a chart deleted event
    /// </summary>
    public async Task PublishChartDeletedAsync(ChartDeletedEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        await PublishAsync(@event, e => e.OnChartDeletedAsync(@event as ChartDeletedEvent)!);
    }

    /// <summary>
    /// Publishes a chart rendered event
    /// </summary>
    public async Task PublishChartRenderedAsync(ChartRenderedEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        await PublishAsync(@event, e => e.OnChartRenderedAsync(@event as ChartRenderedEvent)!);
    }

    /// <summary>
    /// Publishes a chart exported event
    /// </summary>
    public async Task PublishChartExportedAsync(ChartExportedEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        await PublishAsync(@event, e => e.OnChartExportedAsync(@event as ChartExportedEvent)!);
    }

    /// <summary>
    /// Publishes an error event
    /// </summary>
    public async Task PublishErrorAsync(ChartErrorEvent @event)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        await PublishAsync(@event, e => e.OnErrorAsync(@event as ChartErrorEvent)!);
    }

    /// <summary>
    /// Gets the number of registered subscribers
    /// </summary>
    public int GetSubscriberCount()
    {
        lock (_subscriberLock)
        {
            return _subscribers.Count;
        }
    }

    private async Task PublishAsync(ChartEvent @event, Func<IChartEventSubscriber, Task> publishAction)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            List<IChartEventSubscriber> subscribersCopy;
            lock (_subscriberLock)
            {
                subscribersCopy = new List<IChartEventSubscriber>(_subscribers);
            }

            var tasks = subscribersCopy.Select(async subscriber =>
            {
                try
                {
                    await publishAction(subscriber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in event subscriber {Type} for event {EventName}",
                        subscriber.GetType().Name, @event.GetEventName());
                }
            });

            await Task.WhenAll(tasks);

            stopwatch.Stop();
            _logger.LogInformation(
                "Event published - Type: {EventName}, EventId: {EventId}, Subscribers: {SubscriberCount}, " +
                "Duration: {DurationMs}ms",
                @event.GetEventName(),
                @event.EventId,
                subscribersCopy.Count,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventName}", @event.GetEventName());
            throw;
        }
    }
}

/// <summary>
/// Interface for event publishing
/// </summary>
public interface IChartEventPublisher
{
    void Subscribe(IChartEventSubscriber subscriber);
    void Unsubscribe(IChartEventSubscriber subscriber);
    Task PublishChartCreatedAsync(ChartCreatedEvent @event);
    Task PublishChartUpdatedAsync(ChartUpdatedEvent @event);
    Task PublishChartDeletedAsync(ChartDeletedEvent @event);
    Task PublishChartRenderedAsync(ChartRenderedEvent @event);
    Task PublishChartExportedAsync(ChartExportedEvent @event);
    Task PublishErrorAsync(ChartErrorEvent @event);
    int GetSubscriberCount();
}
