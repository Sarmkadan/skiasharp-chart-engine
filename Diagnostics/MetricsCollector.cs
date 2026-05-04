// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Diagnostics;

/// <summary>
/// Collects and analyzes diagnostic metrics for chart rendering operations.
/// Tracks CPU usage, memory consumption, and rendering times.
/// </summary>
public class MetricsCollector
{
    private readonly ILogger<MetricsCollector> _logger;
    private readonly Dictionary<string, OperationMetrics> _metrics;
    private readonly object _lockObject = new object();

    public MetricsCollector(ILogger<MetricsCollector> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = new Dictionary<string, OperationMetrics>();
    }

    // Begin collecting metrics for operation
    public MetricsContext StartCollection(string operationName)
    {
        try
        {
            var context = new MetricsContext
            {
                OperationName = operationName,
                StartTime = DateTime.UtcNow,
                Stopwatch = Stopwatch.StartNew(),
                StartMemory = GC.GetTotalMemory(false),
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };

            _logger.LogDebug("Metrics collection started for: {OperationName}", operationName);
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting metrics collection");
            return null;
        }
    }

    // End metrics collection
    public void EndCollection(MetricsContext context)
    {
        if (context == null)
            return;

        try
        {
            context.Stopwatch?.Stop();
            context.EndTime = DateTime.UtcNow;
            context.EndMemory = GC.GetTotalMemory(false);
            context.ElapsedMs = context.Stopwatch?.ElapsedMilliseconds ?? 0;
            context.MemoryUsedBytes = Math.Max(0, context.EndMemory - context.StartMemory);

            lock (_lockObject)
            {
                if (!_metrics.TryGetValue(context.OperationName, out var metrics))
                {
                    metrics = new OperationMetrics { OperationName = context.OperationName };
                    _metrics[context.OperationName] = metrics;
                }

                metrics.Executions.Add(new ExecutionMetric
                {
                    ExecutedAt = context.EndTime,
                    ElapsedMs = context.ElapsedMs,
                    MemoryUsedBytes = context.MemoryUsedBytes,
                    ThreadId = context.ThreadId
                });

                // Limit history to prevent unbounded growth
                if (metrics.Executions.Count > 1000)
                {
                    metrics.Executions.RemoveAt(0);
                }
            }

            _logger.LogDebug("Metrics collected: {OperationName} - {ElapsedMs}ms, {MemoryKb}KB",
                context.OperationName, context.ElapsedMs, context.MemoryUsedBytes / 1024);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending metrics collection");
        }
    }

    // Get metrics for operation
    public OperationMetrics GetMetrics(string operationName)
    {
        lock (_lockObject)
        {
            return _metrics.TryGetValue(operationName, out var metrics) ? metrics : null;
        }
    }

    // Get summary statistics
    public MetricsSummary GetSummary(string operationName)
    {
        lock (_lockObject)
        {
            if (!_metrics.TryGetValue(operationName, out var metrics) || metrics.Executions.Count == 0)
                return null;

            var executions = metrics.Executions;
            var times = executions.Select(e => e.ElapsedMs).OrderBy(t => t).ToList();
            var memories = executions.Select(e => e.MemoryUsedBytes).OrderBy(m => m).ToList();

            return new MetricsSummary
            {
                OperationName = operationName,
                ExecutionCount = executions.Count,
                AverageTimeMs = times.Average(),
                MinTimeMs = times.First(),
                MaxTimeMs = times.Last(),
                MedianTimeMs = times[times.Count / 2],
                AverageMemoryBytes = (long)memories.Average(),
                MinMemoryBytes = memories.First(),
                MaxMemoryBytes = memories.Last(),
                LastExecutedAt = executions.Last().ExecutedAt
            };
        }
    }

    // Get all operation summaries
    public Dictionary<string, MetricsSummary> GetAllSummaries()
    {
        lock (_lockObject)
        {
            var summaries = new Dictionary<string, MetricsSummary>();

            foreach (var opName in _metrics.Keys)
            {
                var summary = GetSummary(opName);
                if (summary != null)
                    summaries[opName] = summary;
            }

            return summaries;
        }
    }

    // Clear metrics
    public void Clear()
    {
        lock (_lockObject)
        {
            _metrics.Clear();
        }

        _logger.LogInformation("All metrics cleared");
    }

    // Get total operations tracked
    public int GetOperationCount() => _metrics.Count;
}

public class MetricsContext
{
    public string OperationName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Stopwatch Stopwatch { get; set; }
    public long StartMemory { get; set; }
    public long EndMemory { get; set; }
    public long MemoryUsedBytes { get; set; }
    public long ElapsedMs { get; set; }
    public int ThreadId { get; set; }
}

public class OperationMetrics
{
    public string OperationName { get; set; }
    public List<ExecutionMetric> Executions { get; set; } = new List<ExecutionMetric>();
}

public class ExecutionMetric
{
    public DateTime ExecutedAt { get; set; }
    public long ElapsedMs { get; set; }
    public long MemoryUsedBytes { get; set; }
    public int ThreadId { get; set; }
}

public class MetricsSummary
{
    public string OperationName { get; set; }
    public int ExecutionCount { get; set; }
    public double AverageTimeMs { get; set; }
    public long MinTimeMs { get; set; }
    public long MaxTimeMs { get; set; }
    public long MedianTimeMs { get; set; }
    public long AverageMemoryBytes { get; set; }
    public long MinMemoryBytes { get; set; }
    public long MaxMemoryBytes { get; set; }
    public DateTime LastExecutedAt { get; set; }
}
