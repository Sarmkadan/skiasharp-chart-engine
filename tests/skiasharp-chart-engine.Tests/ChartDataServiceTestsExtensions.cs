// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Xunit;

namespace SkiaSharpChartEngine.Tests.Services;

/// <summary>
/// Extension methods for creating and asserting test charts in unit tests.
/// Provides fluent API for test data setup and validation.
/// </summary>

public static class ChartDataServiceTestsExtensions
{
    /// <summary>
    /// Creates a test chart with the specified name and adds a single series with the given data points.
    /// </summary>
    /// <param name="chartName">Name of the chart.</param>
    /// <param name="dataPoints">Collection of (x, y) tuples to add to the series.</param>
    /// <returns>A populated Chart object ready for testing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="chartName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="dataPoints"/> is <see langword="null"/>.</exception>
    public static Chart CreateTestChart(this ChartDataServiceTests _, string chartName, IEnumerable<(double x, double y)> dataPoints)
    {
        ArgumentNullException.ThrowIfNull(chartName);
        ArgumentNullException.ThrowIfNull(dataPoints);

        var chart = new Chart(chartName);
        var series = new ChartSeries("Series1");

        foreach (var (x, y) in dataPoints)
        {
            series.AddDataPoint(x, y);
        }

        chart.AddSeries(series);
        return chart;
    }

    /// <summary>
    /// Creates a test chart with a single series containing evenly spaced data points.
    /// </summary>
    /// <param name="chartName">Name of the chart.</param>
    /// <param name="pointCount">Number of data points to generate.</param>
    /// <param name="xStart">Starting X value.</param>
    /// <param name="xStep">Increment between X values.</param>
    /// <param name="yStart">Starting Y value.</param>
    /// <param name="yStep">Increment between Y values.</param>
    /// <returns>A populated Chart object ready for testing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="chartName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pointCount"/> is less than 0.</exception>
    public static Chart CreateTestChartWithSeries(this ChartDataServiceTests _, string chartName, int pointCount, double xStart = 0.0, double xStep = 1.0, double yStart = 0.0, double yStep = 10.0)
    {
        ArgumentNullException.ThrowIfNull(chartName);
        ArgumentOutOfRangeException.ThrowIfNegative(pointCount);

        var chart = new Chart(chartName);
        var series = new ChartSeries("Series1");

        for (int i = 0; i < pointCount; i++)
        {
            series.AddDataPoint(xStart + i * xStep, yStart + i * yStep);
        }

        chart.AddSeries(series);
        return chart;
    }

    /// <summary>
    /// Asserts that two charts are equivalent by comparing their series and data points.
    /// </summary>
    /// <param name="expected">Expected chart.</param>
    /// <param name="actual">Actual chart.</param>
    /// <param name="because">Optional reason for the assertion.</param>
    /// <exception cref="ArgumentNullException"><paramref name="actual"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="expected"/> is <see langword="null"/>.</exception>
    public static void ShouldBeEquivalentTo(this Chart actual, Chart expected, string because = "")
    {
        ArgumentNullException.ThrowIfNull(actual, nameof(actual));
        ArgumentNullException.ThrowIfNull(expected, nameof(expected));

        actual.Should().NotBeNull(because);
        expected.Should().NotBeNull(because);

        actual.Series.Should().HaveCount(expected.Series.Count, because);

        for (int i = 0; i < actual.Series.Count; i++)
        {
            var actualSeries = actual.Series[i];
            var expectedSeries = expected.Series[i];

            actualSeries.Name.Should().Be(expectedSeries.Name, because);
            actualSeries.LineWidth.Should().Be(expectedSeries.LineWidth, because);
            actualSeries.DataPoints.Should().HaveCount(expectedSeries.DataPoints.Count, because);

            for (int j = 0; j < actualSeries.DataPoints.Count; j++)
            {
                var actualPoint = actualSeries.DataPoints[j];
                var expectedPoint = expectedSeries.DataPoints[j];

                actualPoint.X.Should().BeApproximately(expectedPoint.X, 0.0001, because);
                actualPoint.Y.Should().BeApproximately(expectedPoint.Y, 0.0001, because);
            }
        }
    }

    /// <summary>
    /// Creates a chart with multiple series for testing cross-series operations.
    /// </summary>
    /// <param name="chartName">Name of the chart.</param>
    /// <param name="seriesConfig">Collection of (seriesName, dataPoints) tuples.</param>
    /// <returns>A populated Chart object with multiple series.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="chartName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="seriesConfig"/> is <see langword="null"/>.</exception>
    public static Chart CreateMultiSeriesChart(this ChartDataServiceTests _, string chartName, IEnumerable<(string seriesName, IEnumerable<(double x, double y)> dataPoints)> seriesConfig)
    {
        ArgumentNullException.ThrowIfNull(chartName);
        ArgumentNullException.ThrowIfNull(seriesConfig);

        var chart = new Chart(chartName);

        foreach (var (seriesName, dataPoints) in seriesConfig)
        {
            ArgumentNullException.ThrowIfNull(seriesName);
            ArgumentNullException.ThrowIfNull(dataPoints);

            var series = new ChartSeries(seriesName);
            foreach (var (x, y) in dataPoints)
            {
                series.AddDataPoint(x, y);
            }
            chart.AddSeries(series);
        }

        return chart;
    }
}