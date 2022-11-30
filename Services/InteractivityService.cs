// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Provides interactive chart capabilities: nearest-point tooltip hit-testing and zoom/pan
/// viewport management over a chart's data coordinate space.
/// </summary>
public interface IInteractivityService
{
    /// <summary>
    /// Finds the data point nearest to a canvas pointer position and returns tooltip information.
    /// Returns <see cref="TooltipHitResult.Miss"/> when no point falls within the hit radius.
    /// </summary>
    TooltipHitResult HitTest(Chart chart, float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        TooltipOptions? options = null, ViewportState? viewport = null);

    /// <summary>
    /// Asynchronous overload of <see cref="HitTest"/>. Respects <paramref name="cancellationToken"/>.
    /// </summary>
    Task<TooltipHitResult> HitTestAsync(Chart chart, float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        TooltipOptions? options = null, ViewportState? viewport = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a zoom operation anchored at the specified canvas point and returns the updated viewport.
    /// A <paramref name="factor"/> greater than 1 zooms in; less than 1 zooms out.
    /// </summary>
    ViewportState Zoom(Chart chart, ViewportState current,
        float anchorX, float anchorY, float canvasWidth, float canvasHeight, double factor);

    /// <summary>
    /// Translates the viewport by a pixel delta and returns the updated viewport.
    /// Positive <paramref name="deltaX"/> pans right; positive <paramref name="deltaY"/> pans down.
    /// </summary>
    ViewportState Pan(Chart chart, ViewportState current,
        float deltaX, float deltaY, float canvasWidth, float canvasHeight);

    /// <summary>Returns a <see cref="ViewportState"/> that shows the full data range of the chart.</summary>
    ViewportState ResetViewport(Chart chart);

    /// <summary>
    /// Computes the data-coordinate range visible through the current viewport,
    /// derived from the chart's actual data bounds combined with zoom and pan offsets.
    /// </summary>
    (double minX, double maxX, double minY, double maxY) GetVisibleRange(Chart chart, ViewportState viewport);

    /// <summary>
    /// Formats tooltip display text for a hit result, substituting values into
    /// the <see cref="TooltipOptions.ContentTemplate"/> when one is provided.
    /// </summary>
    string FormatTooltip(TooltipHitResult hit, TooltipOptions? options = null);
}

/// <summary>
/// Default implementation of <see cref="IInteractivityService"/>.
/// All coordinate maths is performed in canvas-pixel space mapped from the chart's data space
/// using the same margin constants as the renderers.
/// </summary>
public sealed class InteractivityService : IInteractivityService
{
    private readonly ILogger<InteractivityService> _logger;

    /// <summary>Initialises a new <see cref="InteractivityService"/>.</summary>
    /// <param name="logger">Logger used for debug and warning output.</param>
    public InteractivityService(ILogger<InteractivityService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public TooltipHitResult HitTest(Chart chart, float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        TooltipOptions? options = null, ViewportState? viewport = null)
    {
        if (chart == null) throw new ArgumentNullException(nameof(chart));
        if (canvasWidth  <= 0f) throw new ArgumentOutOfRangeException(nameof(canvasWidth));
        if (canvasHeight <= 0f) throw new ArgumentOutOfRangeException(nameof(canvasHeight));

        var opts = options ?? new TooltipOptions();
        if (!opts.Enabled) return TooltipHitResult.Miss;

        var (minX, maxX, minY, maxY) = GetVisibleRange(chart, viewport ?? new ViewportState());
        if (maxX <= minX || maxY <= minY)
        {
            _logger.LogWarning("Chart {ChartId} has degenerate data bounds; skipping hit-test", chart.Id);
            return TooltipHitResult.Miss;
        }

        var plotL = (float)ChartConstants.DefaultMarginLeft;
        var plotT = (float)ChartConstants.DefaultMarginTop;
        var plotR = canvasWidth  - ChartConstants.DefaultMarginRight;
        var plotB = canvasHeight - ChartConstants.DefaultMarginBottom;
        var plotW = plotR - plotL;
        var plotH = plotB - plotT;

        if (plotW <= 0f || plotH <= 0f) return TooltipHitResult.Miss;

        double bestDist = opts.HitRadius;
        TooltipHitResult best = TooltipHitResult.Miss;

        for (int si = 0; si < chart.Series.Count; si++)
        {
            var series = chart.Series[si];
            if (!series.IsVisible || series.DataPoints is not { Count: > 0 }) continue;

            foreach (var dp in series.DataPoints)
            {
                var cx = plotL + (float)((dp.X - minX) / (maxX - minX) * plotW);
                var cy = plotB - (float)((dp.Y - minY) / (maxY - minY) * plotH);
                var dist = Math.Sqrt((pointerX - cx) * (pointerX - cx) + (pointerY - cy) * (pointerY - cy));

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = new TooltipHitResult
                    {
                        IsHit       = true,
                        DataPoint   = dp,
                        Series      = series,
                        SeriesIndex = si,
                        DistancePx  = dist,
                        CanvasX     = cx,
                        CanvasY     = cy
                    };
                }
            }
        }

        if (best.IsHit)
        {
            best.TooltipText = FormatTooltip(best, opts);
            _logger.LogDebug(
                "Hit-test matched series '{Series}' at ({X}, {Y}), dist={Dist:F1}px",
                best.Series!.Name, best.DataPoint!.X, best.DataPoint.Y, best.DistancePx);
        }

        return best;
    }

