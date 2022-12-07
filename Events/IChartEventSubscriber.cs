// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Events;

/// <summary>
/// Defines contract for chart event subscribers
/// Enables loose coupling between chart engine and event handlers
/// </summary>
public interface IChartEventSubscriber
{
    /// <summary>
    /// Called when a chart is created
    /// </summary>
    Task OnChartCreatedAsync(ChartCreatedEvent @event);

    /// <summary>
    /// Called when a chart is updated
    /// </summary>
    Task OnChartUpdatedAsync(ChartUpdatedEvent @event);

    /// <summary>
    /// Called when a chart is deleted
    /// </summary>
    Task OnChartDeletedAsync(ChartDeletedEvent @event);

    /// <summary>
    /// Called when a chart is rendered
    /// </summary>
    Task OnChartRenderedAsync(ChartRenderedEvent @event);

    /// <summary>
    /// Called when a chart export completes
    /// </summary>
    Task OnChartExportedAsync(ChartExportedEvent @event);

    /// <summary>
    /// Called when an error occurs
    /// </summary>
    Task OnErrorAsync(ChartErrorEvent @event);
}

/// <summary>
/// Base event class for all chart events
/// </summary>
public abstract class ChartEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string? SourceName { get; set; }
    public Dictionary<string, object> Metadata { get; } = new();

    public virtual string GetEventName() => GetType().Name;
}

/// <summary>
/// Event published when a chart is created
/// </summary>
public class ChartCreatedEvent : ChartEvent
{
    public required string ChartId { get; set; }
    public string? Title { get; set; }
    public ChartType ChartType { get; set; }
    public int SeriesCount { get; set; }
    public int DataPointCount { get; set; }
}

/// <summary>
/// Event published when a chart is updated
/// </summary>
public class ChartUpdatedEvent : ChartEvent
{
    public required string ChartId { get; set; }
    public string? Title { get; set; }
    public string[]? ModifiedFields { get; set; }
    public DateTime? PreviousUpdateTime { get; set; }
}

/// <summary>
/// Event published when a chart is deleted
/// </summary>
public class ChartDeletedEvent : ChartEvent
{
    public required string ChartId { get; set; }
    public string? ChartTitle { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Event published when a chart is rendered
/// </summary>
public class ChartRenderedEvent : ChartEvent
{
    public required string ChartId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public float Dpi { get; set; }
    public long RenderTimeMs { get; set; }
    public long? OutputSizeBytes { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Event published when a chart is exported
/// </summary>
public class ChartExportedEvent : ChartEvent
{
    public required string ChartId { get; set; }
    public required string ExportFormat { get; set; }
    public string? FilePath { get; set; }
    public long? FileSizeBytes { get; set; }
    public long ExportTimeMs { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Event published when an error occurs in chart processing
/// </summary>
public class ChartErrorEvent : ChartEvent
{
    public string? ChartId { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; }
    public string? StackTrace { get; set; }
    public int ErrorCode { get; set; }
}

/// <summary>
/// Adapter for simple event subscription with delegates
/// </summary>
public class DelegateChartEventSubscriber : IChartEventSubscriber
{
    private Func<ChartCreatedEvent, Task>? _onChartCreated;
    private Func<ChartUpdatedEvent, Task>? _onChartUpdated;
    private Func<ChartDeletedEvent, Task>? _onChartDeleted;
    private Func<ChartRenderedEvent, Task>? _onChartRendered;
    private Func<ChartExportedEvent, Task>? _onChartExported;
    private Func<ChartErrorEvent, Task>? _onError;

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

    public Task OnChartCreatedAsync(ChartCreatedEvent @event)
        => _onChartCreated?.Invoke(@event) ?? Task.CompletedTask;

    public Task OnChartUpdatedAsync(ChartUpdatedEvent @event)
        => _onChartUpdated?.Invoke(@event) ?? Task.CompletedTask;

    public Task OnChartDeletedAsync(ChartDeletedEvent @event)
        => _onChartDeleted?.Invoke(@event) ?? Task.CompletedTask;

    public Task OnChartRenderedAsync(ChartRenderedEvent @event)
        => _onChartRendered?.Invoke(@event) ?? Task.CompletedTask;

    public Task OnChartExportedAsync(ChartExportedEvent @event)
        => _onChartExported?.Invoke(@event) ?? Task.CompletedTask;

    public Task OnErrorAsync(ChartErrorEvent @event)
        => _onError?.Invoke(@event) ?? Task.CompletedTask;
}
