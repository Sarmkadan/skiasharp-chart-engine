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
/// Optimizes chart rendering performance through downsampling, simplification, and caching strategies.
/// Provides recommendations for performance improvements.
/// </summary>
public class PerformanceOptimizer
{
    private readonly ILogger<PerformanceOptimizer> _logger;

    public PerformanceOptimizer(ILogger<PerformanceOptimizer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Analyze chart for performance issues
    public PerformanceAnalysis AnalyzeChart(Chart chart)
    {
        try
        {
            if (chart == null)
                return null;

            var analysis = new PerformanceAnalysis
            {
                ChartId = chart.Id,
                AnalyzedAt = DateTime.UtcNow,
                Recommendations = new List<OptimizationRecommendation>()
            };

            // Check data point count
            var totalDataPoints = chart.Series?.Sum(s => s.DataPoints?.Count ?? 0) ?? 0;
            analysis.TotalDataPoints = totalDataPoints;

            if (totalDataPoints > 10000)
            {
                analysis.Recommendations.Add(new OptimizationRecommendation
                {
                    Category = "Data Size",
                    Severity = Severity.High,
                    Message = $"High number of data points ({totalDataPoints}). Consider downsampling.",
                    Action = "Apply data aggregation or downsampling"
                });
            }

            // Check series count
            var seriesCount = chart.Series?.Count ?? 0;
            if (seriesCount > 20)
            {
                analysis.Recommendations.Add(new OptimizationRecommendation
                {
                    Category = "Series Count",
                    Severity = Severity.Medium,
                    Message = $"Large number of series ({seriesCount}). Consider filtering.",
                    Action = "Reduce number of displayed series"
                });
            }

            // Check chart dimensions
            var config = chart.ChartConfiguration;
            if (config != null && (config.Width > 4000 || config.Height > 4000))
            {
                analysis.Recommendations.Add(new OptimizationRecommendation
                {
                    Category = "Resolution",
                    Severity = Severity.Medium,
                    Message = $"Very high resolution ({config.Width}x{config.Height}). May impact performance.",
                    Action = "Reduce chart dimensions"
                });
            }

            _logger.LogInformation("Chart analysis complete: {DataPoints} points, {Series} series, {Recommendations} recommendations",
                totalDataPoints, seriesCount, analysis.Recommendations.Count);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing chart performance");
            return null;
        }
    }

    // Downsample data for performance
    public List<DataPoint> Downsample(List<DataPoint> dataPoints, int targetCount)
    {
        try
        {
            if (dataPoints == null || dataPoints.Count <= targetCount)
                return dataPoints;

            _logger.LogInformation("Downsampling {Count} points to {TargetCount}", dataPoints.Count, targetCount);

            var ratio = (double)dataPoints.Count / targetCount;
            var downsampled = new List<DataPoint>();

            for (int i = 0; i < dataPoints.Count; i += (int)Math.Ceiling(ratio))
            {
                downsampled.Add(dataPoints[i]);
            }

            // Ensure last point is included
            if (downsampled.Last() != dataPoints.Last())
            {
                downsampled.Add(dataPoints.Last());
            }

            _logger.LogDebug("Downsampling complete: {OriginalCount} -> {DownsampledCount}", dataPoints.Count, downsampled.Count);
            return downsampled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downsampling data");
            return dataPoints;
        }
    }

    // Get memory estimate for rendering
    public long EstimateMemoryUsage(Chart chart)
    {
        try
        {
            var estimate = 0L;

            // Base estimate per chart
            estimate += 1024; // Chart metadata

            // Add for each series
            if (chart.Series != null)
            {
                foreach (var series in chart.Series)
                {
                    estimate += 512; // Series metadata
                    estimate += (series.DataPoints?.Count ?? 0) * 64; // ~64 bytes per data point
                }
            }

            // Add for rendering
            var config = chart.ChartConfiguration;
            if (config != null)
            {
                var pixelCount = (long)config.Width * config.Height;
                estimate += pixelCount * 4; // 4 bytes per pixel (RGBA)
            }

            _logger.LogDebug("Estimated memory usage for chart {ChartId}: {MemoryMb}MB", chart.Id, estimate / (1024 * 1024));
            return estimate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating memory usage");
            return 0;
        }
    }
}

/// <summary>
/// Performance analysis result.
/// </summary>
public class PerformanceAnalysis
{
    public string ChartId { get; set; }
    public int TotalDataPoints { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public List<OptimizationRecommendation> Recommendations { get; set; }

    public bool HasIssues => Recommendations.Count > 0;
}

/// <summary>
/// Optimization recommendation.
/// </summary>
public class OptimizationRecommendation
{
    public string Category { get; set; }
    public Severity Severity { get; set; }
    public string Message { get; set; }
    public string Action { get; set; }
}

public enum Severity
{
    Low,
    Medium,
    High,
    Critical
}
