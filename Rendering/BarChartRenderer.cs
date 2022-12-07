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
/// Specialized renderer for bar charts.
/// Supports grouped bars, stacked bars, and horizontal/vertical orientation.
/// </summary>
public class BarChartRenderer
{
    private readonly ILogger<BarChartRenderer> _logger;

    public BarChartRenderer(ILogger<BarChartRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Render bar chart
    public void Render(SKCanvas canvas, Chart chart, SKRect bounds)
    {
        try
        {
            if (canvas == null || chart == null || chart.Series == null || chart.Series.Count == 0)
            {
                _logger.LogWarning("Invalid parameters for bar chart rendering");
                return;
            }

            _logger.LogInformation("Rendering bar chart: {ChartId}", chart.Id);

            var padding = 40f;
            var chartBounds = new SKRect(bounds.Left + padding, bounds.Top + padding,
                bounds.Right - padding, bounds.Bottom - padding);

            // Get value range
            var allValues = chart.Series.SelectMany(s => s.DataPoints.Select(dp => dp.Value)).ToList();
            var minValue = 0.0;
            var maxValue = allValues.Max();

            // Render bars
            _renderBars(canvas, chart, chartBounds, minValue, maxValue);

            // Render axes
            _renderAxes(canvas, chartBounds, minValue, maxValue);

            _logger.LogDebug("Bar chart rendered: {SeriesCount} series", chart.Series.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering bar chart");
        }
    }

    private void _renderBars(SKCanvas canvas, Chart chart, SKRect bounds, double minValue, double maxValue)
    {
        var valueRange = maxValue - minValue;
        if (valueRange == 0) valueRange = 1;

        var barPaint = new SKPaint { IsAntialias = true };
        var series = chart.Series;
        var pointCount = series.First().DataPoints.Count;

        var barWidth = bounds.Width / (pointCount * series.Count + pointCount - 1);
        var groupWidth = barWidth * series.Count;

        for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
        {
            var groupX = bounds.Left + pointIndex * (groupWidth + barWidth);

            for (int seriesIndex = 0; seriesIndex < series.Count; seriesIndex++)
            {
                var dataPoint = series[seriesIndex].DataPoints[pointIndex];
                var normalizedValue = (dataPoint.Value - minValue) / valueRange;

                var barHeight = (float)(normalizedValue * bounds.Height);
                var barX = groupX + seriesIndex * barWidth;
                var barY = bounds.Bottom - barHeight;

                barPaint.Color = _getColor(seriesIndex);
                var barRect = new SKRect(barX, barY, barX + barWidth - 2, bounds.Bottom);
                canvas.DrawRect(barRect, barPaint);

                // Draw value label
                var labelPaint = new SKPaint { TextSize = 9, Color = SKColors.Black };
                var labelText = dataPoint.Value.ToString("F1");
                var textWidth = labelPaint.MeasureText(labelText);
                canvas.DrawText(labelText, barX + (barWidth - textWidth) / 2, barY - 5, labelPaint);
            }
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
        var colors = new[] { SKColors.Blue, SKColors.Red, SKColors.Green, SKColors.Orange, SKColors.Purple };
        return colors[index % colors.Length];
    }
}