    /// <inheritdoc />
    public Task<TooltipHitResult> HitTestAsync(Chart chart, float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        TooltipOptions? options = null, ViewportState? viewport = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(HitTest(chart, pointerX, pointerY, canvasWidth, canvasHeight, options, viewport));
    }

    /// <inheritdoc />
    public ViewportState Zoom(Chart chart, ViewportState current,
        float anchorX, float anchorY, float canvasWidth, float canvasHeight, double factor)
    {
        if (chart   == null) throw new ArgumentNullException(nameof(chart));
        if (current == null) throw new ArgumentNullException(nameof(current));
        if (factor  <= 0.0)  throw new ArgumentOutOfRangeException(nameof(factor), "Zoom factor must be positive.");

        var next     = current.Clone();
        var newZoomX = Math.Clamp(current.ZoomX * factor, 0.01, 100.0);
        var newZoomY = Math.Clamp(current.ZoomY * factor, 0.01, 100.0);

        var (dataMinX, dataMaxX, dataMinY, dataMaxY) = _getFullDataBounds(chart);
        var (visMinX,  visMaxX,  visMinY,  visMaxY)  = GetVisibleRange(chart, current);

        var plotL = (float)ChartConstants.DefaultMarginLeft;
        var plotT = (float)ChartConstants.DefaultMarginTop;
        var plotW = canvasWidth  - ChartConstants.DefaultMarginRight  - plotL;
        var plotH = canvasHeight - ChartConstants.DefaultMarginBottom - plotT;

        var anchorDataX = visMinX + ((anchorX - plotL) / plotW) * (visMaxX - visMinX);
        var anchorDataY = visMinY + ((canvasHeight - ChartConstants.DefaultMarginBottom - anchorY) / plotH) * (visMaxY - visMinY);

        var halfW = (dataMaxX - dataMinX) / newZoomX / 2.0;
        var halfH = (dataMaxY - dataMinY) / newZoomY / 2.0;

        next.ZoomX         = newZoomX;
        next.ZoomY         = newZoomY;
        next.PanX          = anchorDataX - halfW - dataMinX;
        next.PanY          = anchorDataY - halfH - dataMinY;
        next.VisibleXRange = (anchorDataX - halfW, anchorDataX + halfW);
        next.VisibleYRange = (anchorDataY - halfH, anchorDataY + halfH);

        _logger.LogDebug("Zoom applied: factor={Factor:F2}, zoomX={ZoomX:F2}, zoomY={ZoomY:F2}",
            factor, next.ZoomX, next.ZoomY);
        return next;
    }

