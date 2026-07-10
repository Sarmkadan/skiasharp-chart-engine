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
    /// <returns>A performance context ready for monitoring</returns>
    public static PerformanceContext StartMonitoring(this PerformanceMonitoringMiddleware middleware, string requestId, string operationName)
    {
        if (middleware == null)
            throw new ArgumentNullException(nameof(middleware));

        if (string.IsNullOrWhiteSpace(requestId))
            throw new ArgumentException("Request ID cannot be null or empty", nameof(requestId));

        if (string.IsNullOrWhiteSpace(operationName))
            throw new ArgumentException("Operation name cannot be null or empty", nameof(operationName));

        return middleware.StartRequest(requestId, operationName);
    }

    /// <summary>
    /// Completes monitoring for the given context and returns performance statistics.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="context">The performance context to complete</param>
    /// <returns>Performance statistics for the completed operation</returns>
    public static PerformanceStatistics EndMonitoring(this PerformanceMonitoringMiddleware middleware, PerformanceContext context)
    {
        if (middleware == null)
            throw new ArgumentNullException(nameof(middleware));

        if (context == null)
            throw new ArgumentNullException(nameof(context));

        middleware.EndRequest(context);
        return middleware.GetStatistics(context.OperationName);
    }

    /// <summary>
    /// Checks if the operation exceeded the slow threshold and returns the result.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="context">The performance context to check</param>
    /// <returns>True if the operation was slow, false otherwise</returns>
    public static bool WasSlowOperation(this PerformanceMonitoringMiddleware middleware, PerformanceContext context)
    {
        if (middleware == null)
            throw new ArgumentNullException(nameof(middleware));

        if (context == null)
            throw new ArgumentNullException(nameof(context));

        return context.ElapsedMilliseconds > _slowThresholdMs;
    }

    /// <summary>
    /// Gets a formatted performance report for the given operation.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="operationName">The name of the operation to report on</param>
    /// <returns>A formatted string containing performance statistics</returns>
    public static string GetPerformanceReport(this PerformanceMonitoringMiddleware middleware, string operationName)
    {
        if (middleware == null)
            throw new ArgumentNullException(nameof(middleware));

        var statistics = middleware.GetStatistics(operationName);

        if (statistics == null)
            return $"No performance data available for operation '{operationName}'";

        return $"Performance Report for '{statistics.OperationName}'\n" +
               $"========================================\n" +
               $"Average: {statistics.AverageMs}ms\n" +
               $"Minimum: {statistics.MinMs}ms\n" +
               $"Maximum: {statistics.MaxMs}ms\n" +
               $"Median: {statistics.MedianMs}ms\n" +
               $"P95: {statistics.P95Ms}ms\n" +
               $"P99: {statistics.P99Ms}ms\n" +
               $"Total Calls: {statistics.SampleCount}\n" +
               $"Success Rate: {statistics.SuccessRate:F1}%\n" +
               $"Collected At: {statistics.CollectedAt:yyyy-MM-dd HH:mm:ss}\n" +
               $"========================================";
    }

    /// <summary>
    /// Gets performance statistics for all tracked operations.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <returns>A dictionary mapping operation names to their statistics</returns>
    public static Dictionary<string, PerformanceStatistics> GetAllOperationStatistics(this PerformanceMonitoringMiddleware middleware)
    {
        if (middleware == null)
            throw new ArgumentNullException(nameof(middleware));

        var allStats = new Dictionary<string, PerformanceStatistics>();
        var performanceMonitor = middleware.GetType().GetField("_performanceMonitor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(middleware) as PerformanceMonitor;

        if (performanceMonitor != null)
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
    /// <returns>True if the operation was successful, false otherwise</returns>
    public static bool IsOperationSuccessful(this PerformanceMonitoringMiddleware middleware, PerformanceContext context, long maxMemoryThresholdKb = 1024)
    {
        if (middleware == null)
            throw new ArgumentNullException(nameof(middleware));

        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var memoryThresholdBytes = maxMemoryThresholdKb * 1024;
        var isWithinTime = context.ElapsedMilliseconds <= _slowThresholdMs;
        var isWithinMemory = context.MemoryUsedBytes <= memoryThresholdBytes;

        return isWithinTime && isWithinMemory;
    }

    /// <summary>
    /// Gets the top N slowest operations by average execution time.
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="topN">Number of slowest operations to return</param>
    /// <returns>List of slowest operations sorted by average time</returns>
    public static List<PerformanceStatistics> GetSlowestOperations(this PerformanceMonitoringMiddleware middleware, int topN = 5)
    {
        if (middleware == null)
            throw new ArgumentNullException(nameof(middleware));

        if (topN <= 0)
            throw new ArgumentOutOfRangeException(nameof(topN), "Top N must be greater than zero");

        var allStats = middleware.GetAllOperationStatistics();
        return allStats.Values
            .Where(stat => stat != null && stat.SampleCount > 0)
            .OrderByDescending(stat => stat.AverageMs)
            .Take(topN)
            .ToList();
    }
}