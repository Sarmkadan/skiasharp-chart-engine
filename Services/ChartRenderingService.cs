// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Constants;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Service for rendering charts to various formats using SkiaSharp
/// </summary>
public class ChartRenderingService : IChartRenderingService
{
    private readonly ILogger<ChartRenderingService> _logger;
    private readonly IChartDataService _dataService;
    private readonly IRenderCacheService _cacheService;

    public ChartRenderingService(
        ILogger<ChartRenderingService> logger,
        IChartDataService dataService,
        IRenderCacheService cacheService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<RenderResult> RenderToByteArrayAsync(Chart chart, CancellationToken cancellationToken = default)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var startTime = DateTime.UtcNow;

        try
        {
            chart.ValidateForRendering();

            var cacheKey = GenerateCacheKey(chart);
            var cachedData = _cacheService.Get(cacheKey);

            if (cachedData is { Length: > 0 })
            {
                _logger.LogInformation($"Using cached render result for chart {chart.Id}");
                return RenderResult.CreateSuccess(chart.Id, cachedData, 0, ExportFormat.PNG);
            }

            var imageData = await Task.Run(() => RenderChartToBytes(chart), cancellationToken);

            _cacheService.Set(cacheKey, imageData);

            var renderTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation($"Chart {chart.Id} rendered successfully in {renderTime}ms");

            return RenderResult.CreateSuccess(chart.Id, imageData, renderTime, ExportFormat.PNG);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error rendering chart {chart?.Id}");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public async Task<RenderResult> RenderToFileAsync(Chart chart, string outputPath, CancellationToken cancellationToken = default)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentNullException(nameof(outputPath));

        var startTime = DateTime.UtcNow;

        try
        {
            chart.ValidateForRendering();

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await Task.Run(() =>
            {
                var imageData = RenderChartToBytes(chart);
                File.WriteAllBytes(outputPath, imageData);
            }, cancellationToken);

            var renderTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation($"Chart {chart.Id} exported to {outputPath} in {renderTime}ms");

            return RenderResult.CreateSuccess(chart.Id, outputPath, renderTime, ExportFormat.PNG);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error exporting chart {chart?.Id} to file");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public async Task<RenderResult> RenderWithExportAsync(Chart chart, ExportOptions exportOptions, CancellationToken cancellationToken = default)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (exportOptions == null)
            throw new ArgumentNullException(nameof(exportOptions));

        var startTime = DateTime.UtcNow;

        try
        {
            exportOptions.Validate();
            chart.ValidateForRendering();

            var fullPath = exportOptions.GetFullPath();
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await Task.Run(() =>
            {
                if (exportOptions.Format == ExportFormat.SVG)
                {
                    var svgData = RenderChartToSvg(chart);
                    File.WriteAllText(fullPath, svgData, System.Text.Encoding.UTF8);
                }
                else
                {
                    var imageData = RenderChartToBytes(chart, exportOptions.Format, exportOptions.Quality);
                    File.WriteAllBytes(fullPath, imageData);
                }
            }, cancellationToken);

            var renderTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation($"Chart {chart.Id} exported as {exportOptions.Format} to {fullPath}");

            return RenderResult.CreateSuccess(chart.Id, fullPath, renderTime, exportOptions.Format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error exporting chart with options");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public RenderResult RenderToByteArray(Chart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var startTime = DateTime.UtcNow;

        try
        {
            chart.ValidateForRendering();

            var cacheKey = GenerateCacheKey(chart);
            var cachedData = _cacheService.Get(cacheKey);

            if (cachedData is { Length: > 0 })
            {
                var cachedRenderTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                return RenderResult.CreateSuccess(chart.Id, cachedData, cachedRenderTime, ExportFormat.PNG);
            }

            var imageData = RenderChartToBytes(chart);
            _cacheService.Set(cacheKey, imageData);

            var renderTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return RenderResult.CreateSuccess(chart.Id, imageData, renderTime, ExportFormat.PNG);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering chart");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public RenderResult RenderToFile(Chart chart, string outputPath)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentNullException(nameof(outputPath));

        var startTime = DateTime.UtcNow;

        try
        {
            chart.ValidateForRendering();

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var imageData = RenderChartToBytes(chart);
            File.WriteAllBytes(outputPath, imageData);

            var renderTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return RenderResult.CreateSuccess(chart.Id, outputPath, renderTime, ExportFormat.PNG);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering chart to file");
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public void PrewarmCache(Chart chart)
    {
        if (chart == null)
            return;

        try
        {
            var imageData = RenderChartToBytes(chart);
            var cacheKey = GenerateCacheKey(chart);
            _cacheService.Set(cacheKey, imageData);
            _logger.LogInformation($"Prewarmed cache for chart {chart.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to prewarm cache for chart {chart.Id}");
        }
    }

    private byte[] RenderChartToBytes(Chart chart)
        => RenderChartToBytes(chart, ExportFormat.PNG, 1.0f);

    private byte[] RenderChartToBytes(Chart chart, ExportFormat format, float quality)
    {
        var width = chart.Configuration.Width;
        var height = chart.Configuration.Height;

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888));
        var canvas = surface.Canvas;

        canvas.Clear(SKColor.Parse(chart.Configuration.BackgroundColor));

        DrawChartFrame(canvas, chart);
        DrawGrid(canvas, chart);
        DrawSeries(canvas, chart);
        DrawAxes(canvas, chart);
        DrawLegend(canvas, chart);
        DrawTitle(canvas, chart);

        using var image = surface.Snapshot();
        var (encodedFormat, quality100) = MapEncodingFormat(format, quality);
        using var data = image.Encode(encodedFormat, quality100);
        return data.ToArray();
    }

    private static (SKEncodedImageFormat Format, int Quality) MapEncodingFormat(ExportFormat format, float quality)
    {
        var quality100 = (int)Math.Clamp(quality * 100, 1, 100);
        return format switch
        {
            ExportFormat.JPEG => (SKEncodedImageFormat.Jpeg, quality100),
            ExportFormat.WEBP => (SKEncodedImageFormat.Webp, quality100),
            _ => (SKEncodedImageFormat.Png, 100)
        };
    }

    private string RenderChartToSvg(Chart chart)
    {
        var width = chart.Configuration.Width;
        var height = chart.Configuration.Height;
        var bounds = new SKRect(0, 0, width, height);

        using var stream = new System.IO.MemoryStream();

        // SKSvgCanvas.Create returns an SKCanvas that, when disposed, finalises the SVG
        // document and flushes all content to the stream. Failing to dispose the canvas
        // is the root cause of memory leaks because SkiaSharp holds onto the internal
        // path cache until the document is closed.
        using (var canvas = SKSvgCanvas.Create(bounds, stream))
        {
            canvas.Clear(SKColor.Parse(chart.Configuration.BackgroundColor));

            DrawChartFrame(canvas, chart);
            DrawGrid(canvas, chart);
            DrawSeries(canvas, chart);
            DrawAxes(canvas, chart);
            DrawLegend(canvas, chart);
            DrawTitle(canvas, chart);
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>
    /// Computes the top Y coordinate of the plot area, accounting for any space
    /// consumed by the chart title and optional subtitle above it.
    /// </summary>
    private static float GetPlotAreaTop(Chart chart)
    {
        var config = chart.Configuration;
        float top = config.MarginTop;

        if (!string.IsNullOrEmpty(config.Title))
            top += ChartConstants.TitleFontSize + 8f;

        if (!string.IsNullOrEmpty(config.Subtitle))
            top += ChartConstants.SubtitleFontSize + 6f;

        return top;
    }

    private void DrawChartFrame(SKCanvas canvas, Chart chart)
    {
        var config = chart.Configuration;
        var rect = new SKRect(config.MarginLeft, GetPlotAreaTop(chart),
                              config.Width - config.MarginRight,
                              config.Height - config.MarginBottom);

        using var paint = new SKPaint { Color = SKColor.Parse(config.AxisColor), Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
        canvas.DrawRect(rect, paint);
    }

    private void DrawGrid(SKCanvas canvas, Chart chart)
    {
        if (!chart.Configuration.ShowGrid)
            return;

        var config = chart.Configuration;
        var rect = new SKRect(config.MarginLeft, GetPlotAreaTop(chart),
                              config.Width - config.MarginRight,
                              config.Height - config.MarginBottom);

        using var paint = new SKPaint { Color = SKColor.Parse(config.GridColor), Style = SKPaintStyle.Stroke, StrokeWidth = 0.5f };

        for (int i = 0; i <= 10; i++)
        {
            var y = rect.Top + (rect.Height / 10) * i;
            canvas.DrawLine(rect.Left, y, rect.Right, y, paint);

            var x = rect.Left + (rect.Width / 10) * i;
            canvas.DrawLine(x, rect.Top, x, rect.Bottom, paint);
        }
    }

    private void DrawSeries(SKCanvas canvas, Chart chart)
    {
        var config = chart.Configuration;
        var rect = new SKRect(config.MarginLeft, GetPlotAreaTop(chart),
                              config.Width - config.MarginRight,
                              config.Height - config.MarginBottom);

        var (minX, maxX, minY, maxY) = chart.GetDataBounds();
        var xRange = maxX - minX > 0 ? maxX - minX : 1;
        var yRange = maxY - minY > 0 ? maxY - minY : 1;

        foreach (var series in chart.Series.Where(s => s.IsVisible))
        {
            using var paint = new SKPaint
            {
                Color = SKColor.Parse(series.Color),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = series.LineWidth,
                IsAntialias = config.AntiAlias
            };

            if (series.DataPoints.Count < 2)
                continue;

            using var path = new SKPath();
            var firstPoint = true;

            foreach (var point in series.DataPoints)
            {
                var x = rect.Left + ((point.X - minX) / xRange) * rect.Width;
                var y = rect.Bottom - ((point.Y - minY) / yRange) * rect.Height;

                if (firstPoint)
                {
                    path.MoveTo((float)x, (float)y);
                    firstPoint = false;
                }
                else
                {
                    path.LineTo((float)x, (float)y);
                }
            }

            canvas.DrawPath(path, paint);
        }
    }

    private void DrawAxes(SKCanvas canvas, Chart chart)
    {
        var config = chart.Configuration;
        var rect = new SKRect(config.MarginLeft, GetPlotAreaTop(chart),
                              config.Width - config.MarginRight,
                              config.Height - config.MarginBottom);

        using var paint = new SKPaint { Color = SKColor.Parse(config.AxisColor), StrokeWidth = 1.5f };

        // X axis
        canvas.DrawLine(rect.Left, rect.Bottom, rect.Right, rect.Bottom, paint);

        // Y axis
        canvas.DrawLine(rect.Left, rect.Top, rect.Left, rect.Bottom, paint);

        // Axis labels
        if (!config.ShowAxisLabels)
            return;

        using var textPaint = new SKPaint
        {
            Color = SKColor.Parse(config.TextColor),
            TextSize = ChartConstants.AxisLabelFontSize,
            IsAntialias = true
        };

        if (!string.IsNullOrEmpty(config.XAxisLabel))
            canvas.DrawText(config.XAxisLabel, config.Width / 2, config.Height - 10, textPaint);

        if (!string.IsNullOrEmpty(config.YAxisLabel))
            canvas.DrawText(config.YAxisLabel, 10, config.MarginTop / 2, textPaint);
    }

    private void DrawLegend(SKCanvas canvas, Chart chart)
    {
        if (!chart.Configuration.ShowLegend || chart.Series.Count == 0)
            return;

        var config = chart.Configuration;
        var legendX = config.Width - config.MarginRight - 150;
        var legendY = GetPlotAreaTop(chart) + 10;

        using var paint = new SKPaint { Color = SKColor.Parse(config.TextColor), TextSize = ChartConstants.LegendFontSize, IsAntialias = true };

        int itemIndex = 0;
        foreach (var series in chart.Series.Where(s => s.IsVisible))
        {
            var y = legendY + (itemIndex * 20);
            canvas.DrawText(series.Name, legendX, y, paint);
            itemIndex++;
        }
    }

    private void DrawTitle(SKCanvas canvas, Chart chart)
    {
        var config = chart.Configuration;

        if (string.IsNullOrEmpty(config.Title))
            return;

        using var titlePaint = new SKPaint
        {
            Color = SKColor.Parse(config.TextColor),
            TextSize = ChartConstants.TitleFontSize,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            FakeBoldText = true
        };

        var centerX = config.Width / 2f;
        var titleY = config.MarginTop - 4f;
        canvas.DrawText(config.Title, centerX, titleY, titlePaint);

        if (!string.IsNullOrEmpty(config.Subtitle))
        {
            using var subtitlePaint = new SKPaint
            {
                Color = SKColor.Parse(config.TextColor),
                TextSize = ChartConstants.SubtitleFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            var subtitleY = titleY + ChartConstants.TitleFontSize + 2f;
            canvas.DrawText(config.Subtitle, centerX, subtitleY, subtitlePaint);
        }
    }

    private string GenerateCacheKey(Chart chart)
    {
        // ModifiedAt alone is not a reliable cache-invalidation signal: callers can mutate a
        // chart's series/configuration in place without ever touching ModifiedAt, which would
        // otherwise serve stale bytes for genuinely different content. Fold the render-relevant
        // state into the key so a content change always misses the cache.
        var config = chart.Configuration;
        var hash = new HashCode();
        hash.Add(config.Width);
        hash.Add(config.Height);
        hash.Add(config.Title);
        hash.Add(config.Subtitle);
        hash.Add(config.BackgroundColor);
        hash.Add(config.ShowGrid);
        hash.Add(config.ShowLegend);
        hash.Add(config.ShowAxisLabels);

        foreach (var series in chart.Series)
        {
            hash.Add(series.Name);
            hash.Add(series.Color);
            hash.Add(series.IsVisible);
            hash.Add(series.LineWidth);
            hash.Add(series.DataPoints.Count);
            foreach (var point in series.DataPoints)
            {
                hash.Add(point.X);
                hash.Add(point.Y);
            }
        }

        return $"chart_{chart.Id}_{chart.ModifiedAt:yyyyMMddHHmmss}_{hash.ToHashCode():x8}";
    }
}
