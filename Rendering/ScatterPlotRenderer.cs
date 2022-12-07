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
/// Specialized renderer for scatter plots.
/// Renders points with optional size/color encoding of third dimension.
/// </summary>
public class ScatterPlotRenderer
{
    private readonly ILogger<ScatterPlotRenderer> _logger;
    private const float PointSize = 5f;

    public ScatterPlotRenderer(ILogger<ScatterPlotRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Render scatter plot
    public void Render(SKCanvas canvas, Chart chart, SKRect bounds)
    {
        try
        {
            if (canvas == null || chart == null || chart.Series == null || chart.Series.Count == 0)
            {
                _logger.LogWarning("Invalid parameters for scatter plot rendering");
                return;
            }

            _logger.LogInformation("Rendering scatter plot: {ChartId}", chart.Id);

            var padding = 40f;
            var chartBounds = new SKRect(bounds.Left + padding, bounds.Top + padding,
                bounds.Right - padding, bounds.Bottom - padding);

            // Get value ranges
            var allValues = chart.Series.SelectMany(s => s.DataPoints.Select(dp => dp.Value)).ToList();
            var minValue = allValues.Min();
            var maxValue = allValues.Max();
            var valueRange = maxValue - minValue;
            if (valueRange == 0) valueRange = 1;

            // Render points
            var paint = new SKPaint { IsAntialias = true };
            int pointIndex = 0;

            foreach (var series in chart.Series)
            {
                paint.Color = _getColor(pointIndex++);

                foreach (var dataPoint in series.DataPoints)
                {
                    var normalized = (dataPoint.Value - minValue) / valueRange;

                    // For scatter plot, use label as x position (simplified)
                    var xNorm = (dataPoint.Label?.GetHashCode() ?? 0) % 100 / 100.0;
                    var yNorm = normalized;

                    var x = chartBounds.Left + (float)(xNorm * chartBounds.Width);
                    var y = chartBounds.Bottom - (float)(yNorm * chartBounds.Height);

                    canvas.DrawCircle(x, y, PointSize, paint);
                }
            }

            // Render axes
            _renderAxes(canvas, chartBounds, minValue, maxValue);

            _logger.LogDebug("Scatter plot rendered: {SeriesCount} series", chart.Series.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering scatter plot");
        }
    }

    private void _renderAxes(SKCanvas canvas, SKRect bounds, double minValue, double maxValue)
    {
        var axisPaint = new SKPaint
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
        var textPaint = new SKPaint { TextSize = 10, Color = SKColors.Black };
        for (int i = 0; i <= 5; i++)
        {
            var value = minValue + (maxValue - minValue) * i / 5;
            var y = bounds.Bottom - (float)(i / 5.0 * bounds.Height);
            canvas.DrawText(value.ToString("F0"), bounds.Left - 35, y + 3, textPaint);
        }
    }

    private SKColor _getColor(int index)
    {
        var colors = new[]
        {
            SKColors.Red, SKColors.Blue, SKColors.Green, SKColors.Orange,
            SKColors.Purple, SKColors.Brown, SKColors.Pink, SKColors.Cyan
        };

        return colors[index % colors.Length];
    }
}
