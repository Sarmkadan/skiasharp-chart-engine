using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Integration;

/// <summary>
/// Integration tests for chart rendering functionality that verify end-to-end behavior
/// of the SkiaSharp chart engine with real service dependencies and file system operations.
/// Tests cover rendering to files, byte arrays, various export formats, caching behavior,
/// configuration options, multi-series charts, concurrent operations, and edge cases.
/// </summary>
public class ChartRenderingIntegrationTests : IDisposable
{
    /// <summary>
    /// Service provider configured with all chart engine services for integration testing.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Temporary directory path used for storing test output files.
    /// Created in constructor and cleaned up in Dispose method.
    /// </summary>
    private readonly string _tempOutputDir;

    /// <summary>
    /// Initializes a new instance of the ChartRenderingIntegrationTests class.
    /// Sets up a temporary directory for test output files and configures the service provider
    /// with all required chart engine services including rendering, export, caching, and interactivity.
    /// </summary>
    public ChartRenderingIntegrationTests()
    {
        _tempOutputDir = Path.Combine(Path.GetTempPath(), "chart-tests-" + Guid.NewGuid());
        Directory.CreateDirectory(_tempOutputDir);

        var services = new ServiceCollection();
        services.AddLogging(x => x.AddConsole());
        services.AddScoped<IChartDataService, ChartDataService>();
        services.AddScoped<IRenderCacheService, RenderCacheService>();
        services.AddScoped<IChartRenderingService, ChartRenderingService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IInteractivityService, InteractivityService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Cleans up test resources by deleting the temporary output directory and disposing
    /// the service provider to release any allocated resources.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempOutputDir))
            Directory.Delete(_tempOutputDir, true);
        (_serviceProvider as IDisposable)?.Dispose();
    }

    /// <summary>
    /// Creates a sample line chart with 12 months of sales data for testing purposes.
    /// Generates random variations in the data points to ensure realistic chart rendering.
    /// </summary>
    /// <returns>A configured Chart object with line series containing sales data.</returns>
    private Chart CreateLineChartWithData()
    {
        var chart = new Chart("line-chart-integration");
        var series = new ChartSeries("Sales");

        for (int i = 1; i <= 12; i++)
        {
            series.AddDataPoint(i, 1000 + (i * 500) + new Random().Next(100, 500));
        }

        chart.AddSeries(series);
        chart.Configuration.Title = "Monthly Sales Report";
        chart.Configuration.XAxisLabel = "Month";
        chart.Configuration.YAxisLabel = "Sales ($)";
        chart.Configuration.ShowGrid = true;
        chart.Configuration.ShowLegend = true;

        return chart;
    }

    /// <summary>
    /// Creates a sample multi-series chart with three different product lines for testing purposes.
    /// Each series has distinct colors and different data patterns to verify multi-series rendering.
    /// </summary>
    /// <returns>A configured Chart object with three ChartSeries objects representing different products.</returns>
    private Chart CreateMultiSeriesChart()
    {
        var chart = new Chart("multi-series-chart");

        var series1 = new ChartSeries("Product A") { Color = "#1F77B4" };
        for (int i = 1; i <= 6; i++)
            series1.AddDataPoint(i, 100 + i * 20);

        var series2 = new ChartSeries("Product B") { Color = "#FF7F0E" };
        for (int i = 1; i <= 6; i++)
            series2.AddDataPoint(i, 150 + i * 15);

        var series3 = new ChartSeries("Product C") { Color = "#2CA02C" };
        for (int i = 1; i <= 6; i++)
            series3.AddDataPoint(i, 120 + i * 25);

        chart.AddSeries(series1);
        chart.AddSeries(series2);
        chart.AddSeries(series3);

        return chart;
    }

    // ---------------------------------------------------------------
    // Basic rendering tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that a simple line chart can be successfully rendered to a PNG file.
    /// Verifies that the rendering service produces a valid output file with non-zero size.
    /// </summary>
    [Fact]
    public void CanRenderSimpleLineChartToFile()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = CreateLineChartWithData();
        var outputPath = Path.Combine(_tempOutputDir, "simple-line.png");

        // Act
        var result = renderingService.RenderToFile(chart, outputPath);

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(outputPath).Should().BeTrue();
        new FileInfo(outputPath).Length.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that a chart can be rendered to a byte array in memory.
    /// Verifies that the rendering service returns a successful result with valid image data.
    /// </summary>
    [Fact]
    public void CanRenderChartToByteArray()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = CreateLineChartWithData();

