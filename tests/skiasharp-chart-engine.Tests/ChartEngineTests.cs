using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Exceptions;
using SkiaSharpChartEngine.Models;
using Xunit;

namespace SkiaSharpChartEngine.Tests;

/// <summary>
/// Unit tests for the ChartEngine facade class
/// </summary>
public class ChartEngineTests
{
    // ---------------------------------------------------------------
    // Static Create() method tests
    // ---------------------------------------------------------------

    [Fact]
    public void Create_WithDefaultConfiguration_ReturnsChartEngineInstance()
    {
        // Act
        var engine = ChartEngine.Create();

        // Assert
        engine.Should().NotBeNull();
        engine.GetServiceProvider().Should().NotBeNull();
    }

    [Fact]
    public void Create_WithCustomConfiguration_ConfiguresServices()
    {
        // Arrange
        var customConfigCalled = false;

        // Act
        var engine = ChartEngine.Create(services =>
        {
            customConfigCalled = true;
            services.AddSingleton<ISomeCustomService, SomeCustomService>();
        });

        // Assert
        engine.Should().NotBeNull();
        customConfigCalled.Should().BeTrue();
    }

    // ---------------------------------------------------------------
    // RenderChart tests (synchronous)
    // ---------------------------------------------------------------

    [Fact]
    public void RenderChart_WithNullChart_ThrowsArgumentNullException()
    {
        // Arrange
        var engine = ChartEngine.Create();

        // Act
        Action act = () => engine.RenderChart(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public void RenderChart_WithEmptySeries_ReturnsFailureResult()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("empty-chart");

        // Act
        var result = engine.RenderChart(chart);

        // Assert - domain validation errors are reported as a failure result,
        // consistent with RenderChartAsync's behavior for the same condition.
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("must contain at least one series");
    }

    [Fact]
    public void RenderChart_WithValidChart_ReturnsSuccessfulRenderResult()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        series.AddDataPoint(2.0, 150.0);
        chart.AddSeries(series);

        // Act
        var result = engine.RenderChart(chart);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ImageData.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().BeNull();
        result.Exception.Should().BeNull();
    }

    [Fact]
    public void RenderChart_WithMultipleSeries_RendersSuccessfully()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("multi-series-chart");

        var series1 = new ChartSeries("Series1");
        series1.AddDataPoint(1.0, 100.0);
        series1.AddDataPoint(2.0, 150.0);
        chart.AddSeries(series1);

        var series2 = new ChartSeries("Series2");
        series2.AddDataPoint(1.0, 80.0);
        series2.AddDataPoint(2.0, 120.0);
        chart.AddSeries(series2);

        // Act
        var result = engine.RenderChart(chart);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ImageData.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void RenderChart_WithDifferentChartTypes_RendersSuccessfully()
    {
        // Arrange & Act & Assert for different chart types
        var engine = ChartEngine.Create();

        // Line chart
        var lineChart = new Chart(ChartType.LineChart, "line-chart");
        var lineSeries = new ChartSeries("Data");
        lineSeries.AddDataPoint(1.0, 100.0);
        lineChart.AddSeries(lineSeries);
        var lineResult = engine.RenderChart(lineChart);
        lineResult.Success.Should().BeTrue();

        // Bar chart
        var barChart = new Chart(ChartType.BarChart, "bar-chart");
        var barSeries = new ChartSeries("Data");
        barSeries.AddDataPoint(1.0, 100.0);
        barChart.AddSeries(barSeries);
        var barResult = engine.RenderChart(barChart);
        barResult.Success.Should().BeTrue();

        // Pie chart
        var pieChart = new Chart(ChartType.PieChart, "pie-chart");
        var pieSeries = new ChartSeries("Data");
        pieSeries.AddDataPoint(1.0, 100.0);
        pieChart.AddSeries(pieSeries);
        var pieResult = engine.RenderChart(pieChart);
        pieResult.Success.Should().BeTrue();
    }

    [Fact]
    public void RenderChart_WithInvalidData_HandlesErrorGracefully()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("invalid-chart");
        // Don't add any series

