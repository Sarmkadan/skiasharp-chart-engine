// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for ChartEngine providing convenient high-level operations
// =============================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine;

/// <summary>
/// Provides extension methods for <see cref="ChartEngine"/> to simplify common chart operations
/// and provide higher-level convenience APIs.
/// </summary>
public static class ChartEngineExtensions
{
    /// <summary>
    /// Renders a chart and saves the result directly to a file with automatic format detection.
    /// </summary>
    /// <param name="engine">The chart engine instance</param>
    /// <param name="chart">The chart to render</param>
    /// <param name="outputPath">Full path where the chart image will be saved</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The render result containing success status and file information</returns>
    public static async Task<RenderResult> RenderAndSaveChartAsync(
        this ChartEngine engine,
        Chart chart,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        if (engine == null)
            throw new ArgumentNullException(nameof(engine));

        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));

        var format = DetectFormatFromPath(outputPath);
        var options = new ExportOptions(Path.GetFileNameWithoutExtension(outputPath), format)
        {
            OutputDirectory = Path.GetDirectoryName(outputPath),
            DPI = 300,
            Quality = 0.95f
        };

        var result = await engine.ExportChartAsync(chart, options, cancellationToken);

        if (result.Success && result.OutputPath != null)
        {
            try
            {
                File.Copy(result.OutputPath, outputPath, overwrite: true);
                return RenderResult.CreateSuccess(
                    chart.Id,
                    outputPath,
                    result.RenderTimeMs,
                    format);
            }
            catch (Exception ex)
            {
                return RenderResult.CreateFailure(
                    chart.Id,
                    $"Failed to copy file to {outputPath}: {ex.Message}",
                    ex);
            }
        }

        return result;
    }

    /// <summary>
    /// Renders a chart and saves the result directly to a file with automatic format detection.
    /// </summary>
    /// <param name="engine">The chart engine instance</param>
    /// <param name="chart">The chart to render</param>
    /// <param name="outputPath">Full path where the chart image will be saved</param>
    /// <returns>The render result containing success status and file information</returns>
    public static RenderResult RenderAndSaveChart(
        this ChartEngine engine,
        Chart chart,
        string outputPath)
    {
        if (engine == null)
            throw new ArgumentNullException(nameof(engine));

        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("Output path cannot be null or empty", nameof(outputPath));

        var format = DetectFormatFromPath(outputPath);
        var options = new ExportOptions(Path.GetFileNameWithoutExtension(outputPath), format)
        {
            OutputDirectory = Path.GetDirectoryName(outputPath),
            DPI = 300,
            Quality = 0.95f
        };

        var result = engine.ExportChart(chart, options);

        if (result.Success && result.OutputPath != null)
        {
            try
            {
                File.Copy(result.OutputPath, outputPath, overwrite: true);
                return RenderResult.CreateSuccess(
                    chart.Id,
                    outputPath,
                    result.RenderTimeMs,
                    format);
            }
            catch (Exception ex)
            {
                return RenderResult.CreateFailure(
                    chart.Id,
                    $"Failed to copy file to {outputPath}: {ex.Message}",
                    ex);
            }
        }

        return result;
    }

    /// <summary>
    /// Creates a new chart with the specified type and immediately saves it to the repository.
    /// </summary>
    /// <param name="engine">The chart engine instance</param>
    /// <param name="chartType">The type of chart to create</param>
    /// <param name="chartId">Optional chart ID (generates GUID if null)</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>The saved chart with its ID populated</returns>
    public static async Task<Chart> CreateAndSaveChartAsync(
        this ChartEngine engine,
        ChartType chartType,
        string? chartId = null,
        CancellationToken cancellationToken = default)
    {
        if (engine == null)
            throw new ArgumentNullException(nameof(engine));

        var chart = new Chart(chartId ?? Guid.NewGuid().ToString())
        {
            Type = chartType,
            Title = $"Chart {chartType}",
            CreatedAt = DateTime.UtcNow
        };

        await engine.SaveChartAsync(chart, cancellationToken);
        return chart;
    }

    /// <summary>
    /// Creates a new chart with the specified type and immediately saves it to the repository.
    /// </summary>
    /// <param name="engine">The chart engine instance</param>
    /// <param name="chartType">The type of chart to create</param>
    /// <param name="chartId">Optional chart ID (generates GUID if null)</param>
    /// <returns>The saved chart with its ID populated</returns>
    public static Chart CreateAndSaveChart(
        this ChartEngine engine,
        ChartType chartType,
        string? chartId = null)
    {
        if (engine == null)
            throw new ArgumentNullException(nameof(engine));

        var chart = new Chart(chartId ?? Guid.NewGuid().ToString())
        {
            Type = chartType,
            Title = $"Chart {chartType}",
            CreatedAt = DateTime.UtcNow
        };

        engine.SaveChart(chart);
        return chart;
    }

    /// <summary>
    /// Renders a chart to a memory stream instead of returning raw bytes.
    /// </summary>
    /// <param name="engine">The chart engine instance</param>
    /// <param name="chart">The chart to render</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>Memory stream containing the rendered chart image</returns>
    public static async Task<MemoryStream> RenderToStreamAsync(
        this ChartEngine engine,
        Chart chart,
        CancellationToken cancellationToken = default)
    {
        if (engine == null)
            throw new ArgumentNullException(nameof(engine));

        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var result = await engine.RenderChartAsync(chart, cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"Failed to render chart: {result.ErrorMessage}",
                result.Exception);
        }

        if (result.ImageData == null)
        {
            throw new InvalidOperationException("Render result contains no image data");
        }

        var stream = new MemoryStream(result.ImageData);
        return stream;
    }

    /// <summary>
    /// Renders a chart to a memory stream instead of returning raw bytes.
    /// </summary>
    /// <param name="engine">The chart engine instance</param>
    /// <param name="chart">The chart to render</param>
    /// <returns>Memory stream containing the rendered chart image</returns>
    public static MemoryStream RenderToStream(
        this ChartEngine engine,
        Chart chart)
    {
        if (engine == null)
            throw new ArgumentNullException(nameof(engine));

        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var result = engine.RenderChart(chart);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"Failed to render chart: {result.ErrorMessage}",
                result.Exception);
        }

        if (result.ImageData == null)
        {
            throw new InvalidOperationException("Render result contains no image data");
        }

        var stream = new MemoryStream(result.ImageData);
        return stream;
    }

    /// <summary>
    /// Quick render method that creates a chart from a simple data series and renders it.
    /// </summary>
    /// <param name="engine">The chart engine instance</param>
    /// <param name="seriesData">Array of Y-values for the series</param>
    /// <param name="chartType">Type of chart to create</param>
    /// <param name="title">Chart title</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>Render result with the generated chart</returns>
    public static async Task<RenderResult> QuickRenderAsync(
        this ChartEngine engine,
        double[] seriesData,
        ChartType chartType = ChartType.LineChart,
        string? title = null,
        CancellationToken cancellationToken = default)
    {
        if (engine == null)
            throw new ArgumentNullException(nameof(engine));

        if (seriesData == null || seriesData.Length == 0)
            throw new ArgumentException("Series data cannot be null or empty", nameof(seriesData));

        var chart = new Chart(Guid.NewGuid().ToString())
        {
            Type = chartType,
            Title = title ?? $"Quick Chart - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
        };

        var series = new ChartSeries("Series 1")
        {
            SeriesType = chartType
        };

        for (int i = 0; i < seriesData.Length; i++)
        {
            series.AddDataPoint(i, seriesData[i]);
        }

        chart.AddSeries(series);

        return await engine.RenderChartAsync(chart, cancellationToken);
    }

    private static ExportFormat DetectFormatFromPath(string filePath)
    {
        var extension = Path.GetExtension(filePath)?.TrimStart('.')?.ToLowerInvariant();

        return extension switch
        {
            "png" => ExportFormat.PNG,
            "svg" => ExportFormat.SVG,
            "pdf" => ExportFormat.PDF,
            "jpg" or "jpeg" => ExportFormat.JPEG,
            "webp" => ExportFormat.WEBP,
            _ => ExportFormat.PNG
        };
    }
}