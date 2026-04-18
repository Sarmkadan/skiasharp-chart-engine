// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Specialized renderer for line charts.
/// Renders smooth or straight lines connecting data points with optional markers.
/// </summary>
public class LineChartRenderer
{
    private readonly ILogger<LineChartRenderer> _logger;
    private const float MarkerSize = 4f;
    private const float LineWidth = 2f;

    public LineChartRenderer(ILogger<LineChartRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Render line chart
    public void Render(SKCanvas canvas, Chart chart, SKRect bounds)
    {
        try
        {
            if (canvas == null || chart == null || chart.Series == null || chart.Series.Count == 0)
            {
                _logger.LogWarning("Invalid parameters for line chart rendering");
                return;
            }

            _logger.LogInformation("Rendering line chart: {ChartId}", chart.Id);

            var padding = 40f;
            var chartBounds = new SKRect(bounds.Left + padding, bounds.Top + padding,
                bounds.Right - padding, bounds.Bottom - padding);

            // Get value range across all series
            var allValues = chart.Series.SelectMany(s => s.DataPoints.Select(dp => dp.Value)).ToList();
            if (allValues.Count == 0) return;

            var minValue = allValues.Min();
            var maxValue = allValues.Max();
            var valueRange = maxValue - minValue;
            if (valueRange == 0) valueRange = 1;

            // Render each series
            var colors = _getSeriesColors(chart.Series.Count);
            for (int seriesIndex = 0; seriesIndex < chart.Series.Count; seriesIndex++)
            {
                var series = chart.Series[seriesIndex];
                if (series.DataPoints.Count < 2) continue;

                _renderLine(canvas, series, chartBounds, minValue, valueRange, colors[seriesIndex]);
            }

            // Render axes
            _renderAxes(canvas, chartBounds, minValue, maxValue);

            _logger.LogDebug("Line chart rendered: {SeriesCount} series", chart.Series.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering line chart");
        }
    }

    private void _renderLine(SKCanvas canvas, ChartSeries series, SKRect bounds,
        double minValue, double valueRange, SKColor color)
    {
        using var paint = new SKPaint
        {
            Color = color,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = LineWidth,
            IsAntialias = true
        };

        var points = series.DataPoints;
        var pointWidth = bounds.Width / (points.Count - 1);

        using var path = new SKPath();
        var x0 = bounds.Left;
        var y0 = bounds.Bottom - (float)((points[0].Value - minValue) / valueRange * bounds.Height);
        path.MoveTo(x0, y0);

        for (int i = 1; i < points.Count; i++)
        {
            var x = bounds.Left + i * pointWidth;
            var y = bounds.Bottom - (float)((points[i].Value - minValue) / valueRange * bounds.Height);
            path.LineTo(x, y);
        }

        canvas.DrawPath(path, paint);

        // Draw markers
        paint.Style = SKPaintStyle.Fill;
        for (int i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var x = bounds.Left + i * pointWidth;
            var y = bounds.Bottom - (float)((point.Value - minValue) / valueRange * bounds.Height);

            canvas.DrawCircle(x, y, MarkerSize, paint);
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

        // Y axis labels
        using var textPaint = new SKPaint { TextSize = 10, Color = SKColors.Black };
        for (int i = 0; i <= 5; i++)
        {
            var value = minValue + (maxValue - minValue) * i / 5;
            var y = bounds.Bottom - (float)(i / 5.0 * bounds.Height);
            // Hotfix: Use dynamic decimal places based on value range to prevent label overlap
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
