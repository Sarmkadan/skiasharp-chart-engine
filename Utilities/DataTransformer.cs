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
/// Transforms and normalizes chart data for different rendering scenarios.
/// Supports pivoting, filtering, and value transformations.
/// </summary>
public class DataTransformer
{
    private readonly ILogger<DataTransformer> _logger;

    public DataTransformer(ILogger<DataTransformer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Normalize values to 0-1 range
    public List<DataPoint> NormalizeValues(List<DataPoint> dataPoints)
    {
        try
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return dataPoints;

            var min = dataPoints.Min(dp => dp.Value);
            var max = dataPoints.Max(dp => dp.Value);
            var range = max - min;

            if (range == 0)
            {
                return dataPoints.Select(dp => new DataPoint { Label = dp.Label, Value = 0.5 }).ToList();
            }

            var normalized = dataPoints.Select(dp => new DataPoint
            {
                Label = dp.Label,
                Value = (dp.Value - min) / range
            }).ToList();

            _logger.LogDebug("Data normalized: range {Min}-{Max} -> 0-1", min, max);
            return normalized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error normalizing values");
            return dataPoints;
        }
    }

    // Apply logarithmic transformation
    public List<DataPoint> ApplyLogTransformation(List<DataPoint> dataPoints, double baseValue = 10)
    {
        try
        {
            var transformed = dataPoints.Select(dp => new DataPoint
            {
                Label = dp.Label,
                Value = Math.Log(Math.Max(1, dp.Value), baseValue)
            }).ToList();

            _logger.LogDebug("Log transformation applied with base {Base}", baseValue);
            return transformed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying log transformation");
            return dataPoints;
        }
    }

    // Filter outliers using standard deviation
    public List<DataPoint> FilterOutliers(List<DataPoint> dataPoints, double stdDevThreshold = 3)
    {
        try
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return dataPoints;

            var values = dataPoints.Select(dp => dp.Value).ToList();
            var mean = values.Average();
            var stdDev = Math.Sqrt(values.Average(v => Math.Pow(v - mean, 2)));

            var filtered = dataPoints.Where(dp =>
                Math.Abs(dp.Value - mean) <= stdDevThreshold * stdDev
            ).ToList();

            _logger.LogInformation("Outliers removed: {Original} -> {Filtered}", dataPoints.Count, filtered.Count);
            return filtered;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering outliers");
            return dataPoints;
        }
    }

    // Smooth data using moving average
    public List<DataPoint> ApplyMovingAverage(List<DataPoint> dataPoints, int windowSize = 3)
    {
        try
        {
            if (dataPoints == null || dataPoints.Count < windowSize)
                return dataPoints;

            var smoothed = new List<DataPoint>();

            for (int i = 0; i < dataPoints.Count; i++)
            {
                var windowStart = Math.Max(0, i - windowSize / 2);
                var windowEnd = Math.Min(dataPoints.Count - 1, i + windowSize / 2);
                var window = dataPoints.Skip(windowStart).Take(windowEnd - windowStart + 1);

                var average = window.Average(dp => dp.Value);
                smoothed.Add(new DataPoint
                {
                    Label = dataPoints[i].Label,
                    Value = average
                });
            }

            _logger.LogDebug("Moving average applied with window size {WindowSize}", windowSize);
            return smoothed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying moving average");
            return dataPoints;
        }
    }

    // Scale values by multiplier
    public List<DataPoint> ScaleValues(List<DataPoint> dataPoints, double multiplier)
    {
        try
        {
            var scaled = dataPoints.Select(dp => new DataPoint
            {
                Label = dp.Label,
                Value = dp.Value * multiplier
            }).ToList();

            _logger.LogDebug("Values scaled by {Multiplier}", multiplier);
            return scaled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scaling values");
            return dataPoints;
        }
    }

    // Offset values by constant
    public List<DataPoint> OffsetValues(List<DataPoint> dataPoints, double offset)
    {
        try
        {
            var offsetted = dataPoints.Select(dp => new DataPoint
            {
                Label = dp.Label,
                Value = dp.Value + offset
            }).ToList();

            _logger.LogDebug("Values offset by {Offset}", offset);
            return offsetted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error offsetting values");
            return dataPoints;
        }
    }

    // Rank data points
    public List<RankedDataPoint> RankDataPoints(List<DataPoint> dataPoints, bool ascending = false)
    {
        try
        {
            var ordered = ascending
                ? dataPoints.OrderBy(dp => dp.Value).ToList()
                : dataPoints.OrderByDescending(dp => dp.Value).ToList();

            var ranked = ordered.Select((dp, index) => new RankedDataPoint
            {
                Label = dp.Label,
                Value = dp.Value,
                Rank = index + 1
            }).ToList();

            _logger.LogDebug("Data ranked: {Count} points", ranked.Count);
            return ranked;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ranking data points");
            return new List<RankedDataPoint>();
        }
    }
}

/// <summary>
/// Data point with rank information.
/// </summary>
public class RankedDataPoint
{
    public string Label { get; set; }
    public double Value { get; set; }
    public int Rank { get; set; }
}
