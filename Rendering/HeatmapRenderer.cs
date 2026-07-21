// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Specialized renderer for heatmap charts.
/// Renders 2D data grids with color-coded intensity values.
/// Supports <see cref="HeatmapColorScale.Linear"/>, <see cref="HeatmapColorScale.Logarithmic"/>
/// and <see cref="HeatmapColorScale.Quantile"/> color scale modes.
/// </summary>
public class HeatmapRenderer : IChartRenderer
{
    private readonly ILogger<HeatmapRenderer> _logger;
    private const float CellPadding = 2f;

    public HeatmapRenderer(ILogger<HeatmapRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Render heatmap chart
    public void Render(SKCanvas canvas, Chart chart, SKRect bounds)
    {
        try
        {
            if (canvas == null || chart == null)
            {
                _logger.LogWarning("Invalid parameters for heatmap rendering");
                return;
            }

            _logger.LogInformation("Rendering heatmap chart: {ChartId}", chart.Id);

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
            var heatmapBounds = new SKRect(bounds.Left, bounds.Top + titleHeight, bounds.Right, bounds.Bottom);

            // Get data matrix from first series
            var series = chart.Series?.FirstOrDefault();
            if (series?.DataPoints == null || series.DataPoints.Count == 0)
            {
                _logger.LogWarning("No data points for heatmap rendering");
                return;
            }

            var dataPoints = series.DataPoints;
            var (rows, cols) = _calculateGridDimensions(dataPoints.Count);

            var colorScale = chart.Configuration?.HeatmapColorScale ?? HeatmapColorScale.Linear;
            var normalizedValues = _normalizeValues(dataPoints.Select(dp => dp.Value).ToList(), colorScale);

            // Render title and subtitle
            _renderTitleAndSubtitle(canvas, chart, bounds);

            // Calculate cell dimensions
            var cellWidth = (heatmapBounds.Width - (cols + 1) * CellPadding) / cols;
            var cellHeight = (heatmapBounds.Height - (rows + 1) * CellPadding) / rows;

            // Render cells
            using var paint = new SKPaint { IsAntialias = true };
            int index = 0;

            for (int row = 0; row < rows && index < dataPoints.Count; row++)
            {
                for (int col = 0; col < cols && index < dataPoints.Count; col++)
                {
                    var dataPoint = dataPoints[index];
                    var normalized = normalizedValues[index];

                    // Map to color (red for high, blue for low)
                    var color = _getHeatColor(normalized);
                    paint.Color = color;
                    paint.Style = SKPaintStyle.Fill;

                    var cellX = heatmapBounds.Left + col * (cellWidth + CellPadding) + CellPadding;
                    var cellY = heatmapBounds.Top + row * (cellHeight + CellPadding) + CellPadding;
                    var cellRect = new SKRect(cellX, cellY, cellX + cellWidth, cellY + cellHeight);

                    canvas.DrawRect(cellRect, paint);

                    // Draw border
                    paint.Style = SKPaintStyle.Stroke;
                    paint.Color = SKColors.Gray;
                    paint.StrokeWidth = 1;
                    canvas.DrawRect(cellRect, paint);

                    // Draw text
                    paint.Style = SKPaintStyle.Fill;
                    paint.TextSize = 10;
                    paint.Color = SKColors.Black;

                    var value = dataPoint.Value.ToString("F1");
                    var textWidth = paint.MeasureText(value);
                    var textX = cellX + (cellWidth - textWidth) / 2;
                    var textY = cellY + cellHeight / 2 + 3;

                    canvas.DrawText(value, textX, textY, paint);

                    index++;
                }
            }

            _logger.LogDebug("Heatmap rendered: {Rows}x{Cols} grid, scale={Scale}", rows, cols, colorScale);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering heatmap");
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

    /// <summary>
    /// Normalises a list of raw values to the [0, 1] range according to the chosen
    /// <see cref="HeatmapColorScale"/>:
    /// <list type="bullet">
    /// <item><see cref="HeatmapColorScale.Linear"/> — (value - min) / range</item>
    /// <item><see cref="HeatmapColorScale.Logarithmic"/> — log10-compressed before linear normalisation, revealing detail across wide dynamic ranges</item>
    /// <item><see cref="HeatmapColorScale.Quantile"/> — rank-based so each colour band covers an equal number of cells, ideal for skewed distributions</item>
    /// </list>
    /// </summary>
    private static List<double> _normalizeValues(List<double> values, HeatmapColorScale scale)
    {
        if (values.Count == 0) return values;

        return scale switch
        {
            HeatmapColorScale.Logarithmic => _normalizeLogarithmic(values),
            HeatmapColorScale.Quantile => _normalizeQuantile(values),
            _ => _normalizeLinear(values)
        };
    }

    private static List<double> _normalizeLinear(List<double> values)
    {
        var min = values.Min();
        var max = values.Max();
        var range = max - min;
        return values.Select(v => range > 0 ? (v - min) / range : 0.5).ToList();
    }

    private static List<double> _normalizeLogarithmic(List<double> values)
    {
        // Shift all values so the minimum is at least 1 before applying log10,
        // preserving relative distances regardless of the sign or magnitude of the raw data.
        var min = values.Min();
        var shift = min < 1.0 ? 1.0 - min : 0.0;
        var logValues = values.Select(v => Math.Log10(v + shift)).ToList();
        return _normalizeLinear(logValues);
    }

    private static List<double> _normalizeQuantile(List<double> values)
    {
        // Build a sorted index to obtain per-value percentile ranks.
        var indexed = values
            .Select((v, i) => (Value: v, Index: i))
            .OrderBy(x => x.Value)
            .ToList();

        var result = new double[values.Count];
        for (int rank = 0; rank < indexed.Count; rank++)
        {
            // Map rank to [0, 1]; use 0.5 when there is only one element.
            result[indexed[rank].Index] = indexed.Count > 1
                ? (double)rank / (indexed.Count - 1)
                : 0.5;
        }

        return result.ToList();
    }

    private (int rows, int cols) _calculateGridDimensions(int totalPoints)
    {
        var sqrtPoints = (int)Math.Sqrt(totalPoints);
        return (sqrtPoints, (totalPoints + sqrtPoints - 1) / sqrtPoints);
    }

    private SKColor _getHeatColor(double normalized)
    {
        // Interpolate from blue (0) through white (0.5) to red (1)
        if (normalized < 0.5)
        {
            // Blue to white
            var t = normalized * 2; // 0 to 1
            var r = (byte)(255 * t);
            var g = (byte)(255 * t);
            return new SKColor(r, g, 255);
        }
        else
        {
            // White to red
            var t = (normalized - 0.5) * 2; // 0 to 1
            var g = (byte)(255 * (1 - t));
            var b = (byte)(255 * (1 - t));
            return new SKColor(255, g, b);
        }
    }
}
