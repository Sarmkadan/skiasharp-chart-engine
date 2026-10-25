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
/// Specialized renderer for candlestick charts.
/// Supports financial data visualization with open, high, low, close values.
/// Renders green/red candlesticks with wicks and consistent axis layout.
/// </summary>
public class CandlestickChartRenderer : IChartRenderer
{
    private readonly ILogger<CandlestickChartRenderer> _logger;

    // Empty margin between the canvas bounds and the plot area
    private const float PlotPadding = 40f;

    // Value range used when the series has no High/Low values
    private const double DefaultMinValue = 0.0;
    private const double DefaultMaxValue = 100.0;

    // Fraction of the value range added above and below the data so candles don't touch the edges
    private const double ValueRangePaddingRatio = 0.05;

    // Padding in value units used when all highs and lows are equal (zero range)
    private const double FlatRangePadding = 5.0;

    // Vertical space reserved below the title and subtitle text
    private const float TitleBlockPadding = 8f;
    private const float SubtitleBlockPadding = 6f;

    // Vertical offsets for the title and subtitle text
    private const float TitleTopOffset = 4f;
    private const float SubtitleGap = 2f;

    // Candle slot width = plot width / (point count * CandleWidthDivisor)
    private const float CandleWidthDivisor = 1.5f;

    // Distance between candle slots as a multiple of the candle width, leaving room for wicks
    private const float CandleSpacingFactor = 1.5f;

    // Wick line width, drawn centered on the candle
    private const float WickStrokeWidth = 2f;

    // Thin outline drawn around the candle body
    private const float BodyBorderStrokeWidth = 0.5f;

    // Candle colors: bullish when close >= open, bearish otherwise
    private static readonly SKColor BullishColor = SKColors.DarkGreen;
    private static readonly SKColor BearishColor = SKColors.DarkRed;

    // Color shared by the candle body outline and the axis lines
    private static readonly SKColor OutlineColor = SKColors.Black;

    // Axis line width
    private const float AxisStrokeWidth = 1f;

    // Y-axis label font size
    private const float AxisLabelTextSize = 10f;

    // Number of intervals on the Y axis; the axis shows AxisLabelIntervals + 1 labels
    private const int AxisLabelIntervals = 5;

    // Horizontal gap between a Y-axis label and the axis line
    private const float AxisLabelGap = 5f;

    // Vertical offset that centers label text on its tick mark
    private const float AxisLabelBaselineOffset = 4f;

    public CandlestickChartRenderer(ILogger<CandlestickChartRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Renders a candlestick chart onto the supplied <paramref name="canvas"/>.
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
                _logger.LogWarning("Invalid parameters for candlestick chart rendering");
                return;
            }

            _logger.LogInformation("Rendering candlestick chart: {ChartId}", chart.Id);

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

            var padding = PlotPadding;
            var chartBounds = new SKRect(
                bounds.Left + padding,
                bounds.Top + padding + titleHeight,
                bounds.Right - padding,
                bounds.Bottom - padding
            );

            // Get value range from OHLC data
            var allHighValues = chart.Series.SelectMany(s => s.DataPoints
                .Where(dp => dp.CustomProperties != null && dp.CustomProperties.ContainsKey("High"))
                .Select(dp => Convert.ToDouble(dp.CustomProperties["High"]))).ToList();

            var allLowValues = chart.Series.SelectMany(s => s.DataPoints
                .Where(dp => dp.CustomProperties != null && dp.CustomProperties.ContainsKey("Low"))
                .Select(dp => Convert.ToDouble(dp.CustomProperties["Low"]))).ToList();

            var minValue = allLowValues.Any() ? allLowValues.Min() : DefaultMinValue;
            var maxValue = allHighValues.Any() ? allHighValues.Max() : DefaultMaxValue;

            // Add padding to the range for better visualization
            var range = maxValue - minValue;
            if (range > 0)
            {
                minValue -= range * ValueRangePaddingRatio;
                maxValue += range * ValueRangePaddingRatio;
            }
            else
            {
                minValue -= FlatRangePadding;
                maxValue += FlatRangePadding;
            }

            // Render title and subtitle
            _renderTitleAndSubtitle(canvas, chart, bounds);

            // Render candlesticks
            _renderCandlesticks(canvas, chart, chartBounds, minValue, maxValue);

            // Render axes
            _renderAxes(canvas, chartBounds, minValue, maxValue);

