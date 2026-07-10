using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ChartRenderingService"/> that verify chart rendering functionality.
/// Tests cover both async and sync rendering methods, cache behavior, file operations,
/// and constructor validation for the chart rendering service.
/// </summary>
public class ChartRenderingServiceTests
{
    private readonly Mock<ILogger<ChartRenderingService>> _loggerMock;
    private readonly Mock<IChartDataService> _dataServiceMock;
    private readonly Mock<IRenderCacheService> _cacheServiceMock;
    private readonly ChartRenderingService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartRenderingServiceTests"/> class.
    /// Sets up mock dependencies for logger, chart data service, and cache service,
    /// and creates an instance of <see cref="ChartRenderingService"/> for testing.
    /// </summary>
    public ChartRenderingServiceTests()
    {
        _loggerMock = new Mock<ILogger<ChartRenderingService>>();
        _dataServiceMock = new Mock<IChartDataService>();
        _cacheServiceMock = new Mock<IRenderCacheService>();
        _service = new ChartRenderingService(_loggerMock.Object, _dataServiceMock.Object, _cacheServiceMock.Object);
    }

    /// <summary>
    /// Creates a valid chart for testing with default ID "test-chart".
    /// The chart contains one series with two data points at x-values 1.0 and 2.0.
    /// </summary>
    /// <param name="id">Optional chart identifier. Defaults to "test-chart".</param>
    /// <returns>A configured <see cref="Chart"/> instance ready for rendering tests.</returns>
    private Chart CreateValidChart(string id = "test-chart")
    {
        var chart = new Chart(id);
        var series = new ChartSeries("Data");
        series.AddDataPoint(1.0, 100.0);
        series.AddDataPoint(2.0, 150.0);
        chart.AddSeries(series);
        return chart;
    }

    // ---------------------------------------------------------------
    // RenderToByteArrayAsync tests
    // ---------------------------------------------------------------

