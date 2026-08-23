// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Events;

/// <summary>
/// Defines contract for chart event subscribers.
/// Enables loose coupling between chart engine and event handlers.
/// </summary>
public interface IChartEventSubscriber
{
    /// <summary>
    /// Called when a chart is created.
    /// </summary>
    /// <param name="event">The chart created event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnChartCreatedAsync(ChartCreatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Called when a chart is updated.
    /// </summary>
    /// <param name="event">The chart updated event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnChartUpdatedAsync(ChartUpdatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Called when a chart is deleted.
    /// </summary>
    /// <param name="event">The chart deleted event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnChartDeletedAsync(ChartDeletedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Called when a chart is rendered.
    /// </summary>
    /// <param name="event">The chart rendered event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnChartRenderedAsync(ChartRenderedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Called when a chart export completes.
    /// </summary>
    /// <param name="event">The chart exported event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnChartExportedAsync(ChartExportedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        throw new NotImplementedException();
    }

    /// <summary>
    /// Called when an error occurs.
    /// </summary>
    /// <param name="event">The chart error event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task OnErrorAsync(ChartErrorEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);
        throw new NotImplementedException();
    }
}

/// <summary>
/// Base event class for all chart events.
/// </summary>
public abstract class ChartEvent
{
    /// <summary>
    /// Gets the unique identifier assigned to this event instance.
    /// </summary>
    public string EventId { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the UTC timestamp indicating when the event was created.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the name of the component that raised the event.
    /// </summary>
    public string? SourceName { get; set; }

    /// <summary>
    /// Gets additional metadata associated with the event.
    /// </summary>
    public Dictionary<string, object> Metadata { get; } = new();

    /// <summary>
    /// Gets the name of the concrete event type.
    /// </summary>
    /// <returns>The simple name of the derived event class.</returns>
    public virtual string GetEventName() => GetType().Name;
}

/// <summary>
/// Event published when a chart is created.
/// </summary>
public class ChartCreatedEvent : ChartEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the chart that was created.
    /// </summary>
    public required string ChartId { get; set; }

    /// <summary>
    /// Gets or sets the title of the chart that was created.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the type of the chart that was created.
    /// </summary>
    public ChartType ChartType { get; set; }

    /// <summary>
    /// Gets or sets the number of series contained in the created chart.
    /// </summary>
    public int SeriesCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of data points across all series in the created chart.
    /// </summary>
    public int DataPointCount { get; set; }
}

/// <summary>
/// Event published when a chart is updated.
/// </summary>
public class ChartUpdatedEvent : ChartEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the chart that was updated.
    /// </summary>
    public required string ChartId { get; set; }

    /// <summary>
    /// Gets or sets the title of the chart at the time of the update.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the names of the fields that were modified by the update.
    /// </summary>
    public string[]? ModifiedFields { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the previous update, if one occurred.
    /// </summary>
    public DateTime? PreviousUpdateTime { get; set; }
}

/// <summary>
/// Event published when a chart is deleted.
/// </summary>
public class ChartDeletedEvent : ChartEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the chart that was deleted.
    /// </summary>
    public required string ChartId { get; set; }

    /// <summary>
    /// Gets or sets the title of the chart that was deleted.
    /// </summary>
    public string? ChartTitle { get; set; }

    /// <summary>
    /// Gets or sets the identity of the user or component that deleted the chart.
    /// </summary>
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Event published when a chart is rendered.
/// </summary>
public class ChartRenderedEvent : ChartEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the chart that was rendered.
    /// </summary>
    public required string ChartId { get; set; }

    /// <summary>
    /// Gets or sets the width of the rendered output in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the rendered output in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Gets or sets the dots-per-inch resolution used during rendering.
    /// </summary>
    public float Dpi { get; set; }

    /// <summary>
    /// Gets or sets the duration of the render operation in milliseconds.
    /// </summary>
    public long RenderTimeMs { get; set; }

    /// <summary>
    /// Gets or sets the size of the rendered output in bytes, if available.
    /// </summary>
    public long? OutputSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the render operation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message describing why rendering failed, if applicable.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Event published when a chart is exported.
