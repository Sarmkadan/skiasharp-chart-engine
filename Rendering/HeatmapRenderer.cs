// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Specialized renderer for heatmap charts.
/// Renders 2D data grids with color-coded intensity values.
/// </summary>
public class HeatmapRenderer
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

            // Get data matrix from first series
            var series = chart.Series?.FirstOrDefault();
            if (series?.DataPoints == null || series.DataPoints.Count == 0)
            {
                _logger.LogWarning("No data points for heatmap rendering");
                return;
            }

            var dataPoints = series.DataPoints;
            var (rows, cols) = _calculateGridDimensions(dataPoints.Count);

            // Normalize values
            var minValue = dataPoints.Min(dp => dp.Value);
            var maxValue = dataPoints.Max(dp => dp.Value);
            var valueRange = maxValue - minValue;

            // Calculate cell dimensions
            var cellWidth = (bounds.Width - (cols + 1) * CellPadding) / cols;
            var cellHeight = (bounds.Height - (rows + 1) * CellPadding) / rows;

            // Render cells
            var paint = new SKPaint { IsAntialias = true };
            int index = 0;

            for (int row = 0; row < rows && index < dataPoints.Count; row++)
            {
                for (int col = 0; col < cols && index < dataPoints.Count; col++)
                {
                    var dataPoint = dataPoints[index];

                    // Normalize value to 0-1
                    var normalized = valueRange > 0 ? (dataPoint.Value - minValue) / valueRange : 0.5;

                    // Map to color (red for high, blue for low)
                    var color = _getHeatColor(normalized);
                    paint.Color = color;

                    var cellX = bounds.Left + col * (cellWidth + CellPadding) + CellPadding;
                    var cellY = bounds.Top + row * (cellHeight + CellPadding) + CellPadding;
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

            _logger.LogDebug("Heatmap rendered: {Rows}x{Cols} grid", rows, cols);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering heatmap");
        }
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
