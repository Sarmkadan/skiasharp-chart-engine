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
        var startTime = DateTime.UtcNow;

        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            chart.ValidateForRendering();

            var cacheKey = GenerateCacheKey(chart);
            var cachedData = _cacheService.Get(cacheKey);

            if (cachedData != null)
            {
                _logger.LogInformation("Using cached render result for chart {ChartId}", chart.Id);
                return RenderResult.CreateSuccess(chart.Id, cachedData, 0, ExportFormat.PNG);
            }

            var imageData = await Task.Run(() => RenderChartToBytes(chart), cancellationToken);

            _cacheService.Set(cacheKey, imageData);

            var renderTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("Chart {ChartId} rendered successfully in {RenderTimeMs}ms", chart.Id, renderTime);

            return RenderResult.CreateSuccess(chart.Id, imageData, renderTime, ExportFormat.PNG);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering chart {ChartId}", chart?.Id);
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public async Task<RenderResult> RenderToFileAsync(Chart chart, string outputPath, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentNullException(nameof(outputPath));

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
            _logger.LogInformation("Chart {ChartId} exported to {OutputPath} in {RenderTimeMs}ms", chart.Id, outputPath, renderTime);

            return RenderResult.CreateSuccess(chart.Id, outputPath, renderTime, ExportFormat.PNG);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting chart {ChartId} to file", chart?.Id);
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public async Task<RenderResult> RenderWithExportAsync(Chart chart, ExportOptions exportOptions, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            if (exportOptions == null)
                throw new ArgumentNullException(nameof(exportOptions));

            exportOptions.Validate();
            chart.ValidateForRendering();

            var fullPath = exportOptions.GetFullPath();
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            await Task.Run(() =>
            {
                var imageData = RenderChartToBytes(chart);
                File.WriteAllBytes(fullPath, imageData);
            }, cancellationToken);

            var renderTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("Chart {ChartId} exported as {Format} to {OutputPath}", chart.Id, exportOptions.Format, fullPath);

            return RenderResult.CreateSuccess(chart.Id, fullPath, renderTime, exportOptions.Format);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting chart {ChartId} with options", chart?.Id);
            return RenderResult.CreateFailure(chart?.Id ?? "unknown", ex.Message, ex);
        }
    }

    public RenderResult RenderToByteArray(Chart chart)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            chart.ValidateForRendering();

            var imageData = RenderChartToBytes(chart);
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
        var startTime = DateTime.UtcNow;

        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentNullException(nameof(outputPath));

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
            _logger.LogInformation("Prewarmed cache for chart {ChartId}", chart.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to prewarm cache for chart {ChartId}", chart.Id);
        }
    }

    private byte[] RenderChartToBytes(Chart chart)
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
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private void DrawChartFrame(SKCanvas canvas, Chart chart)
    {
        var config = chart.Configuration;
        var rect = new SKRect(config.MarginLeft, config.MarginTop,
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
        var rect = new SKRect(config.MarginLeft, config.MarginTop,
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
        var rect = new SKRect(config.MarginLeft, config.MarginTop,
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
                    path.MoveTo(x, y);
                    firstPoint = false;
                }
                else
                {
                    path.LineTo(x, y);
                }
            }

            canvas.DrawPath(path, paint);
        }
    }

    private void DrawAxes(SKCanvas canvas, Chart chart)
    {
        var config = chart.Configuration;
        var rect = new SKRect(config.MarginLeft, config.MarginTop,
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
        var legendY = config.MarginTop + 10;

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
        if (string.IsNullOrEmpty(chart.Configuration.Title))
            return;

        using var titlePaint = new SKPaint
        {
            Color = SKColor.Parse(chart.Configuration.TextColor),
            TextSize = ChartConstants.TitleFontSize,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };

        canvas.DrawText(chart.Configuration.Title, chart.Configuration.Width / 2, 25, titlePaint);
    }

    private string GenerateCacheKey(Chart chart)
    {
        return $"chart_{chart.Id}_{chart.ModifiedAt:yyyyMMddHHmmss}";
    }
}
