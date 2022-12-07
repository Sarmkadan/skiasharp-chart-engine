// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Events;

/// <summary>
/// Central event dispatcher implementing pub-sub pattern.
/// Manages subscriptions and dispatches events to all registered subscribers.
/// </summary>
public class EventDispatcher
{
    private readonly Dictionary<string, List<IEventHandler>> _handlers;
    private readonly ILogger<EventDispatcher> _logger;

    public EventDispatcher(ILogger<EventDispatcher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _handlers = new Dictionary<string, List<IEventHandler>>(StringComparer.OrdinalIgnoreCase);
    }

    // Subscribe to an event
    public void Subscribe(string eventType, IEventHandler handler)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Event type cannot be empty", nameof(eventType));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            if (!_handlers.TryGetValue(eventType, out var handlerList))
            {
                handlerList = new List<IEventHandler>();
                _handlers[eventType] = handlerList;
            }

            handlerList.Add(handler);
            _logger.LogInformation("Handler subscribed to event: {EventType}", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to event: {EventType}", eventType);
            throw;
        }
    }

    // Unsubscribe from event
    public void Unsubscribe(string eventType, IEventHandler handler)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(eventType) || handler == null)
                return;

            if (_handlers.TryGetValue(eventType, out var handlerList))
            {
                handlerList.Remove(handler);
                _logger.LogInformation("Handler unsubscribed from event: {EventType}", eventType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from event: {EventType}", eventType);
        }
    }

    // Dispatch event synchronously
    public void Dispatch(string eventType, object eventData)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Event type cannot be empty", nameof(eventType));

            if (!_handlers.TryGetValue(eventType, out var handlerList))
            {
                _logger.LogDebug("No handlers registered for event: {EventType}", eventType);
                return;
            }

            foreach (var handler in handlerList.ToList())
            {
                try
                {
                    handler.Handle(eventType, eventData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling event: {EventType}", eventType);
                }
            }

            _logger.LogDebug("Event dispatched: {EventType} to {Count} handlers", eventType, handlerList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching event: {EventType}", eventType);
        }
    }

    // Dispatch event asynchronously
    public async Task DispatchAsync(string eventType, object eventData)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(eventType))
                throw new ArgumentException("Event type cannot be empty", nameof(eventType));

            if (!_handlers.TryGetValue(eventType, out var handlerList))
            {
                _logger.LogDebug("No handlers registered for event: {EventType}", eventType);
                return;
            }

            var tasks = handlerList.ToList().Select(async handler =>
            {
                try
                {
                    if (handler is IAsyncEventHandler asyncHandler)
                    {
                        await asyncHandler.HandleAsync(eventType, eventData);
                    }
                    else
                    {
                        handler.Handle(eventType, eventData);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling event: {EventType}", eventType);
                }
            });

            await Task.WhenAll(tasks);
            _logger.LogDebug("Async event dispatched: {EventType} to {Count} handlers", eventType, handlerList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching async event: {EventType}", eventType);
        }
    }

    // Get handler count for event type
    public int GetHandlerCount(string eventType)
    {
        return _handlers.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
    }

    // Get all subscribed event types
    public IEnumerable<string> GetSubscribedEventTypes() => _handlers.Keys;

    // Clear all handlers
    public void Clear()
    {
        _handlers.Clear();
        _logger.LogInformation("All event handlers cleared");
    }
}

/// <summary>
/// Interface for synchronous event handlers.
/// </summary>
public interface IEventHandler
{
    void Handle(string eventType, object eventData);
}

/// <summary>
/// Interface for asynchronous event handlers.
/// </summary>
public interface IAsyncEventHandler : IEventHandler
{
    Task HandleAsync(string eventType, object eventData);
}
