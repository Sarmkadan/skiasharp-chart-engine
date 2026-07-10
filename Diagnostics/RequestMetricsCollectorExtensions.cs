// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SkiaSharpChartEngine.Diagnostics;

/// <summary>
/// Extension methods for RequestMetricsCollector providing additional metric analysis capabilities
/// </summary>
public static class RequestMetricsCollectorExtensions
{
    /// <summary>
    /// Gets the most active endpoints based on request volume
    /// </summary>
    /// <param name="collector">The metrics collector instance</param>
    /// <param name="count">Number of top endpoints to return</param>
    /// <returns>Collection of endpoint metrics ordered by request count (descending)</returns>
    /// <exception cref="ArgumentNullException">Thrown when collector is null</exception>
    public static IReadOnlyList<EndpointMetrics> GetTopEndpointsByRequests(this RequestMetricsCollector collector, int count = 10)
    {
        ArgumentNullException.ThrowIfNull(collector);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        var allMetrics = collector.GetAllEndpointMetrics();
        return allMetrics.OrderByDescending(m => m.RequestCount).Take(count).ToList();
    }

    /// <summary>
    /// Gets endpoints with the highest error rates
    /// </summary>
    /// <param name="collector">The metrics collector instance</param>
    /// <param name="count">Number of endpoints to return</param>
    /// <returns>Collection of endpoint metrics ordered by error rate (ascending)</returns>
    /// <exception cref="ArgumentNullException">Thrown when collector is null</exception>
    public static IReadOnlyList<EndpointMetrics> GetEndpointsWithHighestErrorRates(this RequestMetricsCollector collector, int count = 10)
    {
        ArgumentNullException.ThrowIfNull(collector);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        var allMetrics = collector.GetAllEndpointMetrics();
        return allMetrics
            .Where(m => m.RequestCount > 0)
            .OrderBy(m => m.SuccessRate)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Gets endpoints that have exceeded a specific response time threshold
    /// </summary>
    /// <param name="collector">The metrics collector instance</param>
    /// <param name="thresholdMs">Response time threshold in milliseconds</param>
    /// <returns>Collection of endpoint metrics that exceed the threshold</returns>
    /// <exception cref="ArgumentNullException">Thrown when collector is null</exception>
    public static IReadOnlyList<EndpointMetrics> GetEndpointsExceedingResponseTime(this RequestMetricsCollector collector, long thresholdMs)
    {
        ArgumentNullException.ThrowIfNull(collector);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(thresholdMs);

        var allMetrics = collector.GetAllEndpointMetrics();
        return allMetrics
            .Where(m => m.AverageResponseTimeMs > thresholdMs)
            .OrderByDescending(m => m.AverageResponseTimeMs)
            .ToList();
    }

    /// <summary>
    /// Gets a summary of metrics formatted as a human-readable string
    /// </summary>
    /// <param name="collector">The metrics collector instance</param>
    /// <param name="cultureInfo">Culture info for formatting numbers (defaults to InvariantCulture)</param>
    /// <returns>Formatted summary string</returns>
    /// <exception cref="ArgumentNullException">Thrown when collector is null</exception>
    public static string GetMetricsSummary(this RequestMetricsCollector collector, CultureInfo? cultureInfo = null)
    {
        ArgumentNullException.ThrowIfNull(collector);

        cultureInfo ??= CultureInfo.InvariantCulture;

        var systemMetrics = collector.GetSystemMetrics();
        var topEndpoints = collector.GetTopEndpointsByRequests(5);

        var summary = new System.Text.StringBuilder();
        summary.AppendLine("=== Request Metrics Summary ===");
        summary.AppendLine($"Period: {systemMetrics.SamplePeriod.TotalMinutes.ToString("F1", cultureInfo)} minutes");
        summary.AppendLine($"Endpoints: {systemMetrics.MonitoredEndpoints}");
        summary.AppendLine($"Total Requests: {systemMetrics.TotalRequests.ToString("N0", cultureInfo)}");
        summary.AppendLine($"Successful: {systemMetrics.TotalSuccessful.ToString("N0", cultureInfo)} ({systemMetrics.OverallSuccessRate.ToString("F1", cultureInfo)}%)");
        summary.AppendLine($"Errors: {systemMetrics.TotalErrors.ToString("N0", cultureInfo)}");
        summary.AppendLine($"Avg Response Time: {systemMetrics.AverageResponseTimeMs.ToString("F0", cultureInfo)} ms");
        summary.AppendLine($"Min/Max Response Time: {systemMetrics.MinResponseTimeMs}/{systemMetrics.MaxResponseTimeMs} ms");

        if (topEndpoints.Count > 0)
        {
            summary.AppendLine("\n=== Top 5 Endpoints by Request Volume ===");
            foreach (var endpoint in topEndpoints)
            {
                summary.AppendLine($"{endpoint.Endpoint}: {endpoint.RequestCount.ToString("N0", cultureInfo)} requests, " +
                                 $"{endpoint.SuccessRate.ToString("F1", cultureInfo)}% success, " +
                                 $"{endpoint.AverageResponseTimeMs.ToString("F0", cultureInfo)}ms avg");
            }
        }

        return summary.ToString();
    }
}