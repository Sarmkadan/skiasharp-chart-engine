using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Exceptions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Services;

public class ExportServiceTests
{
    private readonly Mock<IChartRenderingService> _renderingServiceMock;
    private readonly Mock<ILogger<ExportService>> _loggerMock;
    private readonly ExportService _service;

    public ExportServiceTests()
    {
        _renderingServiceMock = new Mock<IChartRenderingService>();
        _loggerMock = new Mock<ILogger<ExportService>>();
        _service = new ExportService(_renderingServiceMock.Object, _loggerMock.Object);
    }

    private Chart CreateValidChart()
    {
        var chart = new Chart("test-chart");
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        series.AddDataPoint(2.0, 150.0);
        chart.AddSeries(series);
        return chart;
    }

    // ---------------------------------------------------------------
    // ExportAsync tests
    // ---------------------------------------------------------------

    [Fact]
    public async Task ExportAsync_WithNullChart_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new ExportOptions { Format = ExportFormat.PNG };

        // Act
        Func<Task> act = async () => await _service.ExportAsync(null!, options);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public async Task ExportAsync_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var chart = CreateValidChart();

        // Act
        Func<Task> act = async () => await _service.ExportAsync(chart, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public async Task ExportAsync_WithUnsupportedFormat_ThrowsUnsupportedExportFormatException()
    {
        // Arrange
        var chart = CreateValidChart();
        var options = new ExportOptions { Format = (ExportFormat)999 };

        // Act
        Func<Task> act = async () => await _service.ExportAsync(chart, options);

        // Assert
        await act.Should().ThrowAsync<UnsupportedExportFormatException>();
    }

    [Fact]
    public async Task ExportAsync_WithSupportedPngFormat_DelegatesRenderingAndReturnsSuccess()
    {
        // Arrange
        var chart = CreateValidChart();
        var options = new ExportOptions { Format = ExportFormat.PNG, DirectoryPath = "/tmp", FileName = "chart.png" };
        var expectedResult = RenderResult.CreateSuccess(chart.Id, new byte[] { 1, 2, 3 }, 100, ExportFormat.PNG);
        _renderingServiceMock.Setup(x => x.RenderWithExportAsync(chart, options, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ExportAsync(chart, options);

        // Assert
        result.Success.Should().BeTrue();
        _renderingServiceMock.Verify(x => x.RenderWithExportAsync(chart, options, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExportAsync_WithSupportedSvgFormat_DelegatesRenderingAndReturnsSuccess()
    {
        // Arrange
        var chart = CreateValidChart();
        var options = new ExportOptions { Format = ExportFormat.SVG, DirectoryPath = "/tmp", FileName = "chart.svg" };
        var expectedResult = RenderResult.CreateSuccess(chart.Id, "/tmp/chart.svg", 100, ExportFormat.SVG);
        _renderingServiceMock.Setup(x => x.RenderWithExportAsync(chart, options, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ExportAsync(chart, options);

        // Assert
        result.Success.Should().BeTrue();
        _renderingServiceMock.Verify(x => x.RenderWithExportAsync(chart, options, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExportAsync_WhenRenderingServiceReturnsFailure_ReturnsFailureResult()
    {
        // Arrange
        var chart = CreateValidChart();
        var options = new ExportOptions { Format = ExportFormat.PNG, DirectoryPath = "/tmp", FileName = "chart.png" };
        var failureResult = RenderResult.CreateFailure(chart.Id, "Rendering failed", new Exception("Test error"));
        _renderingServiceMock.Setup(x => x.RenderWithExportAsync(chart, options, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _service.ExportAsync(chart, options);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Rendering failed");
    }

    [Fact]
    public async Task ExportAsync_WhenInfrastructureErrorOccurs_ReturnsFailureWithErrorMessage()
    {
        // Arrange
        var chart = CreateValidChart();
        var options = new ExportOptions { Format = ExportFormat.PNG, DirectoryPath = "/tmp", FileName = "chart.png" };
        _renderingServiceMock.Setup(x => x.RenderWithExportAsync(chart, options, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("I/O error"));

        // Act
        var result = await _service.ExportAsync(chart, options);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExportAsync_WithCancellation_RespectsCancellationToken()
    {
        // Arrange
        var chart = CreateValidChart();
        var options = new ExportOptions { Format = ExportFormat.PNG, DirectoryPath = "/tmp", FileName = "chart.png" };
        var cts = new CancellationTokenSource();
        cts.Cancel();
        _renderingServiceMock.Setup(x => x.RenderWithExportAsync(chart, options, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _service.ExportAsync(chart, options, cts.Token);

        // Assert
        result.Success.Should().BeFalse();
    }

    // ---------------------------------------------------------------
    // Export (sync) tests
    // ---------------------------------------------------------------

    [Fact]
    public void Export_WithNullChart_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new ExportOptions { Format = ExportFormat.PNG };

        // Act
        Action act = () => _service.Export(null!, options);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public void Export_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var chart = CreateValidChart();

        // Act
        Action act = () => _service.Export(chart, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Export_WithUnsupportedFormat_ThrowsUnsupportedExportFormatException()
    {
        // Arrange
        var chart = CreateValidChart();
        var options = new ExportOptions { Format = (ExportFormat)999 };

        // Act
        Action act = () => _service.Export(chart, options);

        // Assert
        act.Should().Throw<UnsupportedExportFormatException>();
    }

    [Fact]
    public void Export_WithValidChartAndFormat_DelegatesAndReturnsSuccess()
    {
        // Arrange
        var chart = CreateValidChart();
        var options = new ExportOptions { Format = ExportFormat.PNG, DirectoryPath = "/tmp", FileName = "chart.png" };
        var expectedResult = RenderResult.CreateSuccess(chart.Id, "/tmp/chart.png", 100, ExportFormat.PNG);
        _renderingServiceMock.Setup(x => x.RenderToFile(chart, options.GetFullPath()))
            .Returns(expectedResult);

        // Act
        var result = _service.Export(chart, options);

        // Assert
        result.Success.Should().BeTrue();
        _renderingServiceMock.Verify(x => x.RenderToFile(chart, options.GetFullPath()), Times.Once);
    }

    [Fact]
    public void Export_WhenRenderingServiceThrows_ReturnsFailureResult()
    {
        // Arrange
        var chart = CreateValidChart();
        var options = new ExportOptions { Format = ExportFormat.PNG, DirectoryPath = "/tmp", FileName = "chart.png" };
        _renderingServiceMock.Setup(x => x.RenderToFile(chart, options.GetFullPath()))
            .Throws(new InvalidOperationException("Rendering failed"));

        // Act
        var result = _service.Export(chart, options);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Rendering failed");
    }

    // ---------------------------------------------------------------
    // SupportsFormat tests
    // ---------------------------------------------------------------

    [Fact]
    public void SupportsFormat_WithPng_ReturnsTrue()
    {
        // Act
        var result = _service.SupportsFormat(ExportFormat.PNG);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsFormat_WithSvg_ReturnsTrue()
    {
        // Act
        var result = _service.SupportsFormat(ExportFormat.SVG);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsFormat_WithJpeg_ReturnsTrue()
    {
        // Act
        var result = _service.SupportsFormat(ExportFormat.JPEG);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsFormat_WithWebp_ReturnsTrue()
    {
        // Act
        var result = _service.SupportsFormat(ExportFormat.WEBP);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SupportsFormat_WithUnsupportedFormat_ReturnsFalse()
    {
        // Act
        var result = _service.SupportsFormat((ExportFormat)999);

        // Assert
        result.Should().BeFalse();
    }

    // ---------------------------------------------------------------
    // GetSupportedFormats tests
    // ---------------------------------------------------------------

    [Fact]
    public void GetSupportedFormats_ReturnsAllFourFormats()
    {
        // Act
        var formats = _service.GetSupportedFormats();

        // Assert
        formats.Should().HaveCount(4);
        formats.Should().Contain(ExportFormat.PNG);
        formats.Should().Contain(ExportFormat.SVG);
        formats.Should().Contain(ExportFormat.JPEG);
        formats.Should().Contain(ExportFormat.WEBP);
    }

    [Fact]
    public void GetSupportedFormats_ReturnsOrderedFormats()
    {
        // Act
        var formats = new List<ExportFormat>(_service.GetSupportedFormats());

        // Assert - formats are sorted by name
        formats.Should().BeInAscendingOrder(f => f.ToString());
    }

    // ---------------------------------------------------------------
    // Constructor validation
    // ---------------------------------------------------------------

    [Fact]
    public void Constructor_WithNullRenderingService_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new ExportService(null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("renderingService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new ExportService(_renderingServiceMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }
}