            _logger.LogDebug("Candlestick chart rendered: {SeriesCount} series", chart.Series.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering candlestick chart");
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

    private void _renderCandlesticks(SKCanvas canvas, Chart chart, SKRect bounds, double minValue, double maxValue)
    {
        var valueRange = maxValue - minValue;
        if (valueRange <= 0) valueRange = 1;

        var series = chart.Series;
        var pointCount = series.First().DataPoints.Count;

        // Calculate candlestick width and spacing
        // Spacing between candles leaves room for wicks
        var candleWidth = bounds.Width / (pointCount * CandleWidthDivisor);
        var candleSpacing = candleWidth * CandleSpacingFactor;

        for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
        {
            var candleX = bounds.Left + pointIndex * candleSpacing;

            for (int seriesIndex = 0; seriesIndex < series.Count; seriesIndex++)
            {
                var dataPoint = series[seriesIndex].DataPoints[pointIndex];

                // Get OHLC values from CustomProperties
                if (!dataPoint.CustomProperties?.ContainsKey("Open") ?? true ||
                    !dataPoint.CustomProperties?.ContainsKey("High") ?? true ||
                    !dataPoint.CustomProperties?.ContainsKey("Low") ?? true ||
                    !dataPoint.CustomProperties?.ContainsKey("Close") ?? true)
                {
                    _logger.LogWarning("Data point at index {PointIndex} missing OHLC values", pointIndex);
                    continue;
                }

                var open = Convert.ToDouble(dataPoint.CustomProperties["Open"]);
                var high = Convert.ToDouble(dataPoint.CustomProperties["High"]);
                var low = Convert.ToDouble(dataPoint.CustomProperties["Low"]);
                var close = Convert.ToDouble(dataPoint.CustomProperties["Close"]);

                // Normalize values to chart bounds (invert Y axis since 0,0 is top-left)
                var openY = bounds.Bottom - (float)((open - minValue) / valueRange * bounds.Height);
                var highY = bounds.Bottom - (float)((high - minValue) / valueRange * bounds.Height);
                var lowY = bounds.Bottom - (float)((low - minValue) / valueRange * bounds.Height);
                var closeY = bounds.Bottom - (float)((close - minValue) / valueRange * bounds.Height);

                // Determine candle color (green if close >= open = bullish, red if close < open = bearish)
                var isBullish = close >= open;
                var bodyColor = isBullish ? BullishColor : BearishColor;
                var wickColor = isBullish ? BullishColor : BearishColor;

                // Draw wick (vertical line from high to low)
                // Wick is centered on the candle
                using var wickPaint = new SKPaint
                {
                    IsAntialias = true,
                    StrokeWidth = WickStrokeWidth,
                    Color = wickColor,
                    Style = SKPaintStyle.Stroke
                };
                var wickCenterX = candleX + candleWidth / 2;
                canvas.DrawLine(wickCenterX, highY, wickCenterX, lowY, wickPaint);

                // Draw candle body (rectangle from open to close)
                // Body is a filled rectangle between open and close prices
                var bodyTop = Math.Min(openY, closeY);
                var bodyBottom = Math.Max(openY, closeY);
                var bodyHeight = bodyBottom - bodyTop;

                // Fill the body
                using var bodyFillPaint = new SKPaint
                {
                    IsAntialias = true,
                    Color = bodyColor,
                    Style = SKPaintStyle.Fill
                };
                var bodyRect = new SKRect(candleX, bodyTop, candleX + candleWidth, bodyBottom);
                canvas.DrawRect(bodyRect, bodyFillPaint);

                // Draw body border
                using var borderPaint = new SKPaint
                {
                    IsAntialias = true,
                    StrokeWidth = BodyBorderStrokeWidth,
                    Color = OutlineColor,
                    Style = SKPaintStyle.Stroke
                };
                canvas.DrawRect(bodyRect, borderPaint);
            }
        }
    }

    private void _renderAxes(SKCanvas canvas, SKRect bounds, double minValue, double maxValue)
    {
        var axisPaint = new SKPaint
        {
            Color = OutlineColor,
            StrokeWidth = AxisStrokeWidth,
            IsAntialias = true
        };

        // X axis (bottom)
        canvas.DrawLine(bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom, axisPaint);

        // Y axis (left)
        canvas.DrawLine(bounds.Left, bounds.Top, bounds.Left, bounds.Bottom, axisPaint);

        // Y axis labels (value markers)
        var textPaint = new SKPaint
        {
            TextSize = AxisLabelTextSize,
            Color = OutlineColor,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
        };

        // Draw evenly spaced labels, one per interval boundary (AxisLabelIntervals + 1 in total)
        for (int i = 0; i <= AxisLabelIntervals; i++)
        {
            var value = minValue + (maxValue - minValue) * i / AxisLabelIntervals;
            var y = bounds.Bottom - (float)(i / (double)AxisLabelIntervals * bounds.Height);
            var labelText = value.ToString("F0");
            var textWidth = textPaint.MeasureText(labelText);
            canvas.DrawText(labelText, bounds.Left - textWidth - AxisLabelGap, y + AxisLabelBaselineOffset, textPaint);
        }
    }
}