/// </summary>
public class ChartExportedEvent : ChartEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the chart that was exported.
    /// </summary>
    public required string ChartId { get; set; }

    /// <summary>
    /// Gets or sets the format used for the export operation (e.g. PNG, SVG).
    /// </summary>
    public required string ExportFormat { get; set; }

    /// <summary>
    /// Gets or sets the path of the file produced by the export, if written to disk.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the size of the exported output in bytes, if available.
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the duration of the export operation in milliseconds.
    /// </summary>
    public long ExportTimeMs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the export operation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the error message describing why the export failed, if applicable.
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Event published when an error occurs in chart processing.
/// </summary>
public class ChartErrorEvent : ChartEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the chart related to the error, if known.
    /// </summary>
    public string? ChartId { get; set; }

    /// <summary>
    /// Gets or sets the message describing the error.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the category or type classification of the error.
    /// </summary>
    public string? ErrorType { get; set; }

    /// <summary>
    /// Gets or sets the stack trace captured at the point of failure, if available.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Gets or sets the numeric error code associated with the failure.
    /// </summary>
    public int ErrorCode { get; set; }
}

/// <summary>
/// Adapter for simple event subscription with delegates.
/// </summary>
public class DelegateChartEventSubscriber : IChartEventSubscriber
{
    private Func<ChartCreatedEvent, Task>? _onChartCreated;
    private Func<ChartUpdatedEvent, Task>? _onChartUpdated;
    private Func<ChartDeletedEvent, Task>? _onChartDeleted;
    private Func<ChartRenderedEvent, Task>? _onChartRendered;
    private Func<ChartExportedEvent, Task>? _onChartExported;
    private Func<ChartErrorEvent, Task>? _onError;

    /// <summary>
    /// Registers delegate handlers for chart events.
    /// </summary>
    /// <param name="onCreated">Optional handler invoked when a chart is created.</param>
    /// <param name="onUpdated">Optional handler invoked when a chart is updated.</param>
    /// <param name="onDeleted">Optional handler invoked when a chart is deleted.</param>
    /// <param name="onRendered">Optional handler invoked when a chart is rendered.</param>
    /// <param name="onExported">Optional handler invoked when a chart export completes.</param>
    /// <param name="onError">Optional handler invoked when an error occurs.</param>
    /// <returns>This subscriber instance, enabling fluent configuration.</returns>
    public DelegateChartEventSubscriber Subscribe(
        Func<ChartCreatedEvent, Task>? onCreated = null,
        Func<ChartUpdatedEvent, Task>? onUpdated = null,
        Func<ChartDeletedEvent, Task>? onDeleted = null,
        Func<ChartRenderedEvent, Task>? onRendered = null,
        Func<ChartExportedEvent, Task>? onExported = null,
        Func<ChartErrorEvent, Task>? onError = null)
    {
        _onChartCreated = onCreated;
        _onChartUpdated = onUpdated;
        _onChartDeleted = onDeleted;
        _onChartRendered = onRendered;
        _onChartExported = onExported;
        _onError = onError;
        return this;
    }

    /// <summary>
    /// Called when a chart is created.
    /// </summary>
    /// <param name="event">The chart created event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task OnChartCreatedAsync(ChartCreatedEvent @event)
        => _onChartCreated?.Invoke(@event) ?? Task.CompletedTask;

    /// <summary>
    /// Called when a chart is updated.
    /// </summary>
    /// <param name="event">The chart updated event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task OnChartUpdatedAsync(ChartUpdatedEvent @event)
        => _onChartUpdated?.Invoke(@event) ?? Task.CompletedTask;

    /// <summary>
    /// Called when a chart is deleted.
    /// </summary>
    /// <param name="event">The chart deleted event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task OnChartDeletedAsync(ChartDeletedEvent @event)
        => _onChartDeleted?.Invoke(@event) ?? Task.CompletedTask;

    /// <summary>
    /// Called when a chart is rendered.
    /// </summary>
    /// <param name="event">The chart rendered event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task OnChartRenderedAsync(ChartRenderedEvent @event)
        => _onChartRendered?.Invoke(@event) ?? Task.CompletedTask;

    /// <summary>
    /// Called when a chart export completes.
    /// </summary>
    /// <param name="event">The chart exported event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task OnChartExportedAsync(ChartExportedEvent @event)
        => _onChartExported?.Invoke(@event) ?? Task.CompletedTask;

    /// <summary>
    /// Called when an error occurs.
    /// </summary>
    /// <param name="event">The chart error event.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task OnErrorAsync(ChartErrorEvent @event)
        => _onError?.Invoke(@event) ?? Task.CompletedTask;
}
