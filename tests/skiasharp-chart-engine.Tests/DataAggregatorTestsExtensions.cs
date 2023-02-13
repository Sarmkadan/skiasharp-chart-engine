using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

namespace SkiaSharpChartEngine.Tests.Utilities;

public static class DataAggregatorTestsExtensions
{
    private static DataAggregator GetAggregator(DataAggregatorTests fixture)
    {
        var field = typeof(DataAggregatorTests).GetField("_aggregator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field?.GetValue(fixture) as DataAggregator ?? throw new InvalidOperationException("Aggregator field not found");
    }

    /// <summary>
    /// Creates a test fixture with a pre-configured DataAggregator instance
    /// </summary>
    /// <param name="dataPoints">The data points to initialize with</param>
    /// <returns>A configured DataAggregatorTests fixture</returns>
    public static DataAggregatorTests WithDataPoints(this DataAggregatorTests fixture, IEnumerable<DataPoint> dataPoints)
    {
        // Use reflection to set the private _aggregator field
        var field = typeof(DataAggregatorTests).GetField("_aggregator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            var loggerMock = new Mock<ILogger<DataAggregator>>();
            var aggregator = new DataAggregator(loggerMock.Object);
            field.SetValue(fixture, aggregator);
        }
        return fixture;
    }

    /// <summary>
    /// Creates a test fixture with a pre-configured DataAggregator instance
    /// </summary>
    /// <param name="dataPoints">The data points to initialize with</param>
    /// <param name="loggerMock">Custom logger mock</param>
    /// <returns>A configured DataAggregatorTests fixture</returns>
    public static DataAggregatorTests WithDataPoints(this DataAggregatorTests fixture, IEnumerable<DataPoint> dataPoints, Mock<ILogger<DataAggregator>> loggerMock)
    {
        // Use reflection to set the private _aggregator field
        var field = typeof(DataAggregatorTests).GetField("_aggregator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            var aggregator = new DataAggregator(loggerMock.Object);
            field.SetValue(fixture, aggregator);
        }
        return fixture;
    }

    /// <summary>
    /// Asserts that the aggregator returns expected bucket count for given data points
    /// </summary>
    /// <param name="fixture">The test fixture</param>
    /// <param name="dataPoints">Input data points</param>
    /// <param name="bucketCount">Expected bucket count</param>
    /// <param name="aggregationType">Aggregation type</param>
    /// <param name="expectedCount">Expected result count</param>
    public static void ShouldAggregateToCount(this DataAggregatorTests fixture, IEnumerable<DataPoint> dataPoints, int bucketCount, AggregationType aggregationType, int expectedCount)
    {
        var aggregator = GetAggregator(fixture);
        var result = aggregator.AggregateByCount(dataPoints.ToList(), bucketCount, aggregationType);
        result.Should().HaveCount(expectedCount);
    }

    /// <summary>
    /// Asserts that the aggregator computes correct aggregation values
    /// </summary>
    /// <param name="fixture">The test fixture</param>
    /// <param name="dataPoints">Input data points</param>
    /// <param name="bucketCount">Bucket count</param>
    /// <param name="aggregationType">Aggregation type</param>
    /// <param name="expectedValues">Expected aggregated values in order</param>
    public static void ShouldAggregateToValues(this DataAggregatorTests fixture, IEnumerable<DataPoint> dataPoints, int bucketCount, AggregationType aggregationType, params double[] expectedValues)
    {
        var aggregator = GetAggregator(fixture);
        var result = aggregator.AggregateByCount(dataPoints.ToList(), bucketCount, aggregationType);
        result.Select(r => r.Value).Should().BeEquivalentTo(expectedValues);
    }

    /// <summary>
    /// Asserts that statistics calculation returns expected values
    /// </summary>
    /// <param name="fixture">The test fixture</param>
    /// <param name="dataPoints">Input data points</param>
    /// <param name="expectedSum">Expected sum value</param>
    /// <param name="expectedAverage">Expected average value</param>
    /// <param name="expectedMin">Expected minimum value</param>
    /// <param name="expectedMax">Expected maximum value</param>
    /// <param name="expectedMedian">Expected median value</param>
    public static void ShouldCalculateStatistics(this DataAggregatorTests fixture, IEnumerable<DataPoint> dataPoints,
        double expectedSum, double expectedAverage, double expectedMin, double expectedMax, double expectedMedian)
    {
        var aggregator = GetAggregator(fixture);
        var result = aggregator.CalculateStatistics(dataPoints.ToList());
        result.Should().NotBeNull();
        result!.Sum.Should().Be(expectedSum);
        result.Average.Should().Be(expectedAverage);
        result.Min.Should().Be(expectedMin);
        result.Max.Should().Be(expectedMax);
        result.Median.Should().Be(expectedMedian);
    }

    /// <summary>
    /// Creates a list of data points with sequential values
    /// </summary>
    /// <param name="count">Number of data points to create</param>
    /// <param name="baseValue">Base value for data points</param>
    /// <returns>List of DataPoint objects</returns>
    public static List<DataPoint> CreateSequentialDataPoints(this int count, double baseValue = 10.0)
    {
        return Enumerable.Range(1, count)
            .Select(i => new DataPoint(i, baseValue * i))
            .ToList();
    }

    /// <summary>
    /// Creates a list of data points with random values
    /// </summary>
    /// <param name="count">Number of data points to create</param>
    /// <param name="random">Random number generator</param>
    /// <returns>List of DataPoint objects with random values</returns>
    public static List<DataPoint> CreateRandomDataPoints(this int count, Random random)
    {
        return Enumerable.Range(1, count)
            .Select(i => new DataPoint(i, random.NextDouble() * 1000))
            .ToList();
    }

    /// <summary>
    /// Asserts that the aggregator groups data points by label correctly
    /// </summary>
    /// <param name="fixture">The test fixture</param>
    /// <param name="dataPoints">Input data points with labels</param>
    /// <param name="expectedGroups">Expected group labels</param>
    public static void ShouldGroupByLabel(this DataAggregatorTests fixture, IEnumerable<DataPoint> dataPoints, params string[] expectedGroups)
    {
        var aggregator = GetAggregator(fixture);
        var result = aggregator.AggregateByInterval(dataPoints.ToList(), AggregationType.Average);
        result.Keys.Should().BeEquivalentTo(expectedGroups);
    }

    /// <summary>
    /// Asserts that the aggregator handles null data points correctly
    /// </summary>
    /// <param name="action">Action that should handle null data points</param>
    public static void ShouldHandleNullDataPoints(this Action action)
    {
        action.Should().NotThrow();
    }
}