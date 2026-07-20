using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Specialized renderer for bubble charts.
/// Each data point is rendered as a circle whose radius is derived from a size value
/// using a square‑root scaling. The fill is semi‑transparent to allow overlapping
/// bubbles to be visible.
/// </summary>
public class BubbleChartRenderer
{
    private readonly ILogger<BubbleChartRenderer> _logger;
    private const float MaxRadius = 20f; // maximum radius for the largest bubble

    public BubbleChartRenderer(ILogger<BubbleChartRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Renders a bubble chart onto the supplied <paramref name="canvas"/>.
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
                _logger.LogWarning("Invalid parameters for bubble chart rendering");
                return;
            }

            _logger.LogInformation("Rendering bubble chart: {ChartId}", chart.Id);

            var padding = 40f;
            var chartBounds = new SKRect(
                bounds.Left + padding,
                bounds.Top + padding,
                bounds.Right - padding,
                bounds.Bottom - padding);

            // Determine the overall value range across all series for Y axis
            var allValues = chart.Series
                .SelectMany(s => s.DataPoints.Select(dp => dp.Value))
                .ToList();

            if (allValues.Count == 0) return;

            var minValue = allValues.Min();
            var maxValue = allValues.Max();
            var valueRange = maxValue - minValue;
            if (valueRange == 0) valueRange = 1;

            // Determine the maximum size value for radius scaling
            var maxSize = allValues.Max(); // using the same value set for simplicity

            // Render each series
            var colors = _getSeriesColors(chart.Series.Count);
            int seriesIndex = 0;
            foreach (var series in chart.Series)
            {
                var color = colors[seriesIndex++];
                foreach (var dataPoint in series.DataPoints)
                {
                    // Normalise Y value
                    var yNorm = (dataPoint.Value - minValue) / valueRange;

                    // X position derived from label hash (simplified)
                    var xNorm = (dataPoint.Label?.GetHashCode() ?? 0) % 100 / 100.0;

                    var x = chartBounds.Left + (float)(xNorm * chartBounds.Width);
                    var y = chartBounds.Bottom - (float)(yNorm * chartBounds.Height);

                    // Radius scaling using sqrt
                    var radius = Math.Sqrt(dataPoint.Value / maxSize) * MaxRadius;

                    // Paint for the bubble
                    using var paint = new SKPaint
                    {
                        Color = color.WithAlpha(128), // semi‑transparent fill
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    };

                    canvas.DrawCircle(x, y, radius, paint);
                }
            }

            // Render axes
            _renderAxes(canvas, chartBounds, minValue, maxValue);

            _logger.LogDebug("Bubble chart rendered: {SeriesCount} series", chart.Series.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering bubble chart");
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
            canvas.DrawText(value.ToString("F0"), bounds.Left - 35, y + 3, textPaint);
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
