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
/// Specialized renderer for gauge charts.
/// Renders semicircular gauges with colored zones, needle indicator, and value label.
/// </summary>
public class GaugeChartRenderer
{
    private readonly ILogger<GaugeChartRenderer> _logger;
    private const float LabelDistance = 30f;
    private const float NeedleLengthRatio = 0.8f;
    private const float NeedleWidth = 4f;
    private const float ZoneBorderWidth = 8f;
    private const float TickLength = 10f;
    private const float TickWidth = 1.5f;

    public GaugeChartRenderer(ILogger<GaugeChartRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Renders a semicircular gauge chart onto the supplied canvas.
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
                _logger.LogWarning("Invalid parameters for gauge chart rendering");
                return;
            }

            _logger.LogInformation("Rendering gauge chart: {ChartId}", chart.Id);

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

            // Adjust bounds to account for title/subtitle
            var gaugeBounds = new SKRect(bounds.Left, bounds.Top + titleHeight, bounds.Right, bounds.Bottom);

            // Use the first series for the gauge chart (mirrors PieChartRenderer behaviour)
            var series = chart.Series.FirstOrDefault();
            if (series?.DataPoints == null || series.DataPoints.Count == 0)
                return;

            var dataPoint = series.DataPoints.First();
            double currentValue = dataPoint.Value;

            // Get min/max from series configuration or use defaults
            double minValue = series.YAxisMin ?? 0;
            double maxValue = series.YAxisMax ?? 100;

            if (minValue >= maxValue)
            {
                _logger.LogWarning("Invalid gauge range: minValue ({Min}) must be less than maxValue ({Max})", minValue, maxValue);
                return;
            }

            // Normalize value to [0, 1] range
            float normalizedValue = (float)((currentValue - minValue) / (maxValue - minValue));
            normalizedValue = Math.Max(0, Math.Min(1, normalizedValue));

            // Calculate center and radius
            var centerX = gaugeBounds.MidX;
            var centerY = gaugeBounds.Bottom - 50f; // Position gauge at bottom with some padding
            var radius = Math.Min(gaugeBounds.Width, gaugeBounds.Height) / 2.5f;

