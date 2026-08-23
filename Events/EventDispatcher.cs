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

    /// <summary>
    /// Initializes a new instance of the <see cref="EventDispatcher"/> class.
    /// </summary>
    /// <param name="logger">The logger used for diagnostics and error reporting.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public EventDispatcher(ILogger<EventDispatcher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _handlers = new Dictionary<string, List<IEventHandler>>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Subscribes a handler to the specified event type.
    /// </summary>
    /// <param name="eventType">The name of the event type to subscribe to.</param>
    /// <param name="handler">The handler that will receive events of the specified type.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="eventType"/> is null, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
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

    /// <summary>
    /// Unsubscribes a handler from the specified event type.
    /// Does nothing if the event type is invalid, the handler is null, or no matching subscription exists.
    /// </summary>
    /// <param name="eventType">The name of the event type to unsubscribe from.</param>
    /// <param name="handler">The handler to remove from the subscription list.</param>
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

    /// <summary>
    /// Dispatches an event synchronously to all handlers registered for the specified event type.
    /// Exceptions thrown by individual handlers are logged and do not prevent remaining handlers from executing.
    /// </summary>
    /// <param name="eventType">The name of the event type to dispatch.</param>
    /// <param name="eventData">The payload associated with the event.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="eventType"/> is null, empty, or whitespace.</exception>
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

    /// <summary>
    /// Dispatches an event asynchronously to all handlers registered for the specified event type.
    /// Handlers implementing <see cref="IAsyncEventHandler"/> are awaited; exceptions thrown by
    /// individual handlers are logged and do not prevent remaining handlers from executing.
    /// </summary>
    /// <param name="eventType">The name of the event type to dispatch.</param>
    /// <param name="eventData">The payload associated with the event.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="eventType"/> is null, empty, or whitespace.</exception>
    /// <returns>A task representing the asynchronous dispatch operation.</returns>
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

    /// <summary>
    /// Gets the number of handlers registered for the specified event type.
    /// </summary>
    /// <param name="eventType">The name of the event type.</param>
    /// <returns>The number of registered handlers, or zero if none are registered.</returns>
    public int GetHandlerCount(string eventType)
    {
        return _handlers.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
    }

    /// <summary>
    /// Gets all currently subscribed event types.
    /// </summary>
    /// <returns>An enumerable collection of subscribed event type names.</returns>
    public IEnumerable<string> GetSubscribedEventTypes() => _handlers.Keys;

    /// <summary>
    /// Removes all registered event handlers.
    /// </summary>
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
    /// <summary>
    /// Handles the specified event synchronously.
    /// </summary>
    /// <param name="eventType">The name of the event type being dispatched.</param>
    /// <param name="eventData">The payload associated with the event.</param>
    void Handle(string eventType, object eventData);
}

/// <summary>
/// Interface for asynchronous event handlers.
/// </summary>
public interface IAsyncEventHandler : IEventHandler
{
    /// <summary>
    /// Handles the specified event asynchronously.
    /// </summary>
    /// <param name="eventType">The name of the event type being dispatched.</param>
    /// <param name="eventData">The payload associated with the event.</param>
    /// <returns>A task representing the asynchronous handling operation.</returns>
    Task HandleAsync(string eventType, object eventData);
}
