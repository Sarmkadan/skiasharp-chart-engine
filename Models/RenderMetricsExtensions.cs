// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Extension methods for RenderMetrics providing additional functionality
/// </summary>
public static class RenderMetricsExtensions
{
    /// <summary>
    /// Calculates the rendering speed in megabytes per second
    /// </summary>
    /// <param name="metrics">The render metrics instance</param>
    /// <returns>Speed in MB/s, or 0 if render time is 0</returns>
    public static double GetMegabytesPerSecond(this RenderMetrics metrics)
    {
        if (metrics == null)
            throw new ArgumentNullException(nameof(metrics));

        if (metrics.RenderTimeMs == 0)
            return 0;

        var megabytes = metrics.ImageSizeBytes / (1024.0 * 1024.0);
        var seconds = metrics.RenderTimeMs / 1000.0;
        return megabytes / seconds;
    }

    /// <summary>
    /// Calculates the data points processing rate
    /// </summary>
    /// <param name="metrics">The render metrics instance</param>
    /// <returns>Data points per second, or 0 if render time is 0</returns>
    public static double GetDataPointsPerSecond(this RenderMetrics metrics)
    {
        if (metrics == null)
            throw new ArgumentNullException(nameof(metrics));

        if (metrics.RenderTimeMs == 0)
            return 0;

        return (metrics.DataPointCount * 1000.0) / metrics.RenderTimeMs;
    }

    /// <summary>
    /// Gets a formatted string representation of the metrics with additional details
    /// </summary>
    /// <param name="metrics">The render metrics instance</param>
    /// <returns>Formatted string with metrics details</returns>
    public static string ToDetailedString(this RenderMetrics metrics)
    {
        if (metrics == null)
            throw new ArgumentNullException(nameof(metrics));

        var mbPerSec = metrics.GetMegabytesPerSecond();
        var pointsPerSec = metrics.GetDataPointsPerSecond();
        var cacheStatus = metrics.WasCached ? "CACHED" : "COMPUTED";

        return $@"RenderMetrics [Chart: {metrics.ChartId ?? "null"}]
  Rendered: {metrics.RenderedAt:yyyy-MM-dd HH:mm:ss}
  Duration: {metrics.RenderTimeMs}ms | Size: {FormatSize(metrics.ImageSizeBytes)}
  Format: {metrics.ExportFormat} | Status: {cacheStatus}
  Performance: {mbPerSec:F2} MB/s | {pointsPerSec:F0} points/sec
  Data: {metrics.SeriesCount} series × {metrics.DataPointCount} points
  Cache: {metrics.CacheSizeAtRenderTime} bytes at render time" +
        (metrics.AdditionalMetrics != null && metrics.AdditionalMetrics.Count > 0
            ? "\n  Additional: " + string.Join(", ", metrics.AdditionalMetrics.Keys)
            : "");
    }

    /// <summary>
    /// Formats a byte size into a human-readable string
    /// </summary>
    /// <param name="bytes">Number of bytes</param>
    /// <returns>Formatted string with appropriate unit</returns>
    private static string FormatSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        double value = bytes;

        while (value >= 1024 && counter < suffixes.Length - 1)
        {
            value /= 1024;
            counter++;
        }

        return $"{value:F2} {suffixes[counter]}";
    }

    /// <summary>
    /// Checks if the render operation was fast (under 100ms)
    /// </summary>
    /// <param name="metrics">The render metrics instance</param>
    /// <returns>True if render time was under 100ms</returns>
    public static bool IsFastRender(this RenderMetrics metrics)
    {
        if (metrics == null)
            throw new ArgumentNullException(nameof(metrics));

        return metrics.RenderTimeMs < 100;
    }

    /// <summary>
    /// Gets a comparison string showing how this render compares to a baseline
    /// </summary>
    /// <param name="metrics">The render metrics instance</param>
    /// <param name="baselineTimeMs">Baseline render time in milliseconds</param>
    /// <returns>Comparison string</returns>
    public static string CompareToBaseline(this RenderMetrics metrics, long baselineTimeMs)
    {
        if (metrics == null)
            throw new ArgumentNullException(nameof(metrics));

        var diff = metrics.RenderTimeMs - baselineTimeMs;
        var pct = baselineTimeMs > 0 ? (diff / (double)baselineTimeMs) * 100 : 0;

        var comparison = diff switch
        {
            < 0 => "FASTER",
            > 0 => "SLOWER",
            _ => "EQUAL"
        };

        return $"{comparison} by {Math.Abs(diff)}ms ({Math.Abs(pct):F1}%)";
    }
}