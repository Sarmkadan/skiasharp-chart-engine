// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Extension methods for DataAggregator to provide fluent API and additional
// convenience methods for common aggregation operations.
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Extension methods for DataAggregator providing fluent API and additional
/// convenience methods for working with aggregated data.
/// </summary>
public static class DataAggregatorExtensions
{
    /// <summary>
    /// Filters data points by minimum value threshold before aggregation.
    /// </summary>
    /// <param name="aggregator">The DataAggregator instance</param>
    /// <param name="dataPoints">Data points to filter and aggregate</param>
    /// <param name="minValue">Minimum value threshold</param>
    /// <returns>Filtered data points</returns>
    public static List<DataPoint> FilterByMinValue(this DataAggregator aggregator, List<DataPoint> dataPoints, double minValue)
    {
        if (dataPoints == null || dataPoints.Count == 0)
            return new List<DataPoint>();

        return dataPoints.Where(dp => dp.Value >= minValue).ToList();
    }

    /// <summary>
    /// Filters data points by maximum value threshold before aggregation.
    /// </summary>
    /// <param name="aggregator">The DataAggregator instance</param>
    /// <param name="dataPoints">Data points to filter and aggregate</param>
    /// <param name="maxValue">Maximum value threshold</param>
    /// <returns>Filtered data points</returns>
    public static List<DataPoint> FilterByMaxValue(this DataAggregator aggregator, List<DataPoint> dataPoints, double maxValue)
    {
        if (dataPoints == null || dataPoints.Count == 0)
            return new List<DataPoint>();

        return dataPoints.Where(dp => dp.Value <= maxValue).ToList();
    }

    /// <summary>
    /// Filters data points by value range before aggregation.
    /// </summary>
    /// <param name="aggregator">The DataAggregator instance</param>
    /// <param name="dataPoints">Data points to filter and aggregate</param>
    /// <param name="minValue">Minimum value threshold</param>
    /// <param name="maxValue">Maximum value threshold</param>
    /// <returns>Filtered data points</returns>
    public static List<DataPoint> FilterByRange(this DataAggregator aggregator, List<DataPoint> dataPoints, double minValue, double maxValue)
    {
        if (dataPoints == null || dataPoints.Count == 0)
            return new List<DataPoint>();

        return dataPoints.Where(dp => dp.Value >= minValue && dp.Value <= maxValue).ToList();
    }

    /// <summary>
    /// Aggregates data points and returns statistics in a single operation.
    /// This is a convenience method that combines filtering and aggregation.
    /// </summary>
    /// <param name="aggregator">The DataAggregator instance</param>
    /// <param name="dataPoints">Data points to aggregate</param>
    /// <param name="filterMin">Optional minimum value filter</param>
    /// <param name="filterMax">Optional maximum value filter</param>
    /// <returns>DataStatistics object with calculated statistics</returns>
    public static DataStatistics AggregateWithStatistics(this DataAggregator aggregator,
        List<DataPoint> dataPoints,
        double? filterMin = null,
        double? filterMax = null)
    {
        var filteredPoints = dataPoints ?? new List<DataPoint>();

        if (filterMin.HasValue)
            filteredPoints = filteredPoints.Where(dp => dp.Value >= filterMin.Value).ToList();

        if (filterMax.HasValue)
            filteredPoints = filteredPoints.Where(dp => dp.Value <= filterMax.Value).ToList();

        return aggregator.CalculateStatistics(filteredPoints);
    }

    /// <summary>
    /// Creates a normalized version of the data points where values are scaled
    /// to a 0-1 range based on the min and max values in the dataset.
    /// </summary>
    /// <param name="aggregator">The DataAggregator instance</param>
    /// <param name="dataPoints">Data points to normalize</param>
    /// <returns>New list of data points with normalized values</returns>
    public static List<DataPoint> NormalizeValues(this DataAggregator aggregator, List<DataPoint> dataPoints)
    {
        if (dataPoints == null || dataPoints.Count == 0)
            return new List<DataPoint>();

        var minValue = dataPoints.Min(dp => dp.Value);
        var maxValue = dataPoints.Max(dp => dp.Value);
        var range = maxValue - minValue;

        if (range == 0)
            return dataPoints.Select(dp => new DataPoint { Label = dp.Label, Value = 0.5 }).ToList();

        return dataPoints.Select(dp => new DataPoint
        {
            Label = dp.Label,
            Value = (dp.Value - minValue) / range
        }).ToList();
    }

    /// <summary>
    /// Gets the top N data points with the highest values.
    /// </summary>
    /// <param name="aggregator">The DataAggregator instance</param>
    /// <param name="dataPoints">Data points to process</param>
    /// <param name="count">Number of top points to return</param>
    /// <returns>List of top N data points sorted by value descending</returns>
    public static List<DataPoint> GetTopN(this DataAggregator aggregator, List<DataPoint> dataPoints, int count)
    {
        if (dataPoints == null || dataPoints.Count == 0 || count <= 0)
            return new List<DataPoint>();

        return dataPoints.OrderByDescending(dp => dp.Value).Take(count).ToList();
    }

    /// <summary>
    /// Gets the bottom N data points with the lowest values.
    /// </summary>
    /// <param name="aggregator">The DataAggregator instance</param>
    /// <param name="dataPoints">Data points to process</param>
    /// <param name="count">Number of bottom points to return</param>
    /// <returns>List of bottom N data points sorted by value ascending</returns>
    public static List<DataPoint> GetBottomN(this DataAggregator aggregator, List<DataPoint> dataPoints, int count)
    {
        if (dataPoints == null || dataPoints.Count == 0 || count <= 0)
            return new List<DataPoint>();

        return dataPoints.OrderBy(dp => dp.Value).Take(count).ToList();
    }
}