    [Fact]
    public async Task RenderToByteArrayAsync_WithNullChart_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _service.RenderToByteArrayAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public async Task RenderToByteArrayAsync_WithCachedResult_ReturnsCachedData()
    {
        // Arrange
        var chart = CreateValidChart();
        var cachedData = new byte[] { 1, 2, 3, 4, 5 };
        _cacheServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns(cachedData);

        // Act
        var result = await _service.RenderToByteArrayAsync(chart);

        // Assert
        result.Success.Should().BeTrue();
        result.ImageData.Should().Equal(cachedData);
        _cacheServiceMock.Verify(x => x.Get(It.IsAny<string>()), Times.Once);
        _cacheServiceMock.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task RenderToByteArrayAsync_WithoutCachedResult_RendersAndCaches()
    {
        // Arrange
        var chart = CreateValidChart();
        _cacheServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns((byte[])null!);

        // Act
        var result = await _service.RenderToByteArrayAsync(chart);

        // Assert
        result.Success.Should().BeTrue();
        result.ImageData.Should().NotBeNull();
        result.ImageData.Should().NotBeEmpty();
        _cacheServiceMock.Verify(x => x.Get(It.IsAny<string>()), Times.Once);
        _cacheServiceMock.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task RenderToByteArrayAsync_RecordsRenderTime()
    {
        // Arrange
        var chart = CreateValidChart();
        _cacheServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns((byte[])null!);

        // Act
        var result = await _service.RenderToByteArrayAsync(chart);

        // Assert
        result.RenderTimeMilliseconds.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task RenderToByteArrayAsync_WithCancellation_StopsOperation()
    {
        // Arrange
        var chart = CreateValidChart();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        _cacheServiceMock.Setup(x => x.Get(It.IsAny<string>())).Returns((byte[])null!);

        // Act
        var result = await _service.RenderToByteArrayAsync(chart, cts.Token);

        // Assert
        result.Success.Should().BeFalse();
    }

    // ---------------------------------------------------------------
    // RenderToFileAsync tests
    // ---------------------------------------------------------------

    [Fact]
    public async Task RenderToFileAsync_WithNullChart_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _service.RenderToFileAsync(null!, "/tmp/test.png");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public async Task RenderToFileAsync_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var chart = CreateValidChart();

        // Act
        Func<Task> act = async () => await _service.RenderToFileAsync(chart, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("outputPath");
    }

    [Fact]
    public async Task RenderToFileAsync_WithEmptyPath_ThrowsArgumentNullException()
    {
        // Arrange
        var chart = CreateValidChart();

        // Act
        Func<Task> act = async () => await _service.RenderToFileAsync(chart, "   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("outputPath");
    }

    [Fact]
    public async Task RenderToFileAsync_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var chart = CreateValidChart();
        var tempPath = Path.Combine(Path.GetTempPath(), "test-chart-" + Guid.NewGuid());
        var outputPath = Path.Combine(tempPath, "subdir", "chart.png");

        try
        {
            // Act
            var result = await _service.RenderToFileAsync(chart, outputPath);

            // Assert
            result.Success.Should().BeTrue();
            Directory.Exists(tempPath).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public async Task RenderToFileAsync_WritesImageDataToFile()
    {
        // Arrange
        var chart = CreateValidChart();
        var tempPath = Path.Combine(Path.GetTempPath(), "chart-" + Guid.NewGuid() + ".png");

        try
        {
            // Act
            var result = await _service.RenderToFileAsync(chart, tempPath);

            // Assert
            result.Success.Should().BeTrue();
            File.Exists(tempPath).Should().BeTrue();
            new FileInfo(tempPath).Length.Should().BeGreaterThan(0);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    // ---------------------------------------------------------------
    // RenderWithExportAsync tests
    // ---------------------------------------------------------------

    [Fact]
    public async Task RenderWithExportAsync_WithNullChart_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new ExportOptions { Format = ExportFormat.PNG };

        // Act
        Func<Task> act = async () => await _service.RenderWithExportAsync(null!, options);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public async Task RenderWithExportAsync_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var chart = CreateValidChart();

        // Act
        Func<Task> act = async () => await _service.RenderWithExportAsync(chart, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("exportOptions");
    }

    [Fact]
    public async Task RenderWithExportAsync_WithSvgFormat_WritesSvgText()
    {
        // Arrange
        var chart = CreateValidChart();
        var tempPath = Path.Combine(Path.GetTempPath(), "chart-" + Guid.NewGuid() + ".svg");
        var options = new ExportOptions
        {
            Format = ExportFormat.SVG,
            DirectoryPath = Path.GetDirectoryName(tempPath),
            FileName = Path.GetFileName(tempPath)
        };

        try
        {
            // Act
            var result = await _service.RenderWithExportAsync(chart, options);

            // Assert
            result.Success.Should().BeTrue();
            File.Exists(tempPath).Should().BeTrue();
            var content = File.ReadAllText(tempPath);
            content.Should().Contain("<svg");
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task RenderWithExportAsync_WithPngFormat_WritesPngBytes()
    {
        // Arrange
        var chart = CreateValidChart();
        var tempPath = Path.Combine(Path.GetTempPath(), "chart-" + Guid.NewGuid() + ".png");
        var options = new ExportOptions
        {
            Format = ExportFormat.PNG,
            DirectoryPath = Path.GetDirectoryName(tempPath),
            FileName = Path.GetFileName(tempPath)
        };

        try
        {
            // Act
            var result = await _service.RenderWithExportAsync(chart, options);

            // Assert
            result.Success.Should().BeTrue();
            File.Exists(tempPath).Should().BeTrue();
            var bytes = File.ReadAllBytes(tempPath);
            bytes.Length.Should().BeGreaterThan(0);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    // ---------------------------------------------------------------
    // RenderToByteArray (sync) tests
    // ---------------------------------------------------------------

    [Fact]
    public void RenderToByteArray_WithNullChart_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _service.RenderToByteArray(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public void RenderToByteArray_WithValidChart_ReturnsNonEmptyByteArray()
    {
        // Arrange
        var chart = CreateValidChart();

        // Act
        var result = _service.RenderToByteArray(chart);

        // Assert
        result.Success.Should().BeTrue();
        result.ImageData.Should().NotBeNull();
        result.ImageData.Should().NotBeEmpty();
    }

    // ---------------------------------------------------------------
    // RenderToFile (sync) tests
    // ---------------------------------------------------------------

    [Fact]
    public void RenderToFile_WithNullChart_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _service.RenderToFile(null!, "/tmp/test.png");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public void RenderToFile_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange
        var chart = CreateValidChart();

        // Act
        Action act = () => _service.RenderToFile(chart, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("outputPath");
    }

    [Fact]
    public void RenderToFile_WritesFileSuccessfully()
    {
        // Arrange
        var chart = CreateValidChart();
        var tempPath = Path.Combine(Path.GetTempPath(), "chart-" + Guid.NewGuid() + ".png");

        try
        {
            // Act
            var result = _service.RenderToFile(chart, tempPath);

            // Assert
            result.Success.Should().BeTrue();
            File.Exists(tempPath).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    // ---------------------------------------------------------------
    // PrewarmCache tests
    // ---------------------------------------------------------------

    [Fact]
    public void PrewarmCache_WithNullChart_DoesNotThrow()
    {
        // Act
        Action act = () => _service.PrewarmCache(null!);

        // Assert
        act.Should().NotThrow();
        _cacheServiceMock.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public void PrewarmCache_WithValidChart_PopulatesCache()
    {
        // Arrange
        var chart = CreateValidChart();

        // Act
        _service.PrewarmCache(chart);

        // Assert
        _cacheServiceMock.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void PrewarmCache_WhenRenderingFails_LogsWarningButDoesNotThrow()
    {
        // Arrange
        var chart = CreateValidChart();
        _cacheServiceMock.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Throws(new InvalidOperationException("Cache error"));

        // Act
        Action act = () => _service.PrewarmCache(chart);

        // Assert
        act.Should().NotThrow();
    }

    // ---------------------------------------------------------------
    // Constructor validation
    // ---------------------------------------------------------------

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new ChartRenderingService(null!, _dataServiceMock.Object, _cacheServiceMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_WithNullDataService_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new ChartRenderingService(_loggerMock.Object, null!, _cacheServiceMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("dataService");
    }

    [Fact]
    public void Constructor_WithNullCacheService_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new ChartRenderingService(_loggerMock.Object, _dataServiceMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("cacheService");
    }
}
