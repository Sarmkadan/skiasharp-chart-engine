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

public class ChartRenderingIntegrationTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _tempOutputDir;

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

    public void Dispose()
    {
        if (Directory.Exists(_tempOutputDir))
            Directory.Delete(_tempOutputDir, true);
        (_serviceProvider as IDisposable)?.Dispose();
    }

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

    [Fact]
    public void ExportFailsWithInvalidPath()
    {
        // Arrange
        var exportService = _serviceProvider.GetRequiredService<IExportService>();
        var chart = CreateLineChartWithData();
        var options = new ExportOptions
        {
            Format = ExportFormat.PNG,
            DirectoryPath = "/invalid/nonexistent/path",
            FileName = "test.png"
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
                    chart.AddSeries(new ChartSeries($"Series {j}")).DataPoints.Add(
                        new DataPoint(i, j * 100));
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
        results.Should().AllBe(true);
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
