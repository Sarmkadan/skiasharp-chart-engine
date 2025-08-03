// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Aggregates data points using various aggregation functions.
/// Supports sum, average, min, max, and custom aggregations.
/// </summary>
public class DataAggregator
{
    private readonly ILogger<DataAggregator> _logger;

    public DataAggregator(ILogger<DataAggregator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Aggregate data points into buckets
    public List<DataPoint> AggregateByCount(List<DataPoint> dataPoints, int bucketCount, AggregationType aggregationType)
    {
        try
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return new List<DataPoint>();

            if (bucketCount <= 0)
                throw new ArgumentException("Bucket count must be positive", nameof(bucketCount));

            _logger.LogInformation("Aggregating {Count} points into {BucketCount} buckets using {AggregationType}",
                dataPoints.Count, bucketCount, aggregationType);

            var bucketSize = (int)Math.Ceiling((double)dataPoints.Count / bucketCount);
            var aggregated = new List<DataPoint>();

            for (int i = 0; i < dataPoints.Count; i += bucketSize)
            {
                var bucket = dataPoints.Skip(i).Take(bucketSize).ToList();
                if (bucket.Count == 0) continue;

                var aggregatedPoint = _aggregateBucket(bucket, aggregationType, i / bucketSize);
                aggregated.Add(aggregatedPoint);
            }

            _logger.LogDebug("Aggregation complete: {OriginalCount} -> {AggregatedCount}", dataPoints.Count, aggregated.Count);
            return aggregated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating data");
            return dataPoints;
        }
    }

    // Aggregate data points by time interval (assumes label contains time info)
    public Dictionary<string, List<DataPoint>> AggregateByInterval(List<DataPoint> dataPoints, AggregationType aggregationType)
    {
        try
        {
            var grouped = new Dictionary<string, List<DataPoint>>();

            foreach (var point in dataPoints)
            {
                var key = point.Label ?? "unknown";
                if (!grouped.ContainsKey(key))
                    grouped[key] = new List<DataPoint>();

                grouped[key].Add(point);
            }

            _logger.LogDebug("Data grouped by interval: {GroupCount} groups", grouped.Count);
            return grouped;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating by interval");
            return new Dictionary<string, List<DataPoint>>();
        }
    }

    // Calculate statistics for data points
    public DataStatistics CalculateStatistics(List<DataPoint> dataPoints)
    {
        try
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return null;

            var values = dataPoints.Select(dp => dp.Value).OrderBy(v => v).ToList();
            var sum = values.Sum();
            var avg = values.Average();
            var variance = values.Average(v => Math.Pow(v - avg, 2));

            return new DataStatistics
            {
                Count = values.Count,
                Sum = sum,
                Average = avg,
                Min = values.First(),
                Max = values.Last(),
                Median = values[values.Count / 2],
                StandardDeviation = Math.Sqrt(variance),
                Range = values.Last() - values.First(),
                CalculatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating statistics");
            return null;
        }
    }

    private DataPoint _aggregateBucket(List<DataPoint> bucket, AggregationType aggregationType, int bucketIndex)
    {
        var values = bucket.Select(dp => dp.Value).ToList();
        double aggregatedValue = aggregationType switch
        {
            AggregationType.Sum => values.Sum(),
            AggregationType.Average => values.Average(),
            AggregationType.Min => values.Min(),
            AggregationType.Max => values.Max(),
            AggregationType.Median => _calculateMedian(values),
            _ => values.Average()
        };

        return new DataPoint
        {
            Label = bucket.First().Label ?? $"Bucket {bucketIndex}",
            Value = aggregatedValue
        };
    }

    private double _calculateMedian(List<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        if (sorted.Count % 2 == 0)
            return (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2;

        return sorted[sorted.Count / 2];
    }
}

public enum AggregationType
{
    Sum,
    Average,
    Min,
    Max,
    Median
}

public class DataStatistics
{
    public int Count { get; set; }
    public double Sum { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Median { get; set; }
    public double StandardDeviation { get; set; }
    public double Range { get; set; }
    public DateTime CalculatedAt { get; set; }
}