    /// <inheritdoc />
    public ViewportState Pan(Chart chart, ViewportState current,
        float deltaX, float deltaY, float canvasWidth, float canvasHeight)
    {
        if (chart   == null) throw new ArgumentNullException(nameof(chart));
        if (current == null) throw new ArgumentNullException(nameof(current));

        var plotW = canvasWidth  - ChartConstants.DefaultMarginRight  - ChartConstants.DefaultMarginLeft;
        var plotH = canvasHeight - ChartConstants.DefaultMarginBottom - ChartConstants.DefaultMarginTop;

        if (plotW <= 0f || plotH <= 0f) return current.Clone();

        var (visMinX, visMaxX, visMinY, visMaxY) = GetVisibleRange(chart, current);
        var next = current.Clone();

        next.PanX += -(deltaX / plotW) * (visMaxX - visMinX);
        next.PanY +=  (deltaY / plotH) * (visMaxY - visMinY);

        var (dataMinX, dataMaxX, dataMinY, dataMaxY) = _getFullDataBounds(chart);
        var newMinX = dataMinX + next.PanX;
        var newMinY = dataMinY + next.PanY;

        next.VisibleXRange = (newMinX, newMinX + (dataMaxX - dataMinX) / next.ZoomX);
        next.VisibleYRange = (newMinY, newMinY + (dataMaxY - dataMinY) / next.ZoomY);

        _logger.LogDebug("Pan applied: deltaX={DX:F1}, deltaY={DY:F1}", deltaX, deltaY);
        return next;
    }

    /// <inheritdoc />
    public ViewportState ResetViewport(Chart chart)
    {
        if (chart == null) throw new ArgumentNullException(nameof(chart));

        var (minX, maxX, minY, maxY) = _getFullDataBounds(chart);
        var vp = new ViewportState
        {
            VisibleXRange = (minX, maxX),
            VisibleYRange = (minY, maxY)
        };

        _logger.LogInformation("Viewport reset for chart {ChartId}", chart.Id);
        return vp;
    }

    /// <inheritdoc />
    public (double minX, double maxX, double minY, double maxY) GetVisibleRange(
        Chart chart, ViewportState viewport)
    {
        if (chart    == null) throw new ArgumentNullException(nameof(chart));
        if (viewport == null) throw new ArgumentNullException(nameof(viewport));

        if (!viewport.IsDefault
            && viewport.VisibleXRange != default
            && viewport.VisibleYRange != default)
        {
            return (viewport.VisibleXRange.Min, viewport.VisibleXRange.Max,
                    viewport.VisibleYRange.Min, viewport.VisibleYRange.Max);
        }

        var (dataMinX, dataMaxX, dataMinY, dataMaxY) = _getFullDataBounds(chart);

        var visMinX = dataMinX + viewport.PanX;
        var visMinY = dataMinY + viewport.PanY;
        return (visMinX, visMinX + (dataMaxX - dataMinX) / viewport.ZoomX,
                visMinY, visMinY + (dataMaxY - dataMinY) / viewport.ZoomY);
    }

    /// <inheritdoc />
    public string FormatTooltip(TooltipHitResult hit, TooltipOptions? options = null)
    {
        if (hit?.IsHit != true || hit.DataPoint == null) return string.Empty;

        var template = options?.ContentTemplate;
        if (!string.IsNullOrWhiteSpace(template))
        {
            return template
                .Replace("{x}",      hit.DataPoint.X.ToString("G6"))
                .Replace("{y}",      hit.DataPoint.Y.ToString("G6"))
                .Replace("{label}",  hit.DataPoint.Label  ?? string.Empty)
                .Replace("{series}", hit.Series?.Name     ?? string.Empty);
        }

        var label  = string.IsNullOrWhiteSpace(hit.DataPoint.Label) ? string.Empty : $" — {hit.DataPoint.Label}";
        return $"{hit.Series?.Name ?? "Series"}{label}\nX: {hit.DataPoint.X:G6}\nY: {hit.DataPoint.Y:G6}";
    }

    private static (double minX, double maxX, double minY, double maxY) _getFullDataBounds(Chart chart)
    {
        var points = chart.Series
            .Where(s => s.IsVisible && s.DataPoints is { Count: > 0 })
            .SelectMany(s => s.DataPoints)
            .ToList();

        if (points.Count == 0) return (0, 1, 0, 1);

        var minX = points.Min(p => p.X);
        var maxX = points.Max(p => p.X);
        var minY = points.Min(p => p.Y);
        var maxY = points.Max(p => p.Y);

        if (minX == maxX) { minX -= 1; maxX += 1; }
        if (minY == maxY) { minY -= 1; maxY += 1; }

        return (minX, maxX, minY, maxY);
    }
}
