// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Diagnostics;

namespace SkiaSharpChartEngine.Middleware;

/// <summary>
/// Extension methods for <see cref="PerformanceMonitoringMiddleware"/> to provide additional functionality
/// and convenience methods for performance monitoring scenarios.
/// </summary>
public static class PerformanceMonitoringMiddlewareExtensions
{
    /// <summary>
    /// Creates a performance context and automatically starts monitoring for a given operation.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="requestId">The request identifier</param>
    /// <param name="operationName">The name of the operation being monitored</param>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> or <paramref name="requestId"/> or <paramref name="operationName"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="requestId"/> or <paramref name="operationName"/> is null or whitespace</exception>
    /// <returns>A performance context ready for monitoring</returns>
    public static PerformanceContext StartMonitoring(this PerformanceMonitoringMiddleware middleware, string requestId, string operationName)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        return middleware.StartRequest(requestId, operationName);
    }

    /// <summary>
    /// Completes monitoring for the given context and returns performance statistics.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="context">The performance context to complete</param>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> or <paramref name="context"/> is null</exception>
    /// <returns>Performance statistics for the completed operation</returns>
    public static PerformanceStatistics EndMonitoring(this PerformanceMonitoringMiddleware middleware, PerformanceContext context)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(context);

        middleware.EndRequest(context);
        return middleware.GetStatistics(context.OperationName);
    }

    /// <summary>
    /// Checks if the operation exceeded the slow threshold and returns the result.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="context">The performance context to check</param>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> or <paramref name="context"/> is null</exception>
    /// <returns>True if the operation was slow, false otherwise</returns>
    public static bool WasSlowOperation(this PerformanceMonitoringMiddleware middleware, PerformanceContext context)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(context);

        return context.ElapsedMilliseconds > middleware.GetSlowThreshold();
    }

    /// <summary>
    /// Gets the slow operation threshold in milliseconds.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null</exception>
    /// <returns>The slow operation threshold in milliseconds</returns>
    public static long GetSlowThreshold(this PerformanceMonitoringMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        return middleware.GetType()
            .GetField("_slowThresholdMs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(middleware) as long? ?? 1000L;
    }

    /// <summary>
    /// Gets a formatted performance report for the given operation.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="operationName">The name of the operation to report on</param>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null</exception>
    /// <exception cref="ArgumentException"><paramref name="operationName"/> is null or whitespace</exception>
    /// <returns>A formatted string containing performance statistics</returns>
    public static string GetPerformanceReport(this PerformanceMonitoringMiddleware middleware, string operationName)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var statistics = middleware.GetStatistics(operationName);

        if (statistics is null)
        {
            return $"No performance data available for operation '{operationName}'";
        }

        return $$"Performance Report for '{statistics.OperationName}'
========================================
Average: {statistics.AverageMs}ms
Minimum: {statistics.MinMs}ms
Maximum: {statistics.MaxMs}ms
Median: {statistics.MedianMs}ms
P95: {statistics.P95Ms}ms
P99: {statistics.P99Ms}ms
Total Calls: {statistics.SampleCount}
Success Rate: {statistics.SuccessRate:F1}%
Collected At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}
========================================";
    }

    /// <summary>
    /// Gets performance statistics for all tracked operations.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null</exception>
    /// <returns>A dictionary mapping operation names to their statistics</returns>
    public static Dictionary<string, PerformanceStatistics> GetAllOperationStatistics(this PerformanceMonitoringMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        var allStats = new Dictionary<string, PerformanceStatistics>();
        var performanceMonitor = middleware.GetType()
            .GetField("_performanceMonitor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(middleware) as PerformanceMonitor;

        if (performanceMonitor is not null)
        {
            var allStatsList = performanceMonitor.GetAllStatistics();
            foreach (var stat in allStatsList)
            {
                allStats[stat.OperationName] = stat;
            }
        }

        return allStats;
    }

    /// <summary>
    /// Checks if the operation was successful based on elapsed time and memory usage.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="context">The performance context to evaluate</param>
    /// <param name="maxMemoryThresholdKb">Maximum acceptable memory usage in KB</param>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> or <paramref name="context"/> is null</exception>
    /// <returns>True if the operation was successful, false otherwise</returns>
    public static bool IsOperationSuccessful(this PerformanceMonitoringMiddleware middleware, PerformanceContext context, long maxMemoryThresholdKb = 1024)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentNullException.ThrowIfNull(context);

        var memoryThresholdBytes = maxMemoryThresholdKb * 1024;
        var slowThresholdMs = middleware.GetSlowThreshold();
        var isWithinTime = context.ElapsedMilliseconds <= slowThresholdMs;
        var isWithinMemory = context.MemoryUsedBytes <= memoryThresholdBytes;

        return isWithinTime && isWithinMemory;
    }

    /// <summary>
    /// Gets the top N slowest operations by average execution time.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="topN">Number of slowest operations to return</param>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="topN"/> is less than or equal to zero</exception>
    /// <returns>List of slowest operations sorted by average time</returns>
    public static List<PerformanceStatistics> GetSlowestOperations(this PerformanceMonitoringMiddleware middleware, int topN = 5)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(topN, 0);

        var allStats = middleware.GetAllOperationStatistics();
        return allStats.Values
            .Where(stat => stat is not null && stat.SampleCount > 0)
            .OrderByDescending(stat => stat.AverageMs)
            .Take(topN)
            .ToList();
    }
}