            // Draw background
            using var bgPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };
            canvas.DrawRect(gaugeBounds, bgPaint);

            // Draw title and subtitle
            _renderTitleAndSubtitle(canvas, chart, bounds);

            // Draw gauge zones (colored background bands)
            _drawGaugeZones(canvas, centerX, centerY, radius, minValue, maxValue);

            // Draw gauge arc (semicircle)
            _drawGaugeArc(canvas, centerX, centerY, radius, minValue, maxValue);

            // Draw needle
            _drawNeedle(canvas, centerX, centerY, radius, normalizedValue);

            // Draw value label
            _drawValueLabel(canvas, centerX, centerY, radius, currentValue, minValue, maxValue);

            // Draw min/max labels
            _drawMinMaxLabels(canvas, centerX, centerY, radius, minValue, maxValue);

            _logger.LogDebug("Gauge chart rendered: value={Value}, range=[{Min}-{Max}]", currentValue, minValue, maxValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering gauge chart");
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

    private void _drawGaugeZones(SKCanvas canvas, float centerX, float centerY, float radius, double minValue, double maxValue)
    {
        // Define zones: red (danger), yellow (warning), green (good)
        // Each zone spans 1/3 of the range
        float zone1End = 0.33f; // Red zone: 0-33%
        float zone2End = 0.66f; // Yellow zone: 33-66%
        float zone3End = 1.0f; // Green zone: 66-100%

        // Zone 1: Red (0-33%)
        using var zone1Paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = ZoneBorderWidth };
        zone1Paint.Color = new SKColor(220, 80, 80, 180); // Semi-transparent red
        _drawArcSegment(canvas, centerX, centerY, radius, -180f, -180f + zone1End * 180f, zone1Paint);

        // Zone 2: Yellow (33-66%)
        using var zone2Paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = ZoneBorderWidth };
        zone2Paint.Color = new SKColor(255, 200, 80, 180); // Semi-transparent yellow
        _drawArcSegment(canvas, centerX, centerY, radius, -180f + zone1End * 180f, -180f + zone2End * 180f, zone2Paint);

        // Zone 3: Green (66-100%)
        using var zone3Paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = ZoneBorderWidth };
        zone3Paint.Color = new SKColor(100, 200, 100, 180); // Semi-transparent green
        _drawArcSegment(canvas, centerX, centerY, radius, -180f + zone2End * 180f, -180f + zone3End * 180f, zone3Paint);
    }

    private void _drawGaugeArc(SKCanvas canvas, float centerX, float centerY, float radius, double minValue, double maxValue)
    {
        // Draw the gauge arc outline (semicircle)
        using var arcPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3f,
            Color = SKColors.DimGray
        };

        // Draw semicircle from -180 to 0 degrees (left to right)
        var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
        canvas.DrawArc(rect, -180f, 180f, false, arcPaint);
    }

    private void _drawNeedle(SKCanvas canvas, float centerX, float centerY, float radius, float normalizedValue)
    {
        // Calculate needle angle (-180 to 0 degrees)
        float needleAngle = -180f + normalizedValue * 180f;
        float needleRad = needleAngle * (float)Math.PI / 180;

        // Calculate needle tip position
        float needleTipX = centerX + (float)Math.Cos(needleRad) * radius * NeedleLengthRatio;
        float needleTipY = centerY + (float)Math.Sin(needleRad) * radius * NeedleLengthRatio;

        // Draw needle shaft
        using var needlePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = NeedleWidth,
            Color = SKColors.DarkSlateGray
        };

        canvas.DrawLine(centerX, centerY, needleTipX, needleTipY, needlePaint);

        // Draw needle tip (filled circle)
        using var tipPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = SKColors.Red };
        canvas.DrawCircle(needleTipX, needleTipY, NeedleWidth * 1.5f, tipPaint);
    }

    private void _drawValueLabel(SKCanvas canvas, float centerX, float centerY, float radius, double currentValue, double minValue, double maxValue)
    {
        // Format value based on range
        string format = Math.Abs(maxValue - minValue) < 1 ? "F2" : (Math.Abs(maxValue - minValue) < 10 ? "F1" : "F0");
        string valueText = currentValue.ToString(format);

        using var textPaint = new SKPaint
        {
            TextSize = 24f,
            Color = SKColors.Black,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        var textBounds = new SKRect();
        textPaint.MeasureText(valueText, ref textBounds);

        // Center the text
        float textX = centerX - textBounds.MidX;
        float textY = centerY + radius * 0.3f;

        canvas.DrawText(valueText, textX, textY, textPaint);
    }

    private void _drawMinMaxLabels(SKCanvas canvas, float centerX, float centerY, float radius, double minValue, double maxValue)
    {
        using var labelPaint = new SKPaint
        {
            TextSize = 12f,
            Color = SKColors.DimGray,
            IsAntialias = true
        };

        // Draw min value at left
        var minText = minValue.ToString("F0");
        var minBounds = new SKRect();
        labelPaint.MeasureText(minText, ref minBounds);
        canvas.DrawText(minText, centerX - radius - 10f - minBounds.Width, centerY, labelPaint);

        // Draw max value at right
        var maxText = maxValue.ToString("F0");
        var maxBounds = new SKRect();
        labelPaint.MeasureText(maxText, ref maxBounds);
        canvas.DrawText(maxText, centerX + radius + 10f, centerY, labelPaint);
    }

    private void _drawArcSegment(SKCanvas canvas, float centerX, float centerY, float radius, float startAngle, float sweepAngle, SKPaint paint)
    {
        var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
        canvas.DrawArc(rect, startAngle, sweepAngle, false, paint);
    }
}
