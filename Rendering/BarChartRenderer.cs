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
/// Specialized renderer for bar charts.
/// Supports grouped bars, stacked bars, and horizontal/vertical orientation.
/// </summary>
public class BarChartRenderer : IChartRenderer
{
    private readonly ILogger<BarChartRenderer> _logger;
    private readonly LegendRenderer _legendRenderer;

    /// <summary>
    /// When true, bars are rendered in stacked mode (cumulative per category).
    /// </summary>
    public bool Stacked { get; set; } = false;

    public BarChartRenderer(ILogger<BarChartRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _legendRenderer = new LegendRenderer(logger);
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

            // Calculate title and subtitle height to shrink the plot area
            float titleHeight = 0;
            if (!string.IsNullOrEmpty(chart.Configuration.Title))
            {
                titleHeight += ChartConstants.TitleFontSize + 8f;
            }
            if (!string.IsNullOrEmpty(chart.Configuration.Subtitle))
            {
                titleHeight += ChartConstants.SubtitleFontSize + 6f;
            }

            var padding = 40f;
            var chartBounds = new SKRect(
                bounds.Left + padding,
                bounds.Top + padding + titleHeight,
                bounds.Right - padding,
                bounds.Bottom - padding
            );

            // Get value range
            var allValues = chart.Series.SelectMany(s => s.DataPoints.Select(dp => dp.Value)).ToList();

            double minValue = 0.0;
            double maxValue;

            if (Stacked)
            {
                // Compute stacked totals per category (point index)
                var pointCount = chart.Series.First().DataPoints.Count;
                var stackedSums = new double[pointCount];
                foreach (var series in chart.Series)
                {
                    for (int i = 0; i < pointCount; i++)
                    {
                        stackedSums[i] += series.DataPoints[i].Value;
                    }
                }

                maxValue = stackedSums.Max();
            }
            else
            {
                maxValue = allValues.Max();
            }

            // Render title and subtitle
            _renderTitleAndSubtitle(canvas, chart, bounds);

            // Render bars
            _renderBars(canvas, chart, chartBounds, minValue, maxValue);

            // Render axes
            _renderAxes(canvas, chartBounds, minValue, maxValue);

            // Render legend if series have names
            if (chart.Series.Any(s => !string.IsNullOrWhiteSpace(s.Name)))
            {
                _legendRenderer.Render(canvas, chart, bounds, LegendCorner.TopRight);
            }

            _logger.LogDebug("Bar chart rendered: {SeriesCount} series", chart.Series.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering bar chart");
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

    private void _renderBars(SKCanvas canvas, Chart chart, SKRect bounds, double minValue, double maxValue)
    {
        var valueRange = maxValue - minValue;
        if (valueRange == 0) valueRange = 1;

        var barPaint = new SKPaint { IsAntialias = true };
        var series = chart.Series;
        var pointCount = series.First().DataPoints.Count;

        // Determine bar width based on mode
        float barWidth;
        if (Stacked)
        {
            // One bar per category
            barWidth = bounds.Width / pointCount;
        }
        else
        {
            // Grouped bars: space between groups + bars inside group
            barWidth = bounds.Width / (pointCount * series.Count + pointCount - 1);
        }

        // Width of a full group (used only for grouped mode)
        var groupWidth = barWidth * series.Count;

        for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
        {
            if (Stacked)
            {
                // Stacked mode: single bar per category, cumulative heights
                float cumulativeHeight = 0f;
                var barX = bounds.Left + pointIndex * barWidth;

                for (int seriesIndex = 0; seriesIndex < series.Count; seriesIndex++)
                {
                    var dataPoint = series[seriesIndex].DataPoints[pointIndex];
                    var normalizedValue = (dataPoint.Value - minValue) / valueRange;
                    var barHeight = (float)(normalizedValue * bounds.Height);

                    var barY = bounds.Bottom - cumulativeHeight - barHeight;
                    var barRect = new SKRect(barX, barY, barX + barWidth - 2, bounds.Bottom - cumulativeHeight);

                    barPaint.Color = _getColor(seriesIndex);
                    canvas.DrawRect(barRect, barPaint);

                    // Draw value label on top of each segment
                    var labelPaint = new SKPaint { TextSize = 9, Color = SKColors.Black };
                    var labelText = dataPoint.Value.ToString("F1");
                    var textWidth = labelPaint.MeasureText(labelText);
                    canvas.DrawText(labelText,
                        barX + (barWidth - textWidth) / 2,
                        barY - 5,
                        labelPaint);

                    cumulativeHeight += barHeight;
                }
            }
            else
            {
                // Grouped mode (original behaviour)
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
