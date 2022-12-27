// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Events;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Services;

public class ChartInteractionServiceTests
{
    private readonly Mock<IInteractivityService> _interactivityMock;
    private readonly Mock<ILogger<ChartInteractionService>> _loggerMock;
    private readonly ChartInteractionService _service;

    public ChartInteractionServiceTests()
    {
        _interactivityMock = new Mock<IInteractivityService>();
        _loggerMock        = new Mock<ILogger<ChartInteractionService>>();
        _service           = new ChartInteractionService(_interactivityMock.Object, _loggerMock.Object);
    }

    private static Chart CreateChart(string id = "chart-1")
    {
        var chart  = new Chart(id);
        var series = new ChartSeries("Series A");
        series.AddDataPoint(1.0, 10.0);
        series.AddDataPoint(2.0, 20.0);
        chart.AddSeries(series);
        return chart;
    }

    private static TooltipHitResult MakeHit(Chart chart)
    {
        var dp = chart.Series[0].DataPoints[0];
        return new TooltipHitResult
        {
            IsHit       = true,
            DataPoint   = dp,
            Series      = chart.Series[0],
            SeriesIndex = 0,
            Region      = ChartRegion.PlotArea,
            TooltipText = "x=1, y=10"
        };
    }

    [Fact]
    public void ProcessInteraction_WithNullChart_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _service.ProcessInteraction(null!, ChartInteractionType.Click, 0, 0, 800, 600);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("chart");
    }

    [Fact]
    public void ProcessInteraction_ClickOnDataPoint_RaisesClickedEvent()
    {
        // Arrange
        var chart = CreateChart();
        var hit   = MakeHit(chart);
        _interactivityMock
            .Setup(s => s.HitTest(chart, It.IsAny<float>(), It.IsAny<float>(),
                It.IsAny<float>(), It.IsAny<float>(), null, null))
            .Returns(hit);

        ChartInteractionEventArgs? received = null;
        _service.Clicked += (_, e) => received = e;

        // Act
        var result = _service.ProcessInteraction(chart, ChartInteractionType.Click, 100, 200, 800, 600);

        // Assert
        result.Should().NotBeNull();
        result.InteractionType.Should().Be(ChartInteractionType.Click);
        result.HitDataPoint.Should().Be(hit.DataPoint);
        result.HitSeries.Should().Be(hit.Series);
        result.TooltipText.Should().Be("x=1, y=10");
        received.Should().NotBeNull();
        received!.InteractionType.Should().Be(ChartInteractionType.Click);
    }

    [Fact]
    public void ProcessInteraction_HoverMiss_ReturnsNoHit()
    {
        // Arrange
        var chart = CreateChart();
        _interactivityMock
            .Setup(s => s.HitTest(chart, It.IsAny<float>(), It.IsAny<float>(),
                It.IsAny<float>(), It.IsAny<float>(), null, null))
            .Returns(TooltipHitResult.Miss);

        // Act
        var result = _service.ProcessInteraction(chart, ChartInteractionType.Hover, 0, 0, 800, 600);

        // Assert
        result.HitDataPoint.Should().BeNull();
        result.HitSeries.Should().BeNull();
        result.SeriesIndex.Should().Be(-1);
    }

    [Fact]
    public void ToggleSelection_HitDataPoint_SelectsAndRaisesEvent()
    {
        // Arrange
        var chart = CreateChart();
        var hit   = MakeHit(chart);
        _interactivityMock
            .Setup(s => s.HitTest(chart, It.IsAny<float>(), It.IsAny<float>(),
                It.IsAny<float>(), It.IsAny<float>(), null, null))
            .Returns(hit);

        ChartSelectionChangedEventArgs? selArgs = null;
        _service.SelectionChanged += (_, e) => selArgs = e;

        // Act
        var toggled = _service.ToggleSelection(chart, 100, 200, 800, 600);

        // Assert
        toggled.Should().BeTrue();
        selArgs.Should().NotBeNull();
        selArgs!.TotalSelected.Should().Be(1);
    }

    [Fact]
    public void ToggleSelection_SamePointTwice_DeselectionRemovesPoint()
    {
        // Arrange
        var chart = CreateChart();
        var hit   = MakeHit(chart);
        _interactivityMock
            .Setup(s => s.HitTest(chart, It.IsAny<float>(), It.IsAny<float>(),
                It.IsAny<float>(), It.IsAny<float>(), null, null))
            .Returns(hit);

        // Act – select then deselect
        _service.ToggleSelection(chart, 100, 200, 800, 600);
        _service.ToggleSelection(chart, 100, 200, 800, 600);

        // Assert
        var selection = _service.GetSelection(chart);
        var total = selection.Values.Sum(pts => pts.Count);
        total.Should().Be(0);
    }

    [Fact]
    public void ClearSelection_AfterSelect_EmptiesSelection()
    {
        // Arrange
        var chart = CreateChart();
        var hit   = MakeHit(chart);
        _interactivityMock
            .Setup(s => s.HitTest(chart, It.IsAny<float>(), It.IsAny<float>(),
                It.IsAny<float>(), It.IsAny<float>(), null, null))
            .Returns(hit);

        _service.ToggleSelection(chart, 100, 200, 800, 600);

        ChartSelectionChangedEventArgs? selArgs = null;
        _service.SelectionChanged += (_, e) => selArgs = e;

        // Act
        _service.ClearSelection(chart);

        // Assert
        selArgs.Should().NotBeNull();
        selArgs!.IsEmpty.Should().BeTrue();
        _service.GetSelection(chart).Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessInteractionAsync_WithCancellation_ThrowsOperationCancelled()
    {
        // Arrange
        var chart = CreateChart();
        var cts   = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () =>
            await _service.ProcessInteractionAsync(
                chart, ChartInteractionType.Hover, 0, 0, 800, 600,
                cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
