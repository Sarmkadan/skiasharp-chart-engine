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
public class ScatterPlotRenderer : IChartRenderer
{
    private readonly ILogger<ScatterPlotRenderer> _logger;
    private const float PointSize = 5f;

    /// <summary>Padding in pixels between chart bounds and the drawing area on each side.</summary>
    private const float ChartPadding = 40f;

    /// <summary>Stroke width in pixels for the X and Y axis lines.</summary>
    private const float AxisStrokeWidth = 1f;

    /// <summary>Font size in pixels for the Y-axis value labels.</summary>
    private const float AxisLabelTextSize = 10f;

    /// <summary>Number of evenly spaced divisions along the Y axis for value labels.</summary>
    private const int YAxisDivisions = 5;

    /// <summary>Horizontal offset in pixels from the Y axis to draw value labels (to the left).</summary>
    private const float YAxisLabelLeftOffset = 35f;

    /// <summary>Vertical offset in pixels applied to Y-axis labels for baseline alignment.</summary>
    private const float YAxisLabelBaselineOffset = 3f;

    /// <summary>Modulus applied to label hash codes to normalize X positions into a 0-1 range.</summary>
    private const int LabelHashModulus = 100;

    public ScatterPlotRenderer(ILogger<ScatterPlotRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Render scatter plot
    public void Render(SKCanvas canvas, Chart chart, SKRect bounds)
    {
        try
        {
            if (canvas == null || chart == null)
            {
                _logger.LogWarning("Invalid parameters for scatter plot rendering");
                return;
            }

            _logger.LogInformation("Rendering scatter plot: {ChartId}", chart.Id);

            var chartBounds = new SKRect(bounds.Left + ChartPadding, bounds.Top + ChartPadding,
                bounds.Right - ChartPadding, bounds.Bottom - ChartPadding);

            var seriesList = chart.Series ?? new List<ChartSeries>();

            // Get value ranges; with no points at all, fall back to a unit range so only axes are drawn
            var allValues = seriesList
                .Where(s => s.DataPoints != null)
                .SelectMany(s => s.DataPoints.Select(dp => dp.Value))
                .ToList();
            var minValue = allValues.Count > 0 ? allValues.Min() : 0d;
            var maxValue = allValues.Count > 0 ? allValues.Max() : 1d;

            // A single value (zero span) would collapse the axis; widen it around the value
            if (maxValue - minValue == 0)
            {
                minValue -= 1;
                maxValue += 1;
            }
            var valueRange = maxValue - minValue;

            // Render points using batched SKPath for each series
            var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
            int seriesIndex = 0;

            foreach (var series in seriesList)
            {
                paint.Color = _getColor(seriesIndex++);
                using var seriesPath = new SKPath();

                foreach (var dataPoint in series.DataPoints ?? new List<DataPoint>())
                {
                    var normalized = (dataPoint.Value - minValue) / valueRange;

                    // For scatter plot, use label as x position (simplified)
                    var xNorm = (dataPoint.Label?.GetHashCode() ?? 0) % LabelHashModulus / (double)LabelHashModulus;
                    var yNorm = normalized;

                    var x = chartBounds.Left + (float)(xNorm * chartBounds.Width);
                    var y = chartBounds.Bottom - (float)(yNorm * chartBounds.Height);

                    seriesPath.AddCircle(x, y, PointSize);
                }

                canvas.DrawPath(seriesPath, paint);
            }

            // Render axes
            _renderAxes(canvas, chartBounds, minValue, maxValue);

            _logger.LogDebug("Scatter plot rendered: {SeriesCount} series", seriesList.Count);
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
            StrokeWidth = AxisStrokeWidth,
            IsAntialias = true
        };

        // X axis
        canvas.DrawLine(bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom, axisPaint);

        // Y axis
        canvas.DrawLine(bounds.Left, bounds.Top, bounds.Left, bounds.Bottom, axisPaint);

        // Y axis labels
        var textPaint = new SKPaint { TextSize = AxisLabelTextSize, Color = SKColors.Black };
        for (int i = 0; i <= YAxisDivisions; i++)
        {
            var value = minValue + (maxValue - minValue) * i / YAxisDivisions;
            var y = bounds.Bottom - (float)((double)i / YAxisDivisions * bounds.Height);
            canvas.DrawText(value.ToString("F0"), bounds.Left - YAxisLabelLeftOffset, y + YAxisLabelBaselineOffset, textPaint);
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
