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
/// gridlines at 25 %, 50 %, 75 % and 100 % of the radius, and the data series as a
/// filled translucent polygon with a stroked outline.
/// </summary>
public class RadarChartRenderer : IChartRenderer
{
    private readonly ILogger<RadarChartRenderer> _logger;
    private const float LabelDistance = 20f; // distance from outer radius for category labels
    private const float AxisStrokeWidth = 1f;
    private const float OutlineStrokeWidth = 2f;
    private const float FillAlpha = 80; // 0‑255

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

            _logger.LogInformation("Rendering radar chart: {ChartId}", chart.Id);

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
            var radarBounds = new SKRect(bounds.Left, bounds.Top + titleHeight, bounds.Right, bounds.Bottom);

            // Use the first series for the radar chart (mirrors PieChartRenderer behaviour)
            var series = chart.Series.FirstOrDefault();
            if (series?.DataPoints == null || series.DataPoints.Count == 0)
                return;

            var dataPoints = series.DataPoints;
            int categoryCount = dataPoints.Count;

            if (categoryCount < 3)
                throw new ChartEngineException("Radar chart requires at least 3 categories.");

            // Determine centre and radius
            var centerX = radarBounds.MidX;
            var centerY = radarBounds.MidY;
            var radius = Math.Min(radarBounds.Width, radarBounds.Height) / 3f;

            // Pre‑compute angles for each axis
            float angleStep = 360f / categoryCount;

            // Render title and subtitle
            _renderTitleAndSubtitle(canvas, chart, bounds);

            // -----------------------------------------------------------------
            // 1. Draw concentric polygon gridlines (25 %, 50 %, 75 %, 100 %)
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
            // 3. Draw data series as filled polygon
            // -----------------------------------------------------------------
            using var fillPaint = new SKPaint
            {
                Color = _getSeriesColor(0).WithAlpha((byte)FillAlpha),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var outlinePaint = new SKPaint
            {
                Color = _getSeriesColor(0),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = OutlineStrokeWidth,
                IsAntialias = true
            };

            using var path2 = new SKPath();
            float currentAngle = -90f;

            for (int i = 0; i < categoryCount; i++)
            {
                float angleDeg = -90f + i * angleStep;
                float rad = angleDeg * (float)Math.PI / 180f;
                float value = (float)dataPoints[i].Value;
                float normalizedValue = Math.Min(1.0f, Math.Max(0.0f, value));
                float x = centerX + (float)Math.Cos(rad) * radius * normalizedValue;
                float y = centerY + (float)Math.Sin(rad) * radius * normalizedValue;

                if (i == 0)
                    path2.MoveTo(x, y);
                else
                    path2.LineTo(x, y);
            }
            path2.Close();
            canvas.DrawPath(path2, fillPaint);
            canvas.DrawPath(path2, outlinePaint);

            // -----------------------------------------------------------------
            // 4. Draw category labels
            // -----------------------------------------------------------------
            using var textPaint = new SKPaint
            {
                TextSize = 11,
                Color = SKColors.Black,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            for (int i = 0; i < categoryCount; i++)
            {
                float angleDeg = -90f + i * angleStep;
                float rad = angleDeg * (float)Math.PI / 180f;
                float x = centerX + (float)Math.Cos(rad) * (radius + LabelDistance);
                float y = centerY + (float)Math.Sin(rad) * (radius + LabelDistance);

                string label = dataPoints[i].Label ?? $"Category {i + 1}";
                canvas.DrawText(label, x, y, textPaint);
            }

            _logger.LogDebug("Radar chart rendered: {CategoryCount} categories", categoryCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering radar chart");
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

    private SKColor _getSeriesColor(int index)
    {
        var colors = new[]
        {
            SKColors.RoyalBlue,
            SKColors.OrangeRed,
            SKColors.ForestGreen,
            SKColors.Gold,
            SKColors.Violet,
            SKColors.Turquoise,
            SKColors.Salmon,
            SKColors.SeaGreen
        };

        return colors[index % colors.Length];
    }
}
