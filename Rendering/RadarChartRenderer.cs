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
    private const float GridStrokeWidth = 0.5f; // stroke width of the concentric gridline polygons
    private const int GridLevels = 4; // number of concentric gridline polygons (25 %, 50 %, 75 %, 100 %)
    private const float StartAngleDegrees = -90f; // first axis points straight up (12 o'clock)
    private const float FullCircleDegrees = 360f; // angular span divided evenly between categories
    private const float RadiusDivisor = 3f; // chart radius = smaller bounds dimension / RadiusDivisor (leaves room for labels)
    private const int MinimumCategories = 3; // a radar polygon needs at least three axes
    private const float LabelTextSize = 11f; // font size of category labels
    private const float TitleBlockPadding = 8f; // extra vertical space reserved below the title
    private const float SubtitleBlockPadding = 6f; // extra vertical space reserved below the subtitle
    private const float TitleTopMargin = 4f; // gap between the top of the bounds and the title baseline
    private const float SubtitleLineGap = 2f; // gap between the title baseline and the subtitle baseline

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
                titleHeight += ChartConstants.TitleFontSize + TitleBlockPadding;
            }
            if (!string.IsNullOrEmpty(chart.Configuration.Subtitle))
            {
                titleHeight += ChartConstants.SubtitleFontSize + SubtitleBlockPadding;
            }

            // Adjust bounds to account for title/subtitle
            var radarBounds = new SKRect(bounds.Left, bounds.Top + titleHeight, bounds.Right, bounds.Bottom);

            // Use the first series for the radar chart (mirrors PieChartRenderer behaviour)
            var series = chart.Series.FirstOrDefault();
            if (series?.DataPoints == null || series.DataPoints.Count == 0)
                return;

            var dataPoints = series.DataPoints;
            int categoryCount = dataPoints.Count;

            if (categoryCount < MinimumCategories)
                throw new ChartEngineException($"Radar chart requires at least {MinimumCategories} categories.");

            // Determine centre and radius
            var centerX = radarBounds.MidX;
            var centerY = radarBounds.MidY;
            var radius = Math.Min(radarBounds.Width, radarBounds.Height) / RadiusDivisor;

            // Pre‑compute angles for each axis
            float angleStep = FullCircleDegrees / categoryCount;

            // Render title and subtitle
            _renderTitleAndSubtitle(canvas, chart, bounds);

            // -----------------------------------------------------------------
            // 1. Draw concentric polygon gridlines (25 %, 50 %, 75 %, 100 %)
            // -----------------------------------------------------------------
            using var gridPaint = new SKPaint
            {
                Color = SKColors.LightGray,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = GridStrokeWidth,
                IsAntialias = true
            };

            for (int level = 1; level <= GridLevels; level++)
            {
                float levelRadius = radius * level / GridLevels;
                using var path = new SKPath();

                for (int i = 0; i < categoryCount; i++)
                {
                    float angleDeg = StartAngleDegrees + i * angleStep;
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
                float angleDeg = StartAngleDegrees + i * angleStep;
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
            float currentAngle = StartAngleDegrees;

            for (int i = 0; i < categoryCount; i++)
            {
                float angleDeg = StartAngleDegrees + i * angleStep;
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
                TextSize = LabelTextSize,
                Color = SKColors.Black,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            for (int i = 0; i < categoryCount; i++)
            {
                float angleDeg = StartAngleDegrees + i * angleStep;
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
        var titleY = bounds.Top + TitleTopMargin;

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
            var subtitleY = titleY + ChartConstants.TitleFontSize + SubtitleLineGap;
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
