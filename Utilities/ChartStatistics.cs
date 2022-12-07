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
/// Calculates detailed statistical metrics for charts and data series.
/// Provides trend analysis, outlier detection, and correlation metrics.
/// </summary>
public class ChartStatistics
{
    private readonly ILogger<ChartStatistics> _logger;

    public ChartStatistics(ILogger<ChartStatistics> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Calculate series statistics
    public SeriesStatistics CalculateSeriesStatistics(ChartSeries series)
    {
        try
        {
            if (series == null || series.DataPoints == null || series.DataPoints.Count == 0)
                return null;

            var values = series.DataPoints.Select(dp => dp.Value).ToList();
            var sorted = values.OrderBy(v => v).ToList();

            var sum = values.Sum();
            var mean = values.Average();
            var variance = values.Average(v => Math.Pow(v - mean, 2));
            var stdDev = Math.Sqrt(variance);

            // Calculate quartiles
            var q1Index = (int)Math.Ceiling(sorted.Count * 0.25) - 1;
            var q3Index = (int)Math.Ceiling(sorted.Count * 0.75) - 1;

            return new SeriesStatistics
            {
                SeriesName = series.Name,
                Count = values.Count,
                Sum = sum,
                Mean = mean,
                Median = sorted[sorted.Count / 2],
                Mode = _calculateMode(values),
                Min = sorted.First(),
                Max = sorted.Last(),
                Range = sorted.Last() - sorted.First(),
                Variance = variance,
                StandardDeviation = stdDev,
                CoefficientOfVariation = stdDev / Math.Abs(mean),
                Q1 = sorted[Math.Max(0, q1Index)],
                Q3 = sorted[Math.Min(sorted.Count - 1, q3Index)],
                IQR = sorted[Math.Min(sorted.Count - 1, q3Index)] - sorted[Math.Max(0, q1Index)],
                Skewness = _calculateSkewness(values, mean, stdDev),
                Kurtosis = _calculateKurtosis(values, mean, stdDev)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating series statistics");
            return null;
        }
    }

    // Detect outliers using IQR method
    public List<OutlierInfo> DetectOutliers(ChartSeries series, double iqrMultiplier = 1.5)
    {
        try
        {
            var stats = CalculateSeriesStatistics(series);
            if (stats == null)
                return new List<OutlierInfo>();

            var lowerBound = stats.Q1 - (iqrMultiplier * stats.IQR);
            var upperBound = stats.Q3 + (iqrMultiplier * stats.IQR);

            var outliers = new List<OutlierInfo>();

            for (int i = 0; i < series.DataPoints.Count; i++)
            {
                var value = series.DataPoints[i].Value;
                if (value < lowerBound || value > upperBound)
                {
                    outliers.Add(new OutlierInfo
                    {
                        Index = i,
                        Value = value,
                        Label = series.DataPoints[i].Label,
                        LowerBound = lowerBound,
                        UpperBound = upperBound,
                        IsOutlier = true
                    });
                }
            }

            _logger.LogInformation("Found {OutlierCount} outliers in {SeriesName}", outliers.Count, series.Name);
            return outliers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting outliers");
            return new List<OutlierInfo>();
        }
    }

    // Calculate trend (linear regression slope)
    public double CalculateTrend(ChartSeries series)
    {
        try
        {
            var values = series.DataPoints.Select(dp => dp.Value).ToList();
            if (values.Count < 2)
                return 0;

            var n = values.Count;
            var xMean = (n - 1) / 2.0;
            var yMean = values.Average();

            var numerator = 0.0;
            var denominator = 0.0;

            for (int i = 0; i < n; i++)
            {
                var dx = i - xMean;
                var dy = values[i] - yMean;
                numerator += dx * dy;
                denominator += dx * dx;
            }

            var slope = denominator == 0 ? 0 : numerator / denominator;
            _logger.LogDebug("Trend calculated for {SeriesName}: {Slope}", series.Name, slope);
            return slope;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating trend");
            return 0;
        }
    }

    private double _calculateMode(List<double> values)
    {
        var grouped = values.GroupBy(v => v).OrderByDescending(g => g.Count());
        return grouped.FirstOrDefault()?.Key ?? values.Average();
    }

    private double _calculateSkewness(List<double> values, double mean, double stdDev)
    {
        if (stdDev == 0 || values.Count == 0)
            return 0;

        var cubedDiffs = values.Sum(v => Math.Pow(v - mean, 3));
        return cubedDiffs / (values.Count * Math.Pow(stdDev, 3));
    }

    private double _calculateKurtosis(List<double> values, double mean, double stdDev)
    {
        if (stdDev == 0 || values.Count == 0)
            return 0;

        var fourthDiffs = values.Sum(v => Math.Pow(v - mean, 4));
        return (fourthDiffs / (values.Count * Math.Pow(stdDev, 4))) - 3;
    }
}

public class SeriesStatistics
{
    public string SeriesName { get; set; }
    public int Count { get; set; }
    public double Sum { get; set; }
    public double Mean { get; set; }
    public double Median { get; set; }
    public double Mode { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Range { get; set; }
    public double Variance { get; set; }
    public double StandardDeviation { get; set; }
    public double CoefficientOfVariation { get; set; }
    public double Q1 { get; set; }
    public double Q3 { get; set; }
    public double IQR { get; set; }
    public double Skewness { get; set; }
    public double Kurtosis { get; set; }
}

public class OutlierInfo
{
    public int Index { get; set; }
    public double Value { get; set; }
    public string Label { get; set; }
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public bool IsOutlier { get; set; }
}
