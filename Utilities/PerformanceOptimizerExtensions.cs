using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Extension methods for PerformanceOptimizer providing essential performance optimization utilities.
/// </summary>
public static class PerformanceOptimizerExtensions
{
    /// <summary>
    /// Determines if optimization is strongly recommended based on critical severity issues.
    /// </summary>
    public static bool IsOptimizationStronglyRecommended(this PerformanceOptimizer optimizer, Chart chart)
    {
        var analysis = optimizer.AnalyzeChart(chart);
        return analysis?.Recommendations.Any(r => r.Severity >= Severity.High) ?? false;
    }

    /// <summary>
    /// Gets a brief summary report of the performance analysis.
    /// </summary>
    public static string GetOptimizationSummary(this PerformanceOptimizer optimizer, Chart chart)
    {
        var analysis = optimizer.AnalyzeChart(chart);
        if (analysis == null) return "No analysis available.";
        
        return $"Chart {analysis.ChartId} has {analysis.Recommendations.Count} recommendations. " +
               $"Memory estimate: {optimizer.EstimateMemoryUsage(chart) / 1024} KB.";
    }

    /// <summary>
    /// Safely downsamples data points, ensuring no errors occur if data is already below target.
    /// </summary>
    public static List<DataPoint> SafeDownsample(this PerformanceOptimizer optimizer, List<DataPoint> data, int target)
    {
        if (data == null || data.Count <= target) return data ?? new List<DataPoint>();
        return optimizer.Downsample(data, target);
    }
}
