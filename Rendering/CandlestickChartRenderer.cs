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
public class CandlestickChartRenderer
{
    private readonly ILogger<CandlestickChartRenderer> _logger;

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

            var padding = 40f;
            var chartBounds = new SKRect(bounds.Left + padding, bounds.Top + padding,
                                       bounds.Right - padding, bounds.Bottom - padding);

            // Get value range from OHLC data
            var allHighValues = chart.Series.SelectMany(s => s.DataPoints
                .Where(dp => dp.CustomProperties != null && dp.CustomProperties.ContainsKey("High"))
                .Select(dp => Convert.ToDouble(dp.CustomProperties["High"]))).ToList();

            var allLowValues = chart.Series.SelectMany(s => s.DataPoints
                .Where(dp => dp.CustomProperties != null && dp.CustomProperties.ContainsKey("Low"))
                .Select(dp => Convert.ToDouble(dp.CustomProperties["Low"]))).ToList();

            var minValue = allLowValues.Any() ? allLowValues.Min() : 0.0;
            var maxValue = allHighValues.Any() ? allHighValues.Max() : 100.0;

            // Add padding to the range for better visualization
            var range = maxValue - minValue;
            if (range > 0)
            {
                minValue -= range * 0.05;
                maxValue += range * 0.05;
            }
            else
            {
                minValue -= 5;
                maxValue += 5;
            }

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

    private void _renderCandlesticks(SKCanvas canvas, Chart chart, SKRect bounds, double minValue, double maxValue)
    {
        var valueRange = maxValue - minValue;
        if (valueRange <= 0) valueRange = 1;

        var series = chart.Series;
        var pointCount = series.First().DataPoints.Count;

        // Calculate candlestick width and spacing
        // Use 1.5x spacing between candles to allow for wicks
        var candleWidth = bounds.Width / (pointCount * 1.5f);
        var candleSpacing = candleWidth * 1.5f;

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
                var bodyColor = isBullish ? SKColors.DarkGreen : SKColors.DarkRed;
                var wickColor = isBullish ? SKColors.DarkGreen : SKColors.DarkRed;

                // Draw wick (vertical line from high to low)
                // Wick is centered on the candle
                using var wickPaint = new SKPaint
                {
                    IsAntialias = true,
                    StrokeWidth = 2f,
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
                    StrokeWidth = 0.5f,
                    Color = SKColors.Black,
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
            Color = SKColors.Black,
            StrokeWidth = 1,
            IsAntialias = true
        };

        // X axis (bottom)
        canvas.DrawLine(bounds.Left, bounds.Bottom, bounds.Right, bounds.Bottom, axisPaint);

        // Y axis (left)
        canvas.DrawLine(bounds.Left, bounds.Top, bounds.Left, bounds.Bottom, axisPaint);

        // Y axis labels (value markers)
        var textPaint = new SKPaint
        {
            TextSize = 10,
            Color = SKColors.Black,
            IsAntialias = true,
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal)
        };

        // Draw 6 evenly spaced labels (0-5)
        for (int i = 0; i <= 5; i++)
        {
            var value = minValue + (maxValue - minValue) * i / 5;
            var y = bounds.Bottom - (float)(i / 5.0 * bounds.Height);
            var labelText = value.ToString("F0");
            var textWidth = textPaint.MeasureText(labelText);
            canvas.DrawText(labelText, bounds.Left - textWidth - 5, y + 4, textPaint);
        }
    }
}