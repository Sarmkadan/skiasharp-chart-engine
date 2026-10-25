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
public class GaugeChartRenderer : IChartRenderer
{
    private readonly ILogger<GaugeChartRenderer> _logger;
    private const float LabelDistance = 30f;
    private const float NeedleLengthRatio = 0.8f;
    private const float NeedleWidth = 4f;
    private const float ZoneBorderWidth = 8f;
    private const float TickLength = 10f;
    private const float TickWidth = 1.5f;

    // Value range used when the series does not define YAxisMin / YAxisMax
    private const double DefaultMinValue = 0;
    private const double DefaultMaxValue = 100;

    // Ranges narrower than these thresholds switch the value label to more decimal places
    private const double SmallRangeThreshold = 1;
    private const double MediumRangeThreshold = 10;

    // Vertical space reserved below the title and subtitle text
    private const float TitleBlockPadding = 8f;
    private const float SubtitleBlockPadding = 6f;

    // Vertical offsets for the title and subtitle text
    private const float TitleTopOffset = 4f;
    private const float SubtitleGap = 2f;

    // Distance from the bottom of the drawing bounds up to the gauge center
    private const float CenterBottomPadding = 50f;

    // Gauge radius = min(width, height) / RadiusDivisor, leaving room for the labels
    private const float RadiusDivisor = 2.5f;

    // Semicircle start and sweep in degrees (left to right across the top)
    private const float ArcStartAngle = -180f;
    private const float ArcSweepAngle = 180f;

    // Upper bounds of the red, yellow and green zones as fractions of the full sweep
    private const float RedZoneEnd = 0.33f;
    private const float YellowZoneEnd = 0.66f;
    private const float GreenZoneEnd = 1.0f;

    // Stroke width of the gauge outline drawn over the zones
    private const float ArcOutlineWidth = 3f;

    // Needle tip circle radius as a multiple of NeedleWidth
    private const float NeedleTipRadiusFactor = 1.5f;

    // Value label baseline below the center, as a fraction of the gauge radius
    private const float ValueLabelOffsetRatio = 0.3f;
    private const float ValueLabelTextSize = 24f;

    // Min/max label text and horizontal gap from the ends of the arc
    private const float MinMaxLabelTextSize = 12f;
    private const float MinMaxLabelGap = 10f;

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
                titleHeight += ChartConstants.TitleFontSize + TitleBlockPadding;
            }
            if (!string.IsNullOrEmpty(chart.Configuration.Subtitle))
            {
                titleHeight += ChartConstants.SubtitleFontSize + SubtitleBlockPadding;
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
            double minValue = series.YAxisMin ?? DefaultMinValue;
            double maxValue = series.YAxisMax ?? DefaultMaxValue;

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
            var centerY = gaugeBounds.Bottom - CenterBottomPadding; // Position gauge at bottom with some padding
            var radius = Math.Min(gaugeBounds.Width, gaugeBounds.Height) / RadiusDivisor;

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
        var titleY = bounds.Top + TitleTopOffset;

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
            var subtitleY = titleY + ChartConstants.TitleFontSize + SubtitleGap;
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

        // Zone 1: Red (0-33%)
        using var zone1Paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = ZoneBorderWidth };
        zone1Paint.Color = new SKColor(220, 80, 80, 180); // Semi-transparent red
        _drawArcSegment(canvas, centerX, centerY, radius, ArcStartAngle, ArcStartAngle + RedZoneEnd * ArcSweepAngle, zone1Paint);

        // Zone 2: Yellow (33-66%)
        using var zone2Paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = ZoneBorderWidth };
        zone2Paint.Color = new SKColor(255, 200, 80, 180); // Semi-transparent yellow
        _drawArcSegment(canvas, centerX, centerY, radius, ArcStartAngle + RedZoneEnd * ArcSweepAngle, ArcStartAngle + YellowZoneEnd * ArcSweepAngle, zone2Paint);

        // Zone 3: Green (66-100%)
        using var zone3Paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = ZoneBorderWidth };
        zone3Paint.Color = new SKColor(100, 200, 100, 180); // Semi-transparent green
        _drawArcSegment(canvas, centerX, centerY, radius, ArcStartAngle + YellowZoneEnd * ArcSweepAngle, ArcStartAngle + GreenZoneEnd * ArcSweepAngle, zone3Paint);
    }

    private void _drawGaugeArc(SKCanvas canvas, float centerX, float centerY, float radius, double minValue, double maxValue)
    {
        // Draw the gauge arc outline (semicircle)
        using var arcPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = ArcOutlineWidth,
            Color = SKColors.DimGray
        };

        // Draw semicircle from -180 to 0 degrees (left to right)
        var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
        canvas.DrawArc(rect, ArcStartAngle, ArcSweepAngle, false, arcPaint);
    }

    private void _drawNeedle(SKCanvas canvas, float centerX, float centerY, float radius, float normalizedValue)
    {
        // Calculate needle angle (-180 to 0 degrees)
        float needleAngle = ArcStartAngle + normalizedValue * ArcSweepAngle;
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
        canvas.DrawCircle(needleTipX, needleTipY, NeedleWidth * NeedleTipRadiusFactor, tipPaint);
    }

    private void _drawValueLabel(SKCanvas canvas, float centerX, float centerY, float radius, double currentValue, double minValue, double maxValue)
    {
        // Format value based on range
        string format = Math.Abs(maxValue - minValue) < SmallRangeThreshold ? "F2" : (Math.Abs(maxValue - minValue) < MediumRangeThreshold ? "F1" : "F0");
        string valueText = currentValue.ToString(format);

        using var textPaint = new SKPaint
        {
            TextSize = ValueLabelTextSize,
            Color = SKColors.Black,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
        };

        var textBounds = new SKRect();
        textPaint.MeasureText(valueText, ref textBounds);

        // Center the text
        float textX = centerX - textBounds.MidX;
        float textY = centerY + radius * ValueLabelOffsetRatio;

        canvas.DrawText(valueText, textX, textY, textPaint);
    }

    private void _drawMinMaxLabels(SKCanvas canvas, float centerX, float centerY, float radius, double minValue, double maxValue)
    {
        using var labelPaint = new SKPaint
        {
            TextSize = MinMaxLabelTextSize,
            Color = SKColors.DimGray,
            IsAntialias = true
        };

        // Draw min value at left
        var minText = minValue.ToString("F0");
        var minBounds = new SKRect();
        labelPaint.MeasureText(minText, ref minBounds);
        canvas.DrawText(minText, centerX - radius - MinMaxLabelGap - minBounds.Width, centerY, labelPaint);

        // Draw max value at right
        var maxText = maxValue.ToString("F0");
        var maxBounds = new SKRect();
        labelPaint.MeasureText(maxText, ref maxBounds);
        canvas.DrawText(maxText, centerX + radius + MinMaxLabelGap, centerY, labelPaint);
    }

    private void _drawArcSegment(SKCanvas canvas, float centerX, float centerY, float radius, float startAngle, float sweepAngle, SKPaint paint)
    {
        var rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);
        canvas.DrawArc(rect, startAngle, sweepAngle, false, paint);
    }
}