        // Act
        var result = renderingService.RenderToByteArray(chart);

        // Assert
        result.Success.Should().BeTrue();
        result.ImageData.Should().NotBeNull();
        result.ImageData.Length.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that chart rendering to file works asynchronously.
    /// Verifies that the async rendering service produces a valid output file.
    /// </summary>
    [Fact]
    public async Task CanRenderChartAsyncToFile()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = CreateLineChartWithData();
        var outputPath = Path.Combine(_tempOutputDir, "async-render.png");

        // Act
        var result = await renderingService.RenderToFileAsync(chart, outputPath);

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(outputPath).Should().BeTrue();
    }

    // ---------------------------------------------------------------
    // Export format tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that a chart can be exported to PNG format.
    /// Verifies that the export service successfully creates a PNG file with valid content.
    /// </summary>
    [Fact]
    public void CanExportChartAsPng()
    {
        // Arrange
        var exportService = _serviceProvider.GetRequiredService<IExportService>();
        var chart = CreateLineChartWithData();
        var options = new ExportOptions
        {
            Format = ExportFormat.PNG,
            DirectoryPath = _tempOutputDir,
            FileName = "export-test.png"
        };

        // Act
        var result = exportService.Export(chart, options);

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(Path.Combine(_tempOutputDir, "export-test.png")).Should().BeTrue();
    }

    /// <summary>
    /// Tests that a chart can be exported to SVG format asynchronously.
    /// Verifies that the async export service creates a valid SVG file containing SVG markup.
    /// </summary>
    [Fact]
    public async Task CanExportChartAsSvg()
    {
        // Arrange
        var exportService = _serviceProvider.GetRequiredService<IExportService>();
        var chart = CreateLineChartWithData();
        var options = new ExportOptions
        {
            Format = ExportFormat.SVG,
            DirectoryPath = _tempOutputDir,
            FileName = "export-test.svg"
        };

        // Act
        var result = await exportService.ExportAsync(chart, options);

        // Assert
        result.Success.Should().BeTrue();
        var outputPath = Path.Combine(_tempOutputDir, "export-test.svg");
        File.Exists(outputPath).Should().BeTrue();
        var content = File.ReadAllText(outputPath);
        content.Should().Contain("<svg");
    }

    /// <summary>
    /// Tests that a chart can be exported to JPEG format asynchronously.
    /// Verifies that the async export service successfully creates a JPEG file.
    /// </summary>
    [Fact]
    public async Task CanExportChartAsJpeg()
    {
        // Arrange
        var exportService = _serviceProvider.GetRequiredService<IExportService>();
        var chart = CreateLineChartWithData();
        var options = new ExportOptions
        {
            Format = ExportFormat.JPEG,
            DirectoryPath = _tempOutputDir,
            FileName = "export-test.jpg"
        };

        // Act
        var result = await exportService.ExportAsync(chart, options);

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(Path.Combine(_tempOutputDir, "export-test.jpg")).Should().BeTrue();
    }

    /// <summary>
    /// Tests that a chart can be exported to WebP format asynchronously.
    /// Verifies that the async export service successfully creates a WebP file.
    /// </summary>
    [Fact]
    public async Task CanExportChartAsWebP()
    {
        // Arrange
        var exportService = _serviceProvider.GetRequiredService<IExportService>();
        var chart = CreateLineChartWithData();
        var options = new ExportOptions
        {
            Format = ExportFormat.WEBP,
            DirectoryPath = _tempOutputDir,
            FileName = "export-test.webp"
        };

        // Act
        var result = await exportService.ExportAsync(chart, options);

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(Path.Combine(_tempOutputDir, "export-test.webp")).Should().BeTrue();
    }

    // ---------------------------------------------------------------
    // Multi-series chart tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that a multi-series chart with three different data series can be successfully rendered.
    /// Verifies that multiple series are properly rendered and the output file has sufficient size.
    /// </summary>
    [Fact]
    public void CanRenderMultiSeriesChart()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = CreateMultiSeriesChart();
        var outputPath = Path.Combine(_tempOutputDir, "multi-series.png");

        // Act
        var result = renderingService.RenderToFile(chart, outputPath);

        // Assert
        result.Success.Should().BeTrue();
        File.Exists(outputPath).Should().BeTrue();
        new FileInfo(outputPath).Length.Should().BeGreaterThan(100);
    }

    /// <summary>
    /// Tests that series visibility can be toggled and produces different rendering results.
    /// Verifies that hiding a series changes the rendered output compared to showing it.
    /// </summary>
    [Fact]
    public void CanToggleSeriesVisibility()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = CreateMultiSeriesChart();

        // Act - hide one series
        chart.Series[1].IsVisible = false;
        var result1 = renderingService.RenderToByteArray(chart);

        // Make it visible again
        chart.Series[1].IsVisible = true;
        var result2 = renderingService.RenderToByteArray(chart);

        // Assert - both should succeed but produce different output
        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
        result1.ImageData.Should().NotEqual(result2.ImageData);
    }

    // ---------------------------------------------------------------
    // Caching behavior tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that the rendering cache improves performance on subsequent renders of the same chart.
    /// Verifies that cached rendering produces faster results than the initial uncached render.
    /// </summary>
    [Fact]
    public void CachingImprovesPerfomanceOnSecondRender()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = CreateLineChartWithData();

        // Act - first render
        var result1 = renderingService.RenderToByteArray(chart);
        var time1 = result1.RenderTimeMilliseconds;

        // Second render (should hit cache)
        var result2 = renderingService.RenderToByteArray(chart);
        var time2 = result2.RenderTimeMilliseconds;

        // Assert
        result1.Success.Should().BeTrue();
        result2.Success.Should().BeTrue();
        // Cached result should be nearly instant
        time2.Should().BeLessThan(time1);
    }

    /// <summary>
    /// Tests that different chart configurations produce different cache keys.
    /// Verifies that charts with different IDs generate distinct rendered outputs.
    /// </summary>
    [Fact]
    public void DifferentChartsProduceDifferentCacheKeys()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart1 = CreateLineChartWithData();
        var chart2 = CreateLineChartWithData();
        chart2.Id = "different-id";

        // Act
        var result1 = renderingService.RenderToByteArray(chart1);
        var result2 = renderingService.RenderToByteArray(chart2);

        // Assert
        result1.ImageData.Should().NotEqual(result2.ImageData);
    }

    // ---------------------------------------------------------------
    // Configuration tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that custom chart configuration can be applied and affects rendering.
    /// Verifies that changing width, height, background color, and grid color produces valid output.
    /// </summary>
    [Fact]
    public void CanApplyCustomConfiguration()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = CreateLineChartWithData();

        // Act - apply custom styling
        chart.Configuration.Width = 1024;
        chart.Configuration.Height = 768;
        chart.Configuration.BackgroundColor = "#FFFFFF";
        chart.Configuration.GridColor = "#CCCCCC";
        var result = renderingService.RenderToByteArray(chart);

        // Assert
        result.Success.Should().BeTrue();
        result.ImageData.Length.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that grid and legend can be disabled to produce different rendering output.
    /// Verifies that disabling grid and legend changes the visual appearance of the chart.
    /// </summary>
    [Fact]
    public void CanDisableGridAndLegend()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = CreateLineChartWithData();

        // Act
        var resultWithGrid = renderingService.RenderToByteArray(chart);

        chart.Configuration.ShowGrid = false;
        chart.Configuration.ShowLegend = false;
        var resultWithoutGrid = renderingService.RenderToByteArray(chart);

        // Assert
        resultWithGrid.Success.Should().BeTrue();
        resultWithoutGrid.Success.Should().BeTrue();
        // Different configurations should produce different output
        resultWithGrid.ImageData.Should().NotEqual(resultWithoutGrid.ImageData);
    }

    // ---------------------------------------------------------------
    // Data validation tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that rendering fails appropriately when a chart has no data series.
    /// Verifies that charts without any series produce a failed rendering result.
    /// </summary>
    [Fact]
    public void RenderingFailsWithMissingData()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = new Chart("empty-chart");

        // Act
        var result = renderingService.RenderToByteArray(chart);

        // Assert
        result.Success.Should().BeFalse();
    }

    /// <summary>
    /// Tests that export fails appropriately when an invalid output path is provided.
    /// Verifies that attempting to export to a non-existent directory produces a failed result.
    /// </summary>
    [Fact]
    public void ExportFailsWithInvalidPath()
    {
        // Arrange
        var exportService = _serviceProvider.GetRequiredService<IExportService>();
        var chart = CreateLineChartWithData();
        // A plain non-existent directory is auto-created by the export pipeline (and would
        // succeed here since tests run with permissions to create arbitrary directories), so
        // it does not exercise a real failure. A path containing a NUL byte is rejected by the
        // filesystem regardless of privileges, which is what this test needs to verify.
        var options = new ExportOptions
        {
            Format = ExportFormat.PNG,
            DirectoryPath = "/invalid/nonexistent/path",
            FileName = "test\0invalid.png"
        };

        // Act
        var result = exportService.Export(chart, options);

        // Assert
        result.Success.Should().BeFalse();
    }

    // ---------------------------------------------------------------
    // Concurrent rendering tests
    // ---------------------------------------------------------------

    [Fact]
    public async Task CanRenderMultipleChartsInParallel()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var charts = Enumerable.Range(1, 5)
            .Select(i =>
            {
                var chart = new Chart($"parallel-chart-{i}");
                for (int j = 1; j <= 6; j++)
                {
                    var series = new ChartSeries($"Series {j}");
                    series.DataPoints.Add(new DataPoint(i, j * 100));
                    chart.AddSeries(series);
                }
                return chart;
            })
            .ToList();

        // Act
        var tasks = charts.Select(c => Task.Run(() =>
        {
            var result = renderingService.RenderToByteArray(c);
            return result.Success;
        }));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().OnlyContain(r => r);
    }

    [Fact]
    public async Task CanExportMultipleChartsInParallel()
    {
        // Arrange
        var exportService = _serviceProvider.GetRequiredService<IExportService>();
        var charts = Enumerable.Range(1, 3)
            .Select(i => CreateLineChartWithData())
            .ToList();

        // Act
        var tasks = charts.Select((c, i) =>
        {
            var options = new ExportOptions
            {
                Format = ExportFormat.PNG,
                DirectoryPath = _tempOutputDir,
                FileName = $"parallel-{i}.png"
            };
            return exportService.ExportAsync(c, options);
        });
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        Directory.EnumerateFiles(_tempOutputDir).Count().Should().BeGreaterThanOrEqualTo(3);
    }

    // ---------------------------------------------------------------
    // Edge case tests
    // ---------------------------------------------------------------

    /// <summary>
    /// Tests that a chart with a single data point can be successfully rendered.
    /// Verifies that even minimal chart data produces valid rendering output.
    /// </summary>
    [Fact]
    public void CanRenderChartWithSingleDataPoint()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = new Chart("single-point");
        var series = new ChartSeries("Data");
        series.AddDataPoint(5.0, 100.0);
        chart.AddSeries(series);

        // Act
        var result = renderingService.RenderToByteArray(chart);

        // Assert
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that a chart with a large number of data points (1000) can be successfully rendered.
    /// Verifies that the rendering engine can handle large datasets without failing.
    /// </summary>
    [Fact]
    public void CanRenderChartWithLargeNumberOfDataPoints()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = new Chart("large-dataset");
        var series = new ChartSeries("Data");

        for (int i = 0; i < 1000; i++)
            series.AddDataPoint(i, 100 + (i % 50));

        chart.AddSeries(series);

        // Act
        var result = renderingService.RenderToByteArray(chart);

        // Assert
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that a chart with negative values can be successfully rendered.
    /// Verifies that the rendering engine properly handles negative data points in charts.
    /// </summary>
    [Fact]
    public void CanRenderChartWithNegativeValues()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = new Chart("negative-values");
        var series = new ChartSeries("Data");

        series.AddDataPoint(1.0, -50.0);
        series.AddDataPoint(2.0, 100.0);
        series.AddDataPoint(3.0, -25.0);
        series.AddDataPoint(4.0, 75.0);

        chart.AddSeries(series);

        // Act
        var result = renderingService.RenderToByteArray(chart);

        // Assert
        result.Success.Should().BeTrue();
    }

    /// <summary>
    /// Tests that a chart with zero values can be successfully rendered.
    /// Verifies that charts containing only zero values are handled correctly by the rendering engine.
    /// </summary>
    [Fact]
    public void CanRenderChartWithZeroValues()
    {
        // Arrange
        var renderingService = _serviceProvider.GetRequiredService<IChartRenderingService>();
        var chart = new Chart("zero-values");
        var series = new ChartSeries("Data");

        series.AddDataPoint(1.0, 0.0);
        series.AddDataPoint(2.0, 0.0);
        series.AddDataPoint(3.0, 0.0);

        chart.AddSeries(series);

        // Act
        var result = renderingService.RenderToByteArray(chart);

        // Assert
        result.Success.Should().BeTrue();
    }
}
