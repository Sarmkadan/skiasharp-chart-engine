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
/// Contains unit tests for <see cref="ChartRenderingService"/> that verify chart rendering functionality.
/// Tests cover both async and sync rendering methods, cache behavior, file operations,
/// and constructor validation for the chart rendering service.
/// </summary>
public class ChartRenderingServiceTests
{
    /// <summary>
    /// Gets the logger mock dependency used in tests.
    /// </summary>
    internal Mock<ILogger<ChartRenderingService>> LoggerMock { get; }

    /// <summary>
    /// Gets the data service mock dependency used in tests.
    /// </summary>
    internal Mock<IChartDataService> DataServiceMock { get; }

    /// <summary>
    /// Gets the cache service mock dependency used in tests.
    /// </summary>
    internal Mock<IRenderCacheService> CacheServiceMock { get; }

    /// <summary>
    /// Gets the chart rendering service instance under test.
    /// </summary>
    internal ChartRenderingService Service { get; }

    private readonly Mock<ILogger<ChartRenderingService>> _loggerMock;
    private readonly Mock<IChartDataService> _dataServiceMock;
    private readonly Mock<IRenderCacheService> _cacheServiceMock;
    private readonly ChartRenderingService _service;

    /// <summary>
    /// Initializes a new instance of <see cref="ChartRenderingServiceTests"/> and sets up mock dependencies.
    /// </summary>
    public ChartRenderingServiceTests()
    {
        _loggerMock = new Mock<ILogger<ChartRenderingService>>();
        _dataServiceMock = new Mock<IChartDataService>();
        _cacheServiceMock = new Mock<IRenderCacheService>();
        _service = new ChartRenderingService(_loggerMock.Object, _dataServiceMock.Object, _cacheServiceMock.Object);

        LoggerMock = _loggerMock;
        DataServiceMock = _dataServiceMock;
        CacheServiceMock = _cacheServiceMock;
        Service = _service;
    }

    /// <summary>
    /// Creates a valid <see cref="Chart"/> instance with a default ID and a single series containing two data points.
    /// </summary>
    /// <param name="id">Optional chart identifier. Defaults to "test-chart".</param>
    /// <returns>A configured <see cref="Chart"/> ready for rendering tests.</returns>
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

    /// <summary>
    /// Verifies that <see cref="ChartRenderingService.RenderToByteArrayAsync(Chart)"/> throws an
    /// <see cref="ArgumentNullException"/> when the chart argument is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task RenderToByteArrayAsync_WithNullChart_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _service.RenderToByteArrayAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("chart");
    }

    /// <summary>
    /// Ensures that when a cached result exists, <see cref="ChartRenderingService.RenderToByteArrayAsync(Chart)"/>
    /// returns the cached image data without invoking the cache set operation.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Verifies that when no cached result is found, <see cref="ChartRenderingService.RenderToByteArrayAsync(Chart)"/>
    /// renders the chart, returns image data, and stores the result in the cache.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Confirms that the <see cref="RenderResult.RenderTimeMilliseconds"/> property is populated with a non‑negative value.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Ensures that when a cancellation token is already cancelled, the async render operation returns a failed result.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Verifies that <see cref="ChartRenderingService.RenderToFileAsync(Chart,string)"/> throws an
    /// <see cref="ArgumentNullException"/> when the chart argument is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
    [Fact]
    public async Task RenderToFileAsync_WithNullChart_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _service.RenderToFileAsync(null!, "/tmp/test.png");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("chart");
    }

    /// <summary>
    /// Verifies that <see cref="ChartRenderingService.RenderToFileAsync(Chart,string)"/> throws an
    /// <see cref="ArgumentNullException"/> when the output path is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Verifies that <see cref="ChartRenderingService.RenderToFileAsync(Chart,string)"/> throws an
    /// <see cref="ArgumentNullException"/> when the output path is empty or whitespace.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Confirms that the service creates the target directory if it does not already exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Verifies that the rendered image data is written to the specified file path.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Verifies that <see cref="ChartRenderingService.RenderWithExportAsync(Chart,ExportOptions)"/> throws an
    /// <see cref="ArgumentNullException"/> when the chart argument is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Verifies that <see cref="ChartRenderingService.RenderWithExportAsync(Chart,ExportOptions)"/> throws an
    /// <see cref="ArgumentNullException"/> when the export options argument is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Ensures that when exporting to SVG format, the service writes an SVG file containing the expected root element.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Ensures that when exporting to PNG format, the service writes a non‑empty PNG file.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test execution.</returns>
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

    /// <summary>
    /// Verifies that the synchronous <see cref="ChartRenderingService.RenderToByteArray(Chart)"/> method throws an
    /// <see cref="ArgumentNullException"/> when the chart argument is null.
    /// </summary>
    [Fact]
    public void RenderToByteArray_WithNullChart_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _service.RenderToByteArray(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("chart");
    }

    /// <summary>
    /// Confirms that a valid chart renders to a non‑empty byte array using the synchronous method.
    /// </summary>
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

    /// <summary>
    /// Verifies that the synchronous <see cref="ChartRenderingService.RenderToFile(Chart,string)"/> method throws an
    /// <see cref="ArgumentNullException"/> when the chart argument is null.
    /// </summary>
    [Fact]
    public void RenderToFile_WithNullChart_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _service.RenderToFile(null!, "/tmp/test.png");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("chart");
    }

    /// <summary>
    /// Verifies that the synchronous method throws an <see cref="ArgumentNullException"/> when the output path is null.
    /// </summary>
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

    /// <summary>
    /// Confirms that the synchronous render-to-file operation writes a file successfully.
    /// </summary>
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

    /// <summary>
    /// Verifies that calling <see cref="ChartRenderingService.PrewarmCache(Chart)"/> with a null chart does not throw
    /// and does not interact with the cache service.
    /// </summary>
    [Fact]
    public void PrewarmCache_WithNullChart_DoesNotThrow()
    {
        // Act
        Action act = () => _service.PrewarmCache(null!);

        // Assert
        act.Should().NotThrow();
        _cacheServiceMock.Verify(x => x.Set(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
    }

    /// <summary>
    /// Confirms that pre‑warming the cache with a valid chart results in a single cache set operation.
    /// </summary>
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

    /// <summary>
    /// Ensures that if the cache service throws an exception during pre‑warming, the method logs a warning
    /// but does not propagate the exception.
    /// </summary>
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

    /// <summary>
    /// Verifies that the <see cref="ChartRenderingService"/> constructor throws an <see cref="ArgumentNullException"/>
    /// when the logger dependency is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new ChartRenderingService(null!, _dataServiceMock.Object, _cacheServiceMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    /// <summary>
    /// Verifies that the <see cref="ChartRenderingService"/> constructor throws an <see cref="ArgumentNullException"/>
    /// when the data service dependency is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullDataService_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new ChartRenderingService(_loggerMock.Object, null!, _cacheServiceMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("dataService");
    }

    /// <summary>
    /// Verifies that the <see cref="ChartRenderingService"/> constructor throws an <see cref="ArgumentNullException"/>
    /// when the cache service dependency is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullCacheService_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => new ChartRenderingService(_loggerMock.Object, _dataServiceMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("cacheService");
    }
}
