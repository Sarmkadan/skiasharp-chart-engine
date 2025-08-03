// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Diagnostics;

namespace SkiaSharpChartEngine.Middleware;

/// <summary>
/// Middleware for monitoring request performance and latency.
/// Tracks execution time, memory usage, and generates alerts for slow operations.
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly PerformanceMonitor _performanceMonitor;
    private readonly long _slowThresholdMs;

    public PerformanceMonitoringMiddleware(ILogger<PerformanceMonitoringMiddleware> logger, long slowThresholdMs = 1000)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _performanceMonitor = new PerformanceMonitor(logger);
        _slowThresholdMs = slowThresholdMs;
    }

    // Begin request timing
    public PerformanceContext StartRequest(string requestId, string operationName)
    {
        try
        {
            var context = new PerformanceContext
            {
                RequestId = requestId,
                OperationName = operationName,
                StartTime = DateTime.UtcNow,
                Stopwatch = Stopwatch.StartNew(),
                StartMemory = GC.GetTotalMemory(false)
            };

            _logger.LogDebug("Performance monitoring started: {RequestId}", requestId);
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting performance monitoring");
            return null;
        }
    }

    // End request timing and log results
    public void EndRequest(PerformanceContext context)
    {
        if (context == null)
            return;

        try
        {
            context.Stopwatch?.Stop();
            context.EndTime = DateTime.UtcNow;
            context.EndMemory = GC.GetTotalMemory(false);
            context.ElapsedMilliseconds = context.Stopwatch?.ElapsedMilliseconds ?? 0;

            // Calculate memory delta
            context.MemoryUsedBytes = context.EndMemory - context.StartMemory;

            // Log performance metrics
            LogPerformanceMetrics(context);

            // Alert if operation was slow
            if (context.ElapsedMilliseconds > _slowThresholdMs)
            {
                _logger.LogWarning(
                    "Slow operation detected: {OperationName} took {ElapsedMs}ms (threshold: {Threshold}ms)",
                    context.OperationName,
                    context.ElapsedMilliseconds,
                    _slowThresholdMs);
            }

            // Record metrics
            _performanceMonitor.RecordMetric(context.OperationName, context.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending performance monitoring");
        }
    }

    // Log detailed performance metrics
    private void LogPerformanceMetrics(PerformanceContext context)
    {
        _logger.LogInformation(
            "Performance: {OperationName} | Duration: {ElapsedMs}ms | Memory: {MemoryKb}KB | RequestId: {RequestId}",
            context.OperationName,
            context.ElapsedMilliseconds,
            context.MemoryUsedBytes / 1024,
            context.RequestId);
    }

    // Get performance statistics
    public PerformanceStatistics GetStatistics(string operationName)
    {
        return _performanceMonitor.GetStatistics(operationName);
    }

    // Set slow threshold
    public void SetSlowThreshold(long thresholdMs)
    {
        if (thresholdMs > 0)
        {
            _logger.LogInformation("Updated slow operation threshold to {Threshold}ms", thresholdMs);
        }
    }
}

/// <summary>
/// Context object for tracking performance metrics during request processing.
/// </summary>
public class PerformanceContext
{
    public string RequestId { get; set; }
    public string OperationName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Stopwatch Stopwatch { get; set; }
    public long StartMemory { get; set; }
    public long EndMemory { get; set; }
    public long MemoryUsedBytes { get; set; }
    public long ElapsedMilliseconds { get; set; }
}

/// <summary>
/// Statistics for performance metrics.
/// </summary>
public class PerformanceStatistics
{
    public string OperationName { get; set; }
    public long AverageMs { get; set; }
    public long MinMs { get; set; }
    public long MaxMs { get; set; }
    public long TotalCalls { get; set; }
    public DateTime CollectedAt { get; set; }
}
