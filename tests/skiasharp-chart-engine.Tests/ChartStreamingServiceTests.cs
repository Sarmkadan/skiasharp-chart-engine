// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using SkiaSharpChartEngine.Streaming;
using Xunit;

/// <summary>
/// Tests for the ChartStreamingService class.
/// </summary>
public class ChartStreamingServiceTests
{
    private readonly Mock<IChartRenderingService> _renderMock;
    private readonly Mock<ILogger<ChartStreamingService>> _loggerMock;
    private readonly ChartStreamingService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartStreamingServiceTests"/> class.
    /// </summary>
    public ChartStreamingServiceTests()
    {
        _renderMock  = new Mock<IChartRenderingService>();
        _loggerMock  = new Mock<ILogger<ChartStreamingService>>();
        _service     = new ChartStreamingService(_renderMock.Object, _loggerMock.Object);
    }

    /// <summary>
    /// Creates a new chart with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the chart.</param>
    /// <returns>A new chart instance.</returns>
    private static Chart CreateChart(string id = "stream-chart")
    {
        var chart = new Chart(id);
        chart.AddSeries(new ChartSeries("Sensor"));
        return chart;
    }

    /// <summary>
    /// Verifies that a new chart is registered successfully.
    /// </summary>
    [Fact]
    public void Register_NewChart_IsRegisteredSuccessfully()
    {
        // Arrange
        var chart = CreateChart();

        // Act
        _service.Register(chart);

        // Assert – GetSnapshot should work without throwing
        var snapshot = _service.GetSnapshot(chart.Id);
        snapshot.Should().NotBeNull();
        snapshot.Id.Should().Be(chart.Id);
    }

    /// <summary>
    /// Verifies that publishing an unregistered chart throws an InvalidOperationException.
    /// </summary>
    [Fact]
    public void Publish_UnregisteredChart_ThrowsInvalidOperationException()
    {
        // Act
        Action act = () => _service.Publish("unknown", new StreamDataPoint { SeriesName = "S", X = 1, Y = 2 });

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("*unknown*");
    }

    /// <summary>
    /// Verifies that publishing a valid point is applied to the snapshot.
    /// </summary>
    [Fact]
    public void Publish_ValidPoint_IsAppliedToSnapshot()
    {
        // Arrange
        var chart = CreateChart();
        _service.Register(chart);

        // Act
        _service.Publish(chart.Id, new StreamDataPoint { SeriesName = "Sensor", X = 1.0, Y = 42.0 });
        var snapshot = _service.GetSnapshot(chart.Id);

        // Assert
        var series = snapshot.GetSeriesByName("Sensor");
        series.Should().NotBeNull();
        series!.GetDataPointCount().Should().Be(1);
        series.DataPoints[0].Y.Should().Be(42.0);
    }

    /// <summary>
    /// Verifies that publishing a batch of points is applied to the snapshot.
    /// </summary>
    [Fact]
    public void PublishBatch_MultiplePoints_AllApplied()
    {
        // Arrange
        var chart = CreateChart();
        _service.Register(chart);
        var points = Enumerable.Range(1, 5).Select(i =>
            new StreamDataPoint { SeriesName = "Sensor", X = i, Y = i * 10.0 });

        // Act
        var enqueued = _service.PublishBatch(chart.Id, points);
        _service.GetSnapshot(chart.Id); // drains buffer

        // Assert
        enqueued.Should().Be(5);
        chart.GetSeriesByName("Sensor")!.GetDataPointCount().Should().Be(5);
    }

    /// <summary>
    /// Verifies that the window size is enforced and oldest points are dropped.
    /// </summary>
    [Fact]
    public void WindowSize_Enforced_OldestPointsDropped()
    {
        // Arrange
        var chart   = CreateChart();
        var options = new StreamingChartOptions { WindowSize = 3 };
        _service.Register(chart, options);

        // Act – publish 5 points; only last 3 should survive
        for (int i = 1; i <= 5; i++)
            _service.Publish(chart.Id, new StreamDataPoint { SeriesName = "Sensor", X = i, Y = i });

        _service.GetSnapshot(chart.Id);

        // Assert
        var series = chart.GetSeriesByName("Sensor")!;
        series.GetDataPointCount().Should().Be(3);
        series.DataPoints[0].X.Should().Be(3);
    }

    /// <summary>
    /// Verifies that auto-creating a series when it does not exist creates the series.
    /// </summary>
    [Fact]
    public void AutoCreateSeries_WhenSeriesDoesNotExist_SeriesCreated()
    {
        // Arrange
        var chart = new Chart("auto-series");
        _service.Register(chart);

        // Act
        _service.Publish(chart.Id, new StreamDataPoint { SeriesName = "NewSeries", X = 1, Y = 99 });
        _service.GetSnapshot(chart.Id);

        // Assert
        chart.GetSeriesByName("NewSeries").Should().NotBeNull();
        chart.GetSeriesByName("NewSeries")!.DataPoints[0].Y.Should().Be(99);
    }

    /// <summary>
    /// Verifies that unregistering a chart and then publishing to it throws an InvalidOperationException.
    /// </summary>
    [Fact]
    public void Unregister_PublishAfterwards_ThrowsInvalidOperationException()
    {
        // Arrange
        var chart = CreateChart();
        _service.Register(chart);
        _service.Unregister(chart.Id);

        // Act
        Action act = () => _service.Publish(chart.Id, new StreamDataPoint { SeriesName = "S", X = 1, Y = 1 });

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    /// <summary>
    /// Verifies that flushing the buffer applies the buffered points.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task FlushAsync_AppliesBufferedPoints()
    {
        // Arrange
        var chart = CreateChart();
        _service.Register(chart);
        _service.Publish(chart.Id, new StreamDataPoint { SeriesName = "Sensor", X = 1, Y = 1 });
        _service.Publish(chart.Id, new StreamDataPoint { SeriesName = "Sensor", X = 2, Y = 2 });

        // Act
        var applied = await _service.FlushAsync(chart.Id);

        // Assert
        applied.Should().Be(2);
        chart.GetSeriesByName("Sensor")!.GetDataPointCount().Should().Be(2);
    }
}
