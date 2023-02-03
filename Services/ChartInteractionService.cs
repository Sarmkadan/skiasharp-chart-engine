// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Events;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Default implementation of <see cref="IChartInteractionService"/>.
/// Delegates hit-testing to <see cref="IInteractivityService"/> and maintains
/// per-chart selection state in a thread-safe dictionary.
/// </summary>
public sealed class ChartInteractionService : IChartInteractionService
{
    private readonly IInteractivityService _interactivityService;
    private readonly ILogger<ChartInteractionService> _logger;

    // chartId → (seriesName → selectedPoints)
    private readonly Dictionary<string, Dictionary<string, List<DataPoint>>> _selections = new();
    private readonly object _selectionLock = new();

    /// <inheritdoc/>
    public event EventHandler<ChartInteractionEventArgs>? Clicked;

    /// <inheritdoc/>
    public event EventHandler<ChartInteractionEventArgs>? Hovered;

    /// <inheritdoc/>
    public event EventHandler<ChartSelectionChangedEventArgs>? SelectionChanged;

    public ChartInteractionService(
        IInteractivityService interactivityService,
        ILogger<ChartInteractionService> logger)
    {
        _interactivityService = interactivityService ?? throw new ArgumentNullException(nameof(interactivityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public ChartInteractionEventArgs ProcessInteraction(
        Chart chart,
        ChartInteractionType interactionType,
        float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        TooltipOptions? tooltipOptions = null,
        ViewportState? viewport = null)
    {
        if (chart == null) throw new ArgumentNullException(nameof(chart));

        var hit = _interactivityService.HitTest(
            chart, pointerX, pointerY, canvasWidth, canvasHeight, tooltipOptions, viewport);

        var args = new ChartInteractionEventArgs
        {
            InteractionType = interactionType,
            PointerX        = pointerX,
            PointerY        = pointerY,
            Region          = hit.Region,
            HitDataPoint    = hit.IsHit ? hit.DataPoint : null,
            HitSeries       = hit.IsHit ? hit.Series   : null,
            SeriesIndex     = hit.SeriesIndex,
            TooltipText     = hit.TooltipText
        };

        RaiseEvent(chart, args);

        _logger.LogDebug(
            "Interaction processed – chart={ChartId}, type={Type}, region={Region}, hit={IsHit}",
            chart.Id, interactionType, hit.Region, hit.IsHit);

        return args;
    }

    /// <inheritdoc/>
    public async Task<ChartInteractionEventArgs> ProcessInteractionAsync(
        Chart chart,
        ChartInteractionType interactionType,
        float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        TooltipOptions? tooltipOptions = null,
        ViewportState? viewport = null,
        CancellationToken cancellationToken = default)
    {
        if (chart == null) throw new ArgumentNullException(nameof(chart));
        cancellationToken.ThrowIfCancellationRequested();

        var hit = await _interactivityService.HitTestAsync(
            chart, pointerX, pointerY, canvasWidth, canvasHeight,
            tooltipOptions, viewport, cancellationToken);

        var args = new ChartInteractionEventArgs
        {
            InteractionType = interactionType,
            PointerX        = pointerX,
            PointerY        = pointerY,
            Region          = hit.Region,
            HitDataPoint    = hit.IsHit ? hit.DataPoint : null,
            HitSeries       = hit.IsHit ? hit.Series   : null,
            SeriesIndex     = hit.SeriesIndex,
            TooltipText     = hit.TooltipText
        };

        RaiseEvent(chart, args);

        _logger.LogDebug(
            "Async interaction processed – chart={ChartId}, type={Type}, hit={IsHit}",
            chart.Id, interactionType, hit.IsHit);

        return args;
    }

    /// <inheritdoc/>
    public bool ToggleSelection(
        Chart chart,
        float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        ViewportState? viewport = null)
    {
        if (chart == null) throw new ArgumentNullException(nameof(chart));

        var hit = _interactivityService.HitTest(chart, pointerX, pointerY, canvasWidth, canvasHeight, null, viewport);
        if (!hit.IsHit || hit.DataPoint == null || hit.Series == null)
            return false;

        lock (_selectionLock)
        {
            if (!_selections.TryGetValue(chart.Id, out var chartSelection))
            {
                chartSelection = new Dictionary<string, List<DataPoint>>();
                _selections[chart.Id] = chartSelection;
            }

            if (!chartSelection.TryGetValue(hit.Series.Name, out var pts))
            {
                pts = new List<DataPoint>();
                chartSelection[hit.Series.Name] = pts;
            }

            var existing = pts.FirstOrDefault(p => p.X == hit.DataPoint.X && p.Y == hit.DataPoint.Y);
            if (existing != null)
                pts.Remove(existing);
            else
                pts.Add(hit.DataPoint);
        }

        RaiseSelectionChanged(chart);
        _logger.LogDebug("Selection toggled – chart={ChartId}, series={Series}", chart.Id, hit.Series.Name);
        return true;
    }

    /// <inheritdoc/>
    public void ClearSelection(Chart chart)
    {
        if (chart == null) throw new ArgumentNullException(nameof(chart));

        lock (_selectionLock)
        {
            _selections.Remove(chart.Id);
        }

        RaiseSelectionChanged(chart);
        _logger.LogDebug("Selection cleared – chart={ChartId}", chart.Id);
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, IReadOnlyList<DataPoint>> GetSelection(Chart chart)
    {
        if (chart == null) throw new ArgumentNullException(nameof(chart));

        lock (_selectionLock)
        {
            if (!_selections.TryGetValue(chart.Id, out var sel))
                return new Dictionary<string, IReadOnlyList<DataPoint>>();

            return sel.ToDictionary(
                kv => kv.Key,
                kv => (IReadOnlyList<DataPoint>)kv.Value.AsReadOnly());
        }
    }

    // -------------------------------------------------------------------------
    private void RaiseEvent(Chart chart, ChartInteractionEventArgs args)
    {
        switch (args.InteractionType)
        {
            case ChartInteractionType.Click:
            case ChartInteractionType.DoubleClick:
            case ChartInteractionType.ContextMenu:
                Clicked?.Invoke(chart, args);
                break;
            case ChartInteractionType.Hover:
                Hovered?.Invoke(chart, args);
                break;
            case ChartInteractionType.Select:
                ToggleSelection(chart, args.PointerX, args.PointerY, 0, 0);
                break;
        }
    }

    private void RaiseSelectionChanged(Chart chart)
    {
        var snapshot = GetSelection(chart);
        SelectionChanged?.Invoke(chart, new ChartSelectionChangedEventArgs
        {
            SelectedPoints = snapshot
        });
    }
}
