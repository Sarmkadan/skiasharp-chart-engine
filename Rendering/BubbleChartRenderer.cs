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
public class BubbleChartRenderer : IChartRenderer
{
    private readonly ILogger<BubbleChartRenderer> _logger;
    private const float MaxRadius = 20f; // maximum radius for the largest bubble
    private const int EmptyItemCount = 0; // item count indicating that there is no data to render
    private const float InitialTitleHeight = 0f; // title area height before title content is measured
    private const float TitleSpacing = 8f; // vertical spacing reserved below the chart title
    private const float SubtitleSpacing = 6f; // vertical spacing reserved below the chart subtitle
    private const float ChartPadding = 40f; // padding between the chart bounds and plot area
    private const double DefaultValueRange = 1d; // fallback range used when all values are equal
    private const int InitialSeriesIndex = 0; // starting index used to select series colors
    private const int MissingLabelHash = 0; // hash value used when a data point has no label
    private const int LabelHashScale = 100; // scale used to normalize label hashes into X positions
    private const byte BubbleFillAlpha = 128; // alpha value used for semi-transparent bubble fills

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
            if (canvas == null || chart == null || chart.Series == null || chart.Series.Count == EmptyItemCount)
            {
                _logger.LogWarning("Invalid parameters for bubble chart rendering");
                return;
            }

            _logger.LogInformation("Rendering bubble chart: {ChartId}", chart.Id);

            // Calculate title and subtitle height to shrink the plot area
            float titleHeight = InitialTitleHeight;
            if (!string.IsNullOrEmpty(chart.Configuration.Title))
            {
                titleHeight += ChartConstants.TitleFontSize + TitleSpacing;
            }
            if (!string.IsNullOrEmpty(chart.Configuration.Subtitle))
            {
                titleHeight += ChartConstants.SubtitleFontSize + SubtitleSpacing;
            }

            var chartBounds = new SKRect(
                bounds.Left + ChartPadding,
                bounds.Top + ChartPadding + titleHeight,
                bounds.Right - ChartPadding,
                bounds.Bottom - ChartPadding
            );

            // Determine the overall value range across all series for Y axis
            var allValues = chart.Series
                .SelectMany(s => s.DataPoints.Select(dp => dp.Value))
                .ToList();

            if (allValues.Count == EmptyItemCount) return;

            var minValue = allValues.Min();
            var maxValue = allValues.Max();
            var valueRange = maxValue - minValue;
            if (valueRange == EmptyItemCount) valueRange = DefaultValueRange;

            // Determine the maximum size value for radius scaling
            var maxSize = allValues.Max(); // using the same value set for simplicity

            // Render title and subtitle
            _renderTitleAndSubtitle(canvas, chart, bounds);

            // Render each series
            var colors = _getSeriesColors(chart.Series.Count);
            int seriesIndex = InitialSeriesIndex;
            foreach (var series in chart.Series)
            {
                var color = colors[seriesIndex++];
                foreach (var dataPoint in series.DataPoints)
                {
                    // Normalise Y value
                    var yNorm = (dataPoint.Value - minValue) / valueRange;

                    // X position derived from label hash (simplified)
                    var xNorm = (dataPoint.Label?.GetHashCode() ?? MissingLabelHash) % LabelHashScale / (double)LabelHashScale;

                    var x = chartBounds.Left + (float)(xNorm * chartBounds.Width);
                    var y = chartBounds.Bottom - (float)(yNorm * chartBounds.Height);

                    // Radius scaling using sqrt
                    var radius = Math.Sqrt(dataPoint.Value / maxSize) * MaxRadius;

                    // Paint for the bubble
                    using var paint = new SKPaint
                    {
                        Color = color.WithAlpha(BubbleFillAlpha), // semi‑transparent fill
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

    private void _renderTitleAndSubtitle(SKCanvas canvas, Chart chart, SKRect bounds)
    {
        if (string.IsNullOrEmpty(chart.Configuration.Title) && string.IsNullOrEmpty(chart.Configuration.Subtitle))
            return;

        var centerX = bounds.MidX;
        var textColor = SKColor.Parse(chart.Configuration.TextColor);
        var titleY = bounds.Top + 4f;

        // Render title
        if (!string.IsNullOrEmpty(chart.Configuration.Title))
        {
            using var titlePaint = new SKPaint
            {
                Color = textColor,
                TextSize = ChartConstants.TitleFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                FakeBoldText = true
            };

            canvas.DrawText(chart.Configuration.Title, centerX, titleY, titlePaint);
        }

        // Render subtitle
        if (!string.IsNullOrEmpty(chart.Configuration.Subtitle))
        {
            var subtitleY = titleY + ChartConstants.TitleFontSize + 2f;
            using var subtitlePaint = new SKPaint
            {
                Color = textColor,
                TextSize = ChartConstants.SubtitleFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            canvas.DrawText(chart.Configuration.Subtitle, centerX, subtitleY, subtitlePaint);
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
