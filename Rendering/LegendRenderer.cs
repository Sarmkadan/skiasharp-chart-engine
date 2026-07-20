// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Renders a legend box showing color swatches and series names.
/// The legend can be positioned at any of the four corners of the chart.
/// </summary>
public class LegendRenderer
{
    private readonly ILogger<LegendRenderer> _logger;
    private const int LegendItemHeight = 20;
    private const int LegendSwatchWidth = 20;
    private const int LegendPadding = 10;
    private const int LegendSpacing = 5;

    public LegendRenderer(ILogger<LegendRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Renders a legend box at the specified corner of the chart bounds.
    /// </summary>
    /// <param name="canvas">The canvas to draw on</param>
    /// <param name="chart">The chart containing series information</param>
    /// <param name="bounds">The chart bounds</param>
    /// <param name="corner">The corner to position the legend (TopLeft, TopRight, BottomLeft, BottomRight)</param>
    public void Render(SKCanvas canvas, Chart chart, SKRect bounds, LegendCorner corner = LegendCorner.TopRight)
    {
        try
        {
            if (canvas == null || chart == null || chart.Series == null || chart.Series.Count == 0)
            {
                _logger.LogDebug("No series to render legend for");
                return;
            }

            _logger.LogInformation("Rendering legend with {SeriesCount} series at {Corner}", chart.Series.Count, corner);

            // Filter out invisible series
            var visibleSeries = new List<ChartSeries>();
            foreach (var series in chart.Series)
            {
                if (series.IsVisible && !string.IsNullOrWhiteSpace(series.Name))
                {
                    visibleSeries.Add(series);
                }
            }

            if (visibleSeries.Count == 0)
            {
                _logger.LogDebug("No visible series with names to render legend");
                return;
            }

            // Calculate legend dimensions
            var legendWidth = CalculateLegendWidth(visibleSeries);
            var legendHeight = CalculateLegendHeight(visibleSeries);

            // Calculate legend position based on corner
            var legendBounds = CalculateLegendBounds(bounds, legendWidth, legendHeight, corner);

            // Draw legend background
            DrawLegendBackground(canvas, legendBounds);

            // Draw legend items
            DrawLegendItems(canvas, visibleSeries, legendBounds);

            _logger.LogDebug("Legend rendered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering legend");
        }
    }

    private int CalculateLegendWidth(List<ChartSeries> series)
    {
        var maxTextWidth = 0f;

        using var textPaint = new SKPaint
        {
            TextSize = ChartConstants.LegendFontSize,
            Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold),
            IsAntialias = true
        };

        foreach (var series in series)
        {
            var textWidth = textPaint.MeasureText(series.Name);
            if (textWidth > maxTextWidth)
            {
                maxTextWidth = textWidth;
            }
        }

        // Legend width = swatch width + spacing + text width + padding
        return (int)(LegendSwatchWidth + LegendSpacing + maxTextWidth + (LegendPadding * 2));
    }

    private int CalculateLegendHeight(List<ChartSeries> series)
    {
        // Legend height = (item height * number of series) + padding
        return (int)(LegendItemHeight * series.Count + LegendPadding * 2);
    }

    private SKRect CalculateLegendBounds(SKRect chartBounds, int legendWidth, int legendHeight, LegendCorner corner)
    {
        return corner switch
        {
            LegendCorner.TopLeft => new SKRect(
                chartBounds.Left + LegendPadding,
                chartBounds.Top + LegendPadding,
                chartBounds.Left + LegendPadding + legendWidth,
                chartBounds.Top + LegendPadding + legendHeight
            ),
            LegendCorner.TopRight => new SKRect(
                chartBounds.Right - LegendPadding - legendWidth,
                chartBounds.Top + LegendPadding,
                chartBounds.Right - LegendPadding,
                chartBounds.Top + LegendPadding + legendHeight
            ),
            LegendCorner.BottomLeft => new SKRect(
                chartBounds.Left + LegendPadding,
                chartBounds.Bottom - LegendPadding - legendHeight,
                chartBounds.Left + LegendPadding + legendWidth,
                chartBounds.Bottom - LegendPadding
            ),
            LegendCorner.BottomRight => new SKRect(
                chartBounds.Right - LegendPadding - legendWidth,
                chartBounds.Bottom - LegendPadding - legendHeight,
                chartBounds.Right - LegendPadding,
                chartBounds.Bottom - LegendPadding
            ),
            _ => new SKRect(
                chartBounds.Right - LegendPadding - legendWidth,
                chartBounds.Top + LegendPadding,
                chartBounds.Right - LegendPadding,
                chartBounds.Top + LegendPadding + legendHeight
            )
        };
    }

    private void DrawLegendBackground(SKCanvas canvas, SKRect bounds)
    {
        using var backgroundPaint = new SKPaint
        {
            Color = SKColors.White.WithAlpha(240), // Semi-transparent white
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        // Draw rounded rectangle background
        using var path = new SKPath();
        var radius = 4f;
        path.AddRoundRect(bounds, radius, radius);
        canvas.DrawPath(path, backgroundPaint);

        // Draw border
        using var borderPaint = new SKPaint
        {
            Color = SKColors.Gray.WithAlpha(180),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };

        canvas.DrawRoundRect(bounds, radius, radius, borderPaint);
    }

    private void DrawLegendItems(SKCanvas canvas, List<ChartSeries> series, SKRect bounds)
    {
        var itemY = bounds.Top + LegendPadding;

        using var textPaint = new SKPaint
        {
            TextSize = ChartConstants.LegendFontSize,
            Color = ChartConstants.DefaultTextColor.ToSKColor(),
            Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold),
            IsAntialias = true
        };

        using var swatchPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        for (int i = 0; i < series.Count; i++)
        {
            var seriesItem = series[i];
            var swatchColor = ParseColor(seriesItem.Color);

            // Draw color swatch
            var swatchRect = new SKRect(
                bounds.Left + LegendPadding,
                itemY,
                bounds.Left + LegendPadding + LegendSwatchWidth,
                itemY + LegendItemHeight
            );

            swatchPaint.Color = swatchColor;
            canvas.DrawRect(swatchRect, swatchPaint);

            // Draw series name
            var textX = bounds.Left + LegendPadding + LegendSwatchWidth + LegendSpacing;
            var textY = itemY + LegendItemHeight - 3; // Align text vertically
            canvas.DrawText(seriesItem.Name, textX, textY, textPaint);

            itemY += LegendItemHeight;
        }
    }

    private SKColor ParseColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return SKColors.Blue; // Default color
        }

        try
        {
            if (color.StartsWith("#"))
            {
                // Parse hex color
                if (SKColor.TryParse(color, out var skColor))
                {
                    return skColor;
                }
            }

            // Try direct color parsing
            if (SKColor.TryParse(color, out var directColor))
            {
                return directColor;
            }
        }
        catch
        {
            // Fallback to default
        }

        return SKColors.Blue; // Default fallback
    }
}

/// <summary>
/// Specifies the corner where the legend should be positioned
/// </summary>
public enum LegendCorner
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}
