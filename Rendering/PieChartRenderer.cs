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
/// Specialized renderer for pie charts.
/// Renders donut and pie charts with labels, legends, and optional 3D effect.
/// </summary>
public class PieChartRenderer : IChartRenderer
{
    private readonly ILogger<PieChartRenderer> _logger;
    private const float LabelDistance = 50f;

    public PieChartRenderer(ILogger<PieChartRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Render pie chart
    public void Render(SKCanvas canvas, Chart chart, SKRect bounds, float innerRadiusRatio = 0f)
    {
        if (innerRadiusRatio < 0 || innerRadiusRatio > 0.9f)
        {
            throw new ArgumentOutOfRangeException(nameof(innerRadiusRatio), "Inner radius ratio must be between 0 and 0.9");
        }

        try
        {
            if (canvas == null || chart == null || chart.Series == null || chart.Series.Count == 0)
            {
                _logger.LogWarning("Invalid parameters for pie chart rendering");
                return;
            }

            _logger.LogInformation("Rendering pie chart: {ChartId}", chart.Id);

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

            // Use the first series for the pie chart (mirrors PieChartRenderer behaviour)
            var series = chart.Series.FirstOrDefault();
            if (series?.DataPoints == null || series.DataPoints.Count == 0)
                return;

            var dataPoints = series.DataPoints;
            var total = dataPoints.Sum(dp => dp.Value);

            // Render title and subtitle
            _renderTitleAndSubtitle(canvas, chart, bounds);

            // Calculate center and radius
            var centerX = chartBounds.MidX;
            var centerY = chartBounds.MidY;
            var radius = Math.Min(chartBounds.Width, chartBounds.Height) / 3;
            var innerRadius = radius * innerRadiusRatio;

            // Render slices
            using var paint = new SKPaint { IsAntialias = true };
            float currentAngle = -90f; // Start from top

            // A tiny overlap applied to each slice's sweep angle closes the sub-pixel
            // gaps that appear at segment boundaries on high-DPI / Retina displays due
            // to floating-point rounding in the arc sweep calculation.
            const float sweepOverlap = 0.5f;

            for (int i = 0; i < dataPoints.Count; i++)
            {
                var dataPoint = dataPoints[i];
                var sliceAngle = (float)(dataPoint.Value / total * 360);

                paint.Color = _getColor(i);
                paint.Style = SKPaintStyle.Fill;

                // Draw slice with a slight overlap so rounding gaps are not visible
                var rect = new SKRect(centerX - radius, centerY - radius,
                    centerX + radius, centerY + radius);
                var path = new SKPath();
                path.AddArc(rect, currentAngle, sliceAngle + sweepOverlap);
                if (innerRadiusRatio > 0)
                {
                    var innerRect = new SKRect(centerX - innerRadius, centerY - innerRadius,
                        centerX + innerRadius, centerY + innerRadius);
                    path.AddArc(innerRect, currentAngle + sliceAngle + sweepOverlap, -sliceAngle - sweepOverlap);
                }
                path.Close();
                canvas.DrawPath(path, paint);

                // Draw label
                var labelAngle = currentAngle + sliceAngle / 2;
                var labelRad = labelAngle * (float)Math.PI / 180;
                var labelX = centerX + (float)(Math.Cos(labelRad) * (radius + LabelDistance));
                var labelY = centerY + (float)(Math.Sin(labelRad) * (radius + LabelDistance));

                var percentage = (dataPoint.Value / total * 100).ToString("F1") + "%";
                using var textPaint = new SKPaint { TextSize = 10, Color = SKColors.Black };
                var textWidth = textPaint.MeasureText(percentage);
                canvas.DrawText(percentage, labelX - textWidth / 2, labelY + 3, textPaint);

                currentAngle += sliceAngle;
            }

            _logger.LogDebug("Pie chart rendered: {PointCount} slices", dataPoints.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering pie chart");
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

    private SKColor _getColor(int index)
    {
        var colors = new[]
        {
            SKColors.RoyalBlue, SKColors.OrangeRed, SKColors.ForestGreen, SKColors.Gold,
            SKColors.Violet, SKColors.Turquoise, SKColors.Salmon, SKColors.SeaGreen
        };

        return colors[index % colors.Length];
    }
}
