// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Extension methods for <see cref="RenderMetrics"/> providing additional functionality
/// for formatting and performance analysis.
/// </summary>
public static class RenderMetricsExtensions
{
    /// <summary>
    /// Gets a formatted string representation of the metrics with additional details.
    /// </summary>
    /// <param name="metrics">The render metrics instance. Cannot be null.</param>
    /// <returns>Formatted string with metrics details.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static string ToDetailedString(this RenderMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

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
    /// Formats a byte size into a human-readable string.
    /// </summary>
    /// <param name="bytes">Number of bytes to format. Must be non-negative.</param>
    /// <returns>Formatted string with appropriate unit (B, KB, MB, GB, or TB).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="bytes"/> is negative.</exception>
    private static string FormatSize(long bytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(bytes);

        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
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
    /// Checks if the render operation was fast (under 100ms).
    /// </summary>
    /// <param name="metrics">The render metrics instance. Cannot be null.</param>
    /// <returns>True if render time was under 100ms; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    public static bool IsFastRender(this RenderMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);

        return metrics.RenderTimeMs < 100;
    }

    /// <summary>
    /// Gets a comparison string showing how this render compares to a baseline.
    /// </summary>
    /// <param name="metrics">The render metrics instance. Cannot be null.</param>
    /// <param name="baselineTimeMs">Baseline render time in milliseconds. Must be non-negative.</param>
    /// <returns>Comparison string indicating whether the render was faster, slower, or equal to baseline.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="metrics"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="baselineTimeMs"/> is negative.</exception>
    public static string CompareToBaseline(this RenderMetrics metrics, long baselineTimeMs)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentOutOfRangeException.ThrowIfNegative(baselineTimeMs);

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