// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Events;

/// <summary>
/// Types of interactive input events raised against a chart.
/// </summary>
public enum ChartInteractionType
{
    /// <summary>Pointer clicked on a chart region.</summary>
    Click = 0,
    /// <summary>Pointer is hovering over a chart region.</summary>
    Hover = 1,
    /// <summary>A data point or series was selected.</summary>
    Select = 2,
    /// <summary>A data point or series was deselected.</summary>
    Deselect = 3,
    /// <summary>Right-click / context-menu gesture.</summary>
    ContextMenu = 4,
    /// <summary>Double-click gesture.</summary>
    DoubleClick = 5
}

/// <summary>
/// Arguments for a user-interaction event raised by <see cref="IChartInteractionService"/>.
/// </summary>
public sealed class ChartInteractionEventArgs : EventArgs
{
    /// <summary>Gets the type of interaction.</summary>
    public ChartInteractionType InteractionType { get; init; }

    /// <summary>Gets the canvas X coordinate (pixels) where the interaction occurred.</summary>
    public float PointerX { get; init; }

    /// <summary>Gets the canvas Y coordinate (pixels) where the interaction occurred.</summary>
    public float PointerY { get; init; }

    /// <summary>Gets the logical chart region that was hit.</summary>
    public ChartRegion Region { get; init; }

    /// <summary>Gets the data point that was hit, or <c>null</c> if the pointer missed all series.</summary>
    public DataPoint? HitDataPoint { get; init; }

    /// <summary>Gets the series that owns the hit data point, or <c>null</c> on a miss.</summary>
    public ChartSeries? HitSeries { get; init; }

    /// <summary>Gets the zero-based index of the series within the chart, or <c>-1</c> on a miss.</summary>
    public int SeriesIndex { get; init; } = -1;

    /// <summary>Gets the formatted tooltip text produced by the hit-test, or an empty string on a miss.</summary>
    public string TooltipText { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this event was created.</summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    /// <summary>Gets arbitrary metadata attached by the caller.</summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Describes which data points are currently selected on a chart.
/// </summary>
public sealed class ChartSelectionChangedEventArgs : EventArgs
{
    /// <summary>Gets the selected data points, keyed by series name.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<DataPoint>> SelectedPoints { get; init; }
        = new Dictionary<string, IReadOnlyList<DataPoint>>();

    /// <summary>Gets whether the selection is empty.</summary>
    public bool IsEmpty => !SelectedPoints.Any(kv => kv.Value.Count > 0);

    /// <summary>Gets the total number of selected data points across all series.</summary>
    public int TotalSelected => SelectedPoints.Values.Sum(pts => pts.Count);

    /// <summary>Gets the UTC timestamp when this event was created.</summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
