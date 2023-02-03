// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Events;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Handles user-interaction events (click, hover, select) for chart instances and
/// exposes strongly-typed events that consumers can subscribe to.
/// </summary>
public interface IChartInteractionService
{
    /// <summary>
    /// Raised whenever a click interaction is processed.
    /// </summary>
    event EventHandler<ChartInteractionEventArgs> Clicked;

    /// <summary>
    /// Raised whenever a hover interaction is processed.
    /// </summary>
    event EventHandler<ChartInteractionEventArgs> Hovered;

    /// <summary>
    /// Raised when the set of selected data points changes.
    /// </summary>
    event EventHandler<ChartSelectionChangedEventArgs> SelectionChanged;

    /// <summary>
    /// Processes a pointer interaction at the given canvas coordinates and raises
    /// the appropriate event(s).
    /// </summary>
    /// <param name="chart">The target chart.</param>
    /// <param name="interactionType">The type of interaction to process.</param>
    /// <param name="pointerX">Canvas X coordinate in pixels.</param>
    /// <param name="pointerY">Canvas Y coordinate in pixels.</param>
    /// <param name="canvasWidth">Total canvas width in pixels.</param>
    /// <param name="canvasHeight">Total canvas height in pixels.</param>
    /// <param name="tooltipOptions">Optional tooltip configuration used during hit-testing.</param>
    /// <param name="viewport">Optional active viewport.</param>
    /// <returns>The resolved <see cref="ChartInteractionEventArgs"/> describing the outcome.</returns>
    ChartInteractionEventArgs ProcessInteraction(
        Chart chart,
        ChartInteractionType interactionType,
        float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        TooltipOptions? tooltipOptions = null,
        ViewportState? viewport = null);

    /// <summary>
    /// Asynchronous overload of <see cref="ProcessInteraction"/>.
    /// </summary>
    Task<ChartInteractionEventArgs> ProcessInteractionAsync(
        Chart chart,
        ChartInteractionType interactionType,
        float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        TooltipOptions? tooltipOptions = null,
        ViewportState? viewport = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggles selection of the data point nearest to the given coordinates.
    /// If the pointer misses all series the selection is unchanged.
    /// </summary>
    /// <returns>
    /// <c>true</c> when a point was toggled; <c>false</c> when the pointer missed.
    /// </returns>
    bool ToggleSelection(
        Chart chart,
        float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        ViewportState? viewport = null);

    /// <summary>
    /// Clears all selected data points for the given chart and raises
    /// <see cref="SelectionChanged"/>.
    /// </summary>
    void ClearSelection(Chart chart);

    /// <summary>
    /// Returns the current selection for the given chart.
    /// </summary>
    IReadOnlyDictionary<string, IReadOnlyList<DataPoint>> GetSelection(Chart chart);
}
