using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Extension methods for <see cref="PerformanceOptimizer"/> providing essential performance optimization utilities.
/// </summary>
public static class PerformanceOptimizerExtensions
{
    /// <summary>
    /// Determines if optimization is strongly recommended based on critical severity issues.
    /// </summary>
    /// <param name="optimizer">The performance optimizer instance.</param>
    /// <param name="chart">The chart to analyze.</param>
    /// <returns>True if optimization is strongly recommended; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="optimizer"/> or <paramref name="chart"/> is null.</exception>
    public static bool IsOptimizationStronglyRecommended(this PerformanceOptimizer optimizer, Chart chart)
    {
        ArgumentNullException.ThrowIfNull(optimizer);
        ArgumentNullException.ThrowIfNull(chart);

        var analysis = optimizer.AnalyzeChart(chart);
        return analysis?.Recommendations.Any(r => r.Severity >= Severity.High) ?? false;
    }

    /// <summary>
    /// Gets a brief summary report of the performance analysis.
    /// </summary>
    /// <param name="optimizer">The performance optimizer instance.</param>
    /// <param name="chart">The chart to analyze.</param>
    /// <returns>A summary string of the optimization analysis.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="optimizer"/> or <paramref name="chart"/> is null.</exception>
    public static string GetOptimizationSummary(this PerformanceOptimizer optimizer, Chart chart)
    {
        ArgumentNullException.ThrowIfNull(optimizer);
        ArgumentNullException.ThrowIfNull(chart);

        var analysis = optimizer.AnalyzeChart(chart);
        if (analysis == null)
            return "No analysis available.";

        return $"Chart {analysis.ChartId} has {analysis.Recommendations.Count} recommendations. " +
               $"Memory estimate: {optimizer.EstimateMemoryUsage(chart) / 1024} KB.";
    }

    /// <summary>
    /// Safely downsamples data points, ensuring no errors occur if data is already below target.
    /// </summary>
    /// <param name="optimizer">The performance optimizer instance.</param>
    /// <param name="data">The data points to downsample.</param>
    /// <param name="target">The target number of data points.</param>
    /// <returns>The downsampled data points, or the original data if no downsampling is needed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="optimizer"/> is null.</exception>
    public static List<DataPoint> SafeDownsample(this PerformanceOptimizer optimizer, List<DataPoint> data, int target)
    {
        ArgumentNullException.ThrowIfNull(optimizer);

        if (data == null || data.Count <= target)
            return data ?? new List<DataPoint>();

        return optimizer.Downsample(data, target);
    }
}