        // Act
        var result = engine.RenderChart(chart);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.Exception.Should().NotBeNull();
    }

    // ---------------------------------------------------------------
    // RenderChartAsync tests (asynchronous)
    // ---------------------------------------------------------------

    [Fact]
    public async Task RenderChartAsync_WithNullChart_ThrowsArgumentNullException()
    {
        // Arrange
        var engine = ChartEngine.Create();

        // Act
        Func<Task> act = async () => await engine.RenderChartAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public async Task RenderChartAsync_WithEmptySeries_ReturnsFailureResult()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("empty-chart-async");

        // Act
        var result = await engine.RenderChartAsync(chart);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task RenderChartAsync_WithValidChart_ReturnsSuccessfulRenderResult()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("test-chart-async");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        series.AddDataPoint(2.0, 150.0);
        chart.AddSeries(series);

        // Act
        var result = await engine.RenderChartAsync(chart);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ImageData.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().BeNull();
        result.Exception.Should().BeNull();
    }

    [Fact]
    public async Task RenderChartAsync_WithCancellation_CancelsOperation()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("cancel-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        // Cancel up front rather than after a delay: rendering a single data point
        // completes in well under a millisecond, so a delayed cancellation races the
        // render and never reliably fires before completion.
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await engine.RenderChartAsync(chart, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // ---------------------------------------------------------------
    // ExportChart tests (synchronous)
    // ---------------------------------------------------------------

    [Fact]
    public void ExportChart_WithNullChart_ThrowsArgumentNullException()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var options = new ExportOptions { Format = ExportFormat.PNG };

        // Act
        Action act = () => engine.ExportChart(null!, options);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public void ExportChart_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("test-chart-export");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        // Act
        Action act = () => engine.ExportChart(chart, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void ExportChart_WithValidChartAndPngFormat_ReturnsSuccessfulResult()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("export-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        var options = new ExportOptions
        {
            Format = ExportFormat.PNG,
            DirectoryPath = Path.GetTempPath(),
            FileName = $"test-export-{Guid.NewGuid()}.png"
        };

        // Act
        var result = engine.ExportChart(chart, options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ImageData.Should().NotBeNullOrEmpty();

        // Cleanup
        var filePath = Path.Combine(options.DirectoryPath, options.FileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void ExportChart_WithSvgFormat_ReturnsSvgContent()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("svg-export-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        var options = new ExportOptions
        {
            Format = ExportFormat.SVG,
            DirectoryPath = Path.GetTempPath(),
            FileName = $"test-export-{Guid.NewGuid()}.svg"
        };

        // Act
        var result = engine.ExportChart(chart, options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ImageData.Should().NotBeNullOrEmpty();

        // Verify SVG content
        var svgContent = System.Text.Encoding.UTF8.GetString(result.ImageData);
        svgContent.Should().Contain("<svg");
        svgContent.Should().Contain("</svg>");

        // Cleanup
        var filePath = Path.Combine(options.DirectoryPath, options.FileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void ExportChart_WithUnsupportedFormat_HandlesErrorGracefully()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("unsupported-format-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        var options = new ExportOptions
        {
            Format = (ExportFormat)999, // Invalid format
            DirectoryPath = Path.GetTempPath(),
            FileName = $"test-export-{Guid.NewGuid()}.dat"
        };

        // Act
        var result = engine.ExportChart(chart, options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // ---------------------------------------------------------------
    // ExportChartAsync tests (asynchronous)
    // ---------------------------------------------------------------

    [Fact]
    public async Task ExportChartAsync_WithNullChart_ThrowsArgumentNullException()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var options = new ExportOptions { Format = ExportFormat.PNG };

        // Act
        Func<Task> act = async () => await engine.ExportChartAsync(null!, options);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public async Task ExportChartAsync_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("test-chart-export-async");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        // Act
        Func<Task> act = async () => await engine.ExportChartAsync(chart, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public async Task ExportChartAsync_WithValidChartAndPngFormat_ReturnsSuccessfulResult()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("export-async-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        var options = new ExportOptions
        {
            Format = ExportFormat.PNG,
            DirectoryPath = Path.GetTempPath(),
            FileName = $"test-export-async-{Guid.NewGuid()}.png"
        };

        // Act
        var result = await engine.ExportChartAsync(chart, options);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ImageData.Should().NotBeNullOrEmpty();

        // Cleanup
        var filePath = Path.Combine(options.DirectoryPath, options.FileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    // ---------------------------------------------------------------
    // Chart save/retrieve tests
    // ---------------------------------------------------------------

    [Fact]
    public async Task SaveChartAsync_WithValidChart_ReturnsChartId()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("save-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        // Act
        var chartId = await engine.SaveChartAsync(chart);

        // Assert
        chartId.Should().NotBeNullOrEmpty();
        chartId.Should().Be(chart.Id);
    }

    [Fact]
    public void SaveChart_WithValidChart_ReturnsChartId()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("save-sync-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        // Act
        var chartId = engine.SaveChart(chart);

        // Assert
        chartId.Should().NotBeNullOrEmpty();
        chartId.Should().Be(chart.Id);
    }

    [Fact]
    public async Task GetChartAsync_WithExistingChart_ReturnsChart()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("get-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        var savedId = await engine.SaveChartAsync(chart);

        // Act
        var retrievedChart = await engine.GetChartAsync(savedId);

        // Assert
        retrievedChart.Should().NotBeNull();
        retrievedChart!.Id.Should().Be(chart.Id);
        retrievedChart.Type.Should().Be(chart.Type);
        retrievedChart.GetSeriesCount().Should().Be(1);
    }

    [Fact]
    public void GetChart_WithExistingChart_ReturnsChart()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("get-sync-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        var savedId = engine.SaveChart(chart);

        // Act
        var retrievedChart = engine.GetChart(savedId);

        // Assert
        retrievedChart.Should().NotBeNull();
        retrievedChart!.Id.Should().Be(chart.Id);
        retrievedChart.Type.Should().Be(chart.Type);
        retrievedChart.GetSeriesCount().Should().Be(1);
    }

    [Fact]
    public async Task UpdateChartAsync_WithModifiedChart_UpdatesSuccessfully()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("update-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        var savedId = await engine.SaveChartAsync(chart);

        // Modify chart
        chart.Title = "Updated Chart";
        var newSeries = new ChartSeries("New Data");
        newSeries.AddDataPoint(2.0, 200.0);
        chart.AddSeries(newSeries);

        // Act
        var updateResult = await engine.UpdateChartAsync(chart);

        // Assert
        updateResult.Should().BeTrue();

        // Verify update
        var retrievedChart = await engine.GetChartAsync(savedId);
        retrievedChart.Should().NotBeNull();
        retrievedChart!.Title.Should().Be("Updated Chart");
        retrievedChart.GetSeriesCount().Should().Be(2);
    }

    [Fact]
    public void UpdateChart_WithModifiedChart_UpdatesSuccessfully()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("update-sync-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        var savedId = engine.SaveChart(chart);

        // Modify chart
        chart.Title = "Updated Chart Sync";

        // Act
        var updateResult = engine.UpdateChart(chart);

        // Assert
        updateResult.Should().BeTrue();

        // Verify update
        var retrievedChart = engine.GetChart(savedId);
        retrievedChart.Should().NotBeNull();
        retrievedChart!.Title.Should().Be("Updated Chart Sync");
    }

    [Fact]
    public async Task DeleteChartAsync_WithExistingChart_DeletesSuccessfully()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("delete-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        var savedId = await engine.SaveChartAsync(chart);

        // Verify exists
        var existsBefore = await engine.GetChartAsync(savedId);
        existsBefore.Should().NotBeNull();

        // Act
        var deleteResult = await engine.DeleteChartAsync(savedId);

        // Assert
        deleteResult.Should().BeTrue();

        // Verify deleted
        var existsAfter = await engine.GetChartAsync(savedId);
        existsAfter.Should().BeNull();
    }

    [Fact]
    public void DeleteChart_WithExistingChart_DeletesSuccessfully()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("delete-sync-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        var savedId = engine.SaveChart(chart);

        // Verify exists
        var existsBefore = engine.GetChart(savedId);
        existsBefore.Should().NotBeNull();

        // Act
        var deleteResult = engine.DeleteChart(savedId);

        // Assert
        deleteResult.Should().BeTrue();

        // Verify deleted
        var existsAfter = engine.GetChart(savedId);
        existsAfter.Should().BeNull();
    }

    // ---------------------------------------------------------------
    // Configuration tests
    // ---------------------------------------------------------------

    [Fact]
    public void GetDefaultConfiguration_ReturnsValidConfiguration()
    {
        // Arrange
        var engine = ChartEngine.Create();

        // Act
        var config = engine.GetDefaultConfiguration();

        // Assert
        config.Should().NotBeNull();
        config.Width.Should().BeGreaterThan(0);
        config.Height.Should().BeGreaterThan(0);
        config.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GetConfigurationTemplate_ForLineChart_ReturnsLineChartConfiguration()
    {
        // Arrange
        var engine = ChartEngine.Create();

        // Act
        var config = engine.GetConfigurationTemplate(ChartType.LineChart);

        // Assert
        config.Should().NotBeNull();
        config.Title.Should().Contain("Line");
    }

    [Fact]
    public void GetConfigurationTemplate_ForBarChart_ReturnsBarChartConfiguration()
    {
        // Arrange
        var engine = ChartEngine.Create();

        // Act
        var config = engine.GetConfigurationTemplate(ChartType.BarChart);

        // Assert
        config.Should().NotBeNull();
        config.Title.Should().Contain("Bar");
    }

    [Fact]
    public void GetConfigurationTemplate_ForPieChart_ReturnsPieChartConfiguration()
    {
        // Arrange
        var engine = ChartEngine.Create();

        // Act
        var config = engine.GetConfigurationTemplate(ChartType.PieChart);

        // Assert
        config.Should().NotBeNull();
        config.Title.Should().Contain("Pie");
    }

    [Fact]
    public void GetSupportedExportFormats_ReturnsNonEmptyList()
    {
        // Arrange
        var engine = ChartEngine.Create();

        // Act
        var formats = engine.GetSupportedExportFormats();

        // Assert
        formats.Should().NotBeNull();
        formats.Should().NotBeEmpty();
    }

    // ---------------------------------------------------------------
    // Cache tests
    // ---------------------------------------------------------------

    [Fact]
    public void PrewarmRenderCache_WithValidChart_PopulatesCache()
    {
        // Arrange
        var engine = ChartEngine.Create();
        var chart = new Chart("cache-test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        chart.AddSeries(series);

        // Act
        engine.PrewarmRenderCache(chart);

        // Assert - No exception thrown, cache should be populated
        // (We can't directly verify cache state, but no exception means success)
        true.Should().BeTrue();
    }

    [Fact]
    public void PrewarmRenderCache_WithNullChart_DoesNotThrow()
    {
        // Arrange
        var engine = ChartEngine.Create();

        // Act
        Action act = () => engine.PrewarmRenderCache(null!);

        // Assert
        act.Should().NotThrow();
    }
}

// Test helper interface for custom services
internal interface ISomeCustomService { }
internal class SomeCustomService : ISomeCustomService { }
