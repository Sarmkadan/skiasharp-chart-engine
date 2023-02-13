// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Events;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Xunit;

/// <summary>
/// Extension methods for <see cref="ChartInteractionServiceTests"/> to simplify common test scenarios.
/// </summary>
public static class ChartInteractionServiceTestsExtensions
{
    /// <summary>
    /// Creates a new chart with the specified number of series and data points.
    /// </summary>
    /// <param name="id">The ID of the chart.</param>
    /// <param name="seriesCount">Number of series to create.</param>
    /// <param name="dataPointsPerSeries">Number of data points per series.</param>
    /// <returns>A new chart instance.</returns>
    public static Chart CreateChart(this ChartInteractionServiceTests _, string id = "chart-1", int seriesCount = 1, int dataPointsPerSeries = 2)
    {
        var chart = new Chart(id);

        for (int s = 0; s < seriesCount; s++)
        {
            var series = new ChartSeries($"Series {s + 1}");
            for (int p = 0; p < dataPointsPerSeries; p++)
            {
                series.AddDataPoint(p + 1.0, (p + 1) * 10.0);
            }
            chart.AddSeries(series);
        }

        return chart;
    }

    /// <summary>
    /// Creates a new tooltip hit result with the specified data point.
    /// </summary>
    /// <param name="chart">The chart instance.</param>
    /// <param name="seriesIndex">Index of the series to hit.</param>
    /// <param name="dataPointIndex">Index of the data point to hit.</param>
    /// <returns>A new tooltip hit result instance.</returns>
    public static TooltipHitResult MakeHit(this ChartInteractionServiceTests _, Chart chart, int seriesIndex = 0, int dataPointIndex = 0)
    {
        var dp = chart.Series[seriesIndex].DataPoints[dataPointIndex];
        return new TooltipHitResult
        {
            IsHit = true,
            DataPoint = dp,
            Series = chart.Series[seriesIndex],
            SeriesIndex = seriesIndex,
            Region = ChartRegion.PlotArea,
            TooltipText = $"x={dp.X}, y={dp.Y}"
        };
    }

    /// <summary>
    /// Verifies that the specified chart interaction result is a valid hit.
    /// </summary>
    /// <param name="result">The interaction result to verify.</param>
    /// <param name="expectedDataPoint">The expected data point.</param>
    /// <param name="expectedSeries">The expected series.</param>
    public static void ShouldBeValidHit(this ChartInteractionServiceTests _, ChartInteractionEventArgs result, DataPoint expectedDataPoint, ChartSeries expectedSeries)
    {
        result.Should().NotBeNull();
        result.HitDataPoint.Should().BeSameAs(expectedDataPoint);
        result.HitSeries.Should().BeSameAs(expectedSeries);
        result.Region.Should().Be(ChartRegion.PlotArea);
    }

    /// <summary>
    /// Verifies that the specified chart interaction result is a miss.
    /// </summary>
    /// <param name="result">The interaction result to verify.</param>
    public static void ShouldBeMiss(this ChartInteractionServiceTests _, ChartInteractionEventArgs result)
    {
        result.Should().NotBeNull();
        result.HitDataPoint.Should().BeNull();
        result.HitSeries.Should().BeNull();
        result.Region.Should().Be(ChartRegion.Outside);
        result.SeriesIndex.Should().Be(-1);
    }

    /// <summary>
    /// Sets up the interactivity service mock to return a hit result.
    /// </summary>
    /// <param name="mock">The interactivity service mock.</param>
    /// <param name="chart">The chart instance.</param>
    /// <param name="hitResult">The hit result to return.</param>
    public static void SetupHitTest(this Mock<IInteractivityService> mock, Chart chart, TooltipHitResult hitResult)
    {
        mock.Setup(s => s.HitTest(
            chart,
            It.IsAny<float>(),
            It.IsAny<float>(),
            It.IsAny<float>(),
            It.IsAny<float>(),
            null,
            null))
            .Returns(hitResult);
    }

    /// <summary>
    /// Sets up the interactivity service mock to return a miss result.
    /// </summary>
    /// <param name="mock">The interactivity service mock.</param>
    /// <param name="chart">The chart instance.</param>
    public static void SetupHitMiss(this Mock<IInteractivityService> mock, Chart chart)
    {
        mock.Setup(s => s.HitTest(
            chart,
            It.IsAny<float>(),
            It.IsAny<float>(),
            It.IsAny<float>(),
            It.IsAny<float>(),
            null,
            null))
            .Returns(TooltipHitResult.Miss);
    }
}
