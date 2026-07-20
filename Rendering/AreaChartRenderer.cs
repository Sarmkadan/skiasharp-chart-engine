// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Rendering;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Renderer for area charts. It draws the line exactly like <see cref="LineChartRenderer"/>
/// and then fills the area under the line down to the baseline. The fill opacity can be 17
/// configured via the <c>FillOpacity</c> constant (0‑1 range). Axis and grid drawing are
/// reused from the line renderer to keep visual consistency.
/// </summary>
public class AreaChartRenderer
{
    private readonly ILogger<AreaChartRenderer> _logger;
    private readonly LegendRenderer _legendRenderer;
    private const float MarkerSize = 4f;
    private const float LineWidth = 2f;
    // Opacity of the fill (0 = fully transparent, 1 = fully opaque)
    private const float FillOpacity = 0.3f;

    public AreaChartRenderer(ILogger<AreaChartRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _legendRenderer = new LegendRenderer(logger);
    }

    /// <summary>
    /// Renders an area chart onto the supplied <paramref name="canvas"/>.
    /// </summary>
    /// <param name="canvas">The SkiaSharp canvas to draw on.</param>
    /// <param name="chart">The chart model containing series and data points.</param>
    /// <param name="bounds">The drawing bounds within the canvas.</param>
    public void Render(SKCanvas canvas, Chart chart, SKRect bounds)
    {
        try
        {
            if (canvas == null || chart == null || chart.Series == null || chart.Series.Count == 0)
            {
                _logger.LogWarning("Invalid parameters for area chart rendering");
                return;
            }

            _logger.LogInformation("Rendering area chart: {ChartId}", chart.Id);

            var padding = 40f;
            var chartBounds = new SKRect(
                bounds.Left + padding,
                bounds.Top + padding,
                bounds.Right - padding,
                bounds.Bottom - padding);

            // Determine the overall value range across all series
            var allValues = chart.Series
                .SelectMany(s => s.DataPoints.Select(dp => dp.Value))
                .ToList();

            if (allValues.Count == 0) return;

            var minValue = allValues.Min();
            var maxValue = allValues.Max();
            var valueRange = maxValue - minValue;
            if (valueRange == 0) valueRange = 1;

            // Render each series as a filled area
            var colors = _getSeriesColors(chart.Series.Count);
            for (int seriesIndex = 0; seriesIndex < chart.Series.Count; seriesIndex++)
            {
                var series = chart.Series[seriesIndex];
                if (series.DataPoints.Count < 2) continue;

                _renderArea(canvas, series, chartBounds, minValue, valueRange, colors[seriesIndex]);
            }

            // Render axes (reuse the same logic as the line renderer)
            _renderAxes(canvas, chartBounds, minValue, maxValue);

            // Render legend if series have names
            if (chart.Series.Any(s => !string.IsNullOrWhiteSpace(s.Name)))
            {
                _legendRenderer.Render(canvas, chart, bounds, LegendCorner.TopRight);
            }

            _logger.LogDebug("Area chart rendered: {SeriesCount} series", chart.Series.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering area chart");
        }
    }

    private void _renderArea(
        SKCanvas canvas,
        ChartSeries series,
        SKRect bounds,
        double minValue,
        double valueRange,
        SKColor color)
    {
        // Paint for the line
        using var linePaint = new SKPaint
        {
            Color = color,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = LineWidth,
            IsAntialias = true
        };

        // Paint for the fill (same hue, reduced opacity)
        using var fillPaint = new SKPaint
        {
            Color = color.WithAlpha((byte)(FillOpacity * 255)),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        var points = series.DataPoints;
        var pointWidth = bounds.Width / (points.Count - 1);

        using var path = new SKPath();

        // Start at the baseline under the first point
        var startX = bounds.Left;
        var baselineY = bounds.Bottom;
        path.MoveTo(startX, baselineY);

        // Build the line part of the path
        for (int i = 0; i < points.Count; i++)
        {
            var x = bounds.Left + i * pointWidth;
            var y = bounds.Bottom - (float)((points[i].Value - minValue) / valueRange * bounds.Height);
            path.LineTo(x, y);
        }

        // Close the path back to the baseline under the last point
        var endX = bounds.Left + (points.Count - 1) * pointWidth;
        path.LineTo(endX, baselineY);
        path.Close();

        // Fill the area first, then draw the line on top
        canvas.DrawPath(path, fillPaint);
        canvas.DrawPath(path, linePaint);

        // Draw markers (same as line renderer)
        using var markerPaint = new SKPaint
        {
            Color = color,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        for (int i = 0; i < points.Count; i++)
        {
            var x = bounds.Left + i * pointWidth;
            var y = bounds.Bottom - (float)((points[i].Value - minValue) / valueRange * bounds.Height);
            canvas.DrawCircle(x, y, MarkerSize, markerPaint);
        }
    }

    private void _renderAxes(SKCanvas canvas, SKRect bounds, double minValue, double maxValue)
    {
        using var axisPaint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 1,
            IsAntialias = true
        };

        // X axis
        canvas.DrawLine(bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom, axisPaint);

        // Y axis
        canvas.DrawLine(bounds.Left, bounds.Top, bounds.Left, bounds.Bottom, axisPaint);

        // Y axis labels (reuse the same logic as the line renderer)
        using var textPaint = new SKPaint { TextSize = 10, Color = SKColors.Black };
        for (int i = 0; i <= 5; i++)
        {
            var value = minValue + (maxValue - minValue) * i / 5;
            var y = bounds.Bottom - (float)(i / 5.0 * bounds.Height);
            var format = Math.Abs(maxValue - minValue) < 1 ? "F1" : "F0";
            canvas.DrawText(value.ToString(format), bounds.Left - 35, y + 3, textPaint);
        }
    }

    private SKColor[] _getSeriesColors(int count)
    {
        var colors = new SKColor[count];
        var baseColors = new[] { SKColors.Blue, SKColors.Red, SKColors.Green, SKColors.Orange, SKColors.Purple };

        for (int i = 0; i < count; i++)
        {
            colors[i] = baseColors[i % baseColors.Length];
        }

        return colors;
    }
}