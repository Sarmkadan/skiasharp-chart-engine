// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for ChartEngine providing convenient high-level operations
// and provide higher-level convenience APIs.
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
    /// <param name="engine">The chart engine instance.</param>
    /// <param name="chart">The chart to render.</param>
    /// <param name="outputPath">Full path where the chart image will be saved.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The render result containing success status and file information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> or <paramref name="chart"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Failed to render chart or copy the file.</exception>
    public static async Task<RenderResult> RenderAndSaveChartAsync(
        this ChartEngine engine,
        Chart chart,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

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
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new InvalidOperationException(
                    $"Failed to copy rendered chart to {outputPath}: {ex.Message}",
                    ex);
            }
        }

        if (result.Exception != null)
        {
            throw new InvalidOperationException(
                $"Failed to render chart: {result.ErrorMessage}",
                result.Exception);
        }

        throw new InvalidOperationException(
            $"Chart rendering failed: {result.ErrorMessage}");
    }

    /// <summary>
    /// Renders a chart and saves the result directly to a file with automatic format detection.
    /// </summary>
    /// <param name="engine">The chart engine instance.</param>
    /// <param name="chart">The chart to render.</param>
    /// <param name="outputPath">Full path where the chart image will be saved.</param>
    /// <returns>The render result containing success status and file information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> or <paramref name="chart"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="outputPath"/> is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Failed to render chart or copy the file.</exception>
    public static RenderResult RenderAndSaveChart(
        this ChartEngine engine,
        Chart chart,
        string outputPath)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

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
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new InvalidOperationException(
                    $"Failed to copy rendered chart to {outputPath}: {ex.Message}",
                    ex);
            }
        }

        if (result.Exception != null)
        {
            throw new InvalidOperationException(
                $"Failed to render chart: {result.ErrorMessage}",
                result.Exception);
        }

        throw new InvalidOperationException(
            $"Chart rendering failed: {result.ErrorMessage}");
    }

    /// <summary>
    /// Creates a new chart with the specified type and immediately saves it to the repository.
    /// </summary>
    /// <param name="engine">The chart engine instance.</param>
    /// <param name="chartType">The type of chart to create.</param>
    /// <param name="chartId">Optional chart ID (generates GUID if null).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The saved chart with its ID populated.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/>.</exception>
    public static async Task<Chart> CreateAndSaveChartAsync(
        this ChartEngine engine,
        ChartType chartType,
        string? chartId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);

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
    /// <param name="engine">The chart engine instance.</param>
    /// <param name="chartType">The type of chart to create.</param>
    /// <param name="chartId">Optional chart ID (generates GUID if null).</param>
    /// <returns>The saved chart with its ID populated.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> is <see langword="null"/>.</exception>
    public static Chart CreateAndSaveChart(
        this ChartEngine engine,
        ChartType chartType,
        string? chartId = null)
    {
        ArgumentNullException.ThrowIfNull(engine);

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
    /// <param name="engine">The chart engine instance.</param>
    /// <param name="chart">The chart to render.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Memory stream containing the rendered chart image.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> or <paramref name="chart"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Failed to render chart or image data is missing.</exception>
    public static async Task<MemoryStream> RenderToStreamAsync(
        this ChartEngine engine,
        Chart chart,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(chart);

        var result = await engine.RenderChartAsync(chart, cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"Failed to render chart: {result.ErrorMessage}",
                result.Exception);
        }

        return result.ImageData switch
        {
            null => throw new InvalidOperationException("Render result contains no image data"),
            byte[] data => new MemoryStream(data)
        };
    }

    /// <summary>
    /// Renders a chart to a memory stream instead of returning raw bytes.
    /// </summary>
    /// <param name="engine">The chart engine instance.</param>
    /// <param name="chart">The chart to render.</param>
    /// <returns>Memory stream containing the rendered chart image.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> or <paramref name="chart"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Failed to render chart or image data is missing.</exception>
    public static MemoryStream RenderToStream(
        this ChartEngine engine,
        Chart chart)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(chart);

        var result = engine.RenderChart(chart);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"Failed to render chart: {result.ErrorMessage}",
                result.Exception);
        }

        return result.ImageData switch
        {
            null => throw new InvalidOperationException("Render result contains no image data"),
            byte[] data => new MemoryStream(data)
        };
    }

    /// <summary>
    /// Quick render method that creates a chart from a simple data series and renders it.
    /// </summary>
    /// <param name="engine">The chart engine instance.</param>
    /// <param name="seriesData">Array of Y-values for the series.</param>
    /// <param name="chartType">Type of chart to create.</param>
    /// <param name="title">Chart title.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>Render result with the generated chart.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="engine"/> or <paramref name="seriesData"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="seriesData"/> is empty.</exception>
    public static async Task<RenderResult> QuickRenderAsync(
        this ChartEngine engine,
        double[] seriesData,
        ChartType chartType = ChartType.LineChart,
        string? title = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(seriesData);

        if (seriesData.Length == 0)
        {
            throw new ArgumentException("Series data cannot be empty", nameof(seriesData));
        }

        var chart = new Chart(Guid.NewGuid().ToString())
        {
            Type = chartType,
            Title = title ?? $"Quick Chart - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
            CreatedAt = DateTime.UtcNow
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

    /// <summary>
    /// Detects the export format from a file path based on its extension.
    /// </summary>
    /// <param name="filePath">The file path to analyze.</param>
    /// <returns>The detected export format.</returns>
    private static ExportFormat DetectFormatFromPath(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

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