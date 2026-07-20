// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Exceptions;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Renderer for radar (spider) charts.
/// Draws N axes radiating from the centre (one per category), concentric polygon
/// gridlines at 25 %, 50 %, 75 % and 100 % of the radius, and the data series as a
/// filled translucent polygon with a stroked outline.
/// </summary>
public class RadarChartRenderer
{
    private readonly ILogger<RadarChartRenderer> _logger;
    private const float LabelDistance = 20f;   // distance from outer radius for category labels
    private const float AxisStrokeWidth = 1f;
    private const float OutlineStrokeWidth = 2f;
    private const float FillAlpha = 80;        // 0‑255

    public RadarChartRenderer(ILogger<RadarChartRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Renders a radar chart onto the supplied <paramref name="canvas"/>.
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
                _logger.LogWarning("Invalid parameters for radar chart rendering");
                return;
            }

            // Use the first series for the radar chart (mirrors PieChartRenderer behaviour)
            var series = chart.Series.FirstOrDefault();
            if (series?.DataPoints == null || series.DataPoints.Count == 0)
                return;

            var dataPoints = series.DataPoints;
            int categoryCount = dataPoints.Count;

            if (categoryCount < 3)
                throw new ChartEngineException("Radar chart requires at least 3 categories.");

            _logger.LogInformation("Rendering radar chart: {ChartId}", chart.Id);

            // Determine centre and radius
            var centerX = bounds.MidX;
            var centerY = bounds.MidY;
            var radius = Math.Min(bounds.Width, bounds.Height) / 3f;

            // Pre‑compute angles for each axis
            float angleStep = 360f / categoryCount;

            // -----------------------------------------------------------------
            // 1. Draw concentric polygon gridlines (25 %, 50 %, 75 %, 100 %)
            // -----------------------------------------------------------------
            using var gridPaint = new SKPaint
            {
                Color = SKColors.LightGray,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 0.5f,
                IsAntialias = true
            };

            for (int level = 1; level <= 4; level++)
            {
                float levelRadius = radius * level / 4f;
                using var path = new SKPath();

                for (int i = 0; i < categoryCount; i++)
                {
                    float angleDeg = -90f + i * angleStep;
                    float rad = angleDeg * (float)Math.PI / 180f;
                    float x = centerX + (float)Math.Cos(rad) * levelRadius;
                    float y = centerY + (float)Math.Sin(rad) * levelRadius;

                    if (i == 0)
                        path.MoveTo(x, y);
                    else
                        path.LineTo(x, y);
                }
                path.Close();
                canvas.DrawPath(path, gridPaint);
            }

            // -----------------------------------------------------------------
            // 2. Draw axes
            // -----------------------------------------------------------------
            using var axisPaint = new SKPaint
            {
                Color = SKColors.Gray,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = AxisStrokeWidth,
                IsAntialias = true
            };

            for (int i = 0; i < categoryCount; i++)
            {
                float angleDeg = -90f + i * angleStep;
                float rad = angleDeg * (float)Math.PI / 180f;
                float x = centerX + (float)Math.Cos(rad) * radius;
                float y = centerY + (float)Math.Sin(rad) * radius;
                canvas.DrawLine(centerX, centerY, x, y, axisPaint);
            }

            // -----------------------------------------------------------------
            // 3. Draw data polygon (filled + outline)
            // -----------------------------------------------------------------
            // Normalise values to the maximum value in the series
            double maxValue = dataPoints.Max(dp => dp.Value);
            if (maxValue == 0) maxValue = 1; // avoid division by zero

            using var fillPaint = new SKPaint
            {
                Color = _getColor(0).WithAlpha(FillAlpha),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var outlinePaint = new SKPaint
            {
                Color = _getColor(0),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = OutlineStrokeWidth,
                IsAntialias = true
            };

            using var dataPath = new SKPath();

            for (int i = 0; i < categoryCount; i++)
            {
                var dp = dataPoints[i];
                float valueRatio = (float)(dp.Value / maxValue);
                float pointRadius = radius * valueRatio;

                float angleDeg = -90f + i * angleStep;
                float rad = angleDeg * (float)Math.PI / 180f;
                float x = centerX + (float)Math.Cos(rad) * pointRadius;
                float y = centerY + (float)Math.Sin(rad) * pointRadius;

                if (i == 0)
                    dataPath.MoveTo(x, y);
                else
                    dataPath.LineTo(x, y);
            }
            dataPath.Close();

            canvas.DrawPath(dataPath, fillPaint);
            canvas.DrawPath(dataPath, outlinePaint);

            // -----------------------------------------------------------------
            // 4. Draw category labels
            // -----------------------------------------------------------------
            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 10,
                IsAntialias = true
            };

            for (int i = 0; i < categoryCount; i++)
            {
                var dp = dataPoints[i];
                float angleDeg = -90f + i * angleStep;
                float rad = angleDeg * (float)Math.PI / 180f;
                float labelRadius = radius + LabelDistance;
                float x = centerX + (float)Math.Cos(rad) * labelRadius;
                float y = centerY + (float)Math.Sin(rad) * labelRadius;

                var label = dp.Label ?? $"Cat{i + 1}";
                var textWidth = textPaint.MeasureText(label);
                // Offset the text so it is centred on the radial line
                canvas.DrawText(label, x - textWidth / 2, y + textPaint.TextSize / 2, textPaint);
            }

            _logger.LogDebug("Radar chart rendered: {CategoryCount} categories", categoryCount);
        }
        catch (ChartEngineException)
        {
            // Re‑throw custom chart exceptions unchanged – they are part of the public API
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering radar chart");
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
