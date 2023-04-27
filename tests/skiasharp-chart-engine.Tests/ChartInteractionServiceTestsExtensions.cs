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
/// <remarks>
/// This class provides fluent extension methods for testing chart interaction scenarios.
/// All methods include proper null checking and follow modern C# practices.
/// </remarks>
public static class ChartInteractionServiceTestsExtensions
{
    /// <summary>
    /// Creates a new chart with the specified number of series and data points.
    /// </summary>
    /// <param name="id">The ID of the chart.</param>
    /// <param name="seriesCount">Number of series to create.</param>
    /// <param name="dataPointsPerSeries">Number of data points per series.</param>
    /// <returns>A new chart instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="id"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="seriesCount"/> is negative.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dataPointsPerSeries"/> is negative.</exception>
    public static Chart CreateChart(this ChartInteractionServiceTests _, string id = "chart-1", int seriesCount = 1, int dataPointsPerSeries = 2)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentOutOfRangeException.ThrowIfLessThan(seriesCount, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(dataPointsPerSeries, 0);

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
    /// <exception cref="ArgumentNullException"><paramref name="chart"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="seriesIndex"/> is out of range.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="dataPointIndex"/> is out of range.</exception>
    public static TooltipHitResult MakeHit(this ChartInteractionServiceTests _, Chart chart, int seriesIndex = 0, int dataPointIndex = 0)
    {
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentOutOfRangeException.ThrowIfNegative(seriesIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(dataPointIndex);

        if (seriesIndex >= chart.Series.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(seriesIndex), $"Series index {seriesIndex} is out of range for chart with {chart.Series.Count} series.");
        }

        var series = chart.Series[seriesIndex];
        if (dataPointIndex >= series.DataPoints.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(dataPointIndex), $"Data point index {dataPointIndex} is out of range for series with {series.DataPoints.Count} data points.");
        }

        var dp = series.DataPoints[dataPointIndex];
        return new TooltipHitResult
        {
            IsHit = true,
            DataPoint = dp,
            Series = series,
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
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="expectedDataPoint"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="expectedSeries"/> is null.</exception>
    public static void ShouldBeValidHit(this ChartInteractionServiceTests _, ChartInteractionEventArgs result, DataPoint expectedDataPoint, ChartSeries expectedSeries)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(expectedDataPoint);
        ArgumentNullException.ThrowIfNull(expectedSeries);

        result.Should().NotBeNull();
        result.HitDataPoint.Should().BeSameAs(expectedDataPoint);
        result.HitSeries.Should().BeSameAs(expectedSeries);
        result.Region.Should().Be(ChartRegion.PlotArea);
    }

    /// <summary>
    /// Verifies that the specified chart interaction result is a miss.
    /// </summary>
    /// <param name="result">The interaction result to verify.</param>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is null.</exception>
    public static void ShouldBeMiss(this ChartInteractionServiceTests _, ChartInteractionEventArgs result)
    {
        ArgumentNullException.ThrowIfNull(result);

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
    /// <exception cref="ArgumentNullException"><paramref name="mock"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="chart"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="hitResult"/> is null.</exception>
    public static void SetupHitTest(this Mock<IInteractivityService> mock, Chart chart, TooltipHitResult hitResult)
    {
        ArgumentNullException.ThrowIfNull(mock);
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(hitResult);

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
    /// <exception cref="ArgumentNullException"><paramref name="mock"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="chart"/> is null.</exception>
    public static void SetupHitMiss(this Mock<IInteractivityService> mock, Chart chart)
    {
        ArgumentNullException.ThrowIfNull(mock);
        ArgumentNullException.ThrowIfNull(chart);

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