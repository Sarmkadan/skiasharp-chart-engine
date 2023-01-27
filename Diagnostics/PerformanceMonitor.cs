// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Diagnostics;

/// <summary>
/// Monitors and tracks performance metrics for chart operations
/// Provides insights into rendering performance and bottlenecks
/// </summary>
public class PerformanceMonitor
{
    private readonly ILogger<PerformanceMonitor> _logger;
    private readonly Dictionary<string, List<PerformanceMetric>> _metrics;
    private readonly object _lock = new();
    private readonly int _maxMetricsPerOperation = 1000;

    public PerformanceMonitor(ILogger<PerformanceMonitor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = new Dictionary<string, List<PerformanceMetric>>();
    }

    /// <summary>
    /// Records a performance metric
    /// </summary>
    public void RecordMetric(string operationName, long elapsedMilliseconds, bool success = true)
    {
        if (string.IsNullOrEmpty(operationName))
            throw new ArgumentException("Operation name cannot be empty", nameof(operationName));

        var metric = new PerformanceMetric
        {
            OperationName = operationName,
            ElapsedMilliseconds = elapsedMilliseconds,
            Success = success,
            Timestamp = DateTime.UtcNow
        };

        lock (_lock)
        {
            if (!_metrics.ContainsKey(operationName))
            {
                _metrics[operationName] = new List<PerformanceMetric>();
            }

            var list = _metrics[operationName];
            list.Add(metric);

            // Keep only recent metrics to prevent memory bloat
            if (list.Count > _maxMetricsPerOperation)
            {
                list.RemoveRange(0, list.Count - _maxMetricsPerOperation);
            }
        }

        _logger.LogDebug("Performance metric recorded: {Operation} - {ElapsedMs}ms",
            operationName, elapsedMilliseconds);
    }

    /// <summary>
    /// Gets performance statistics for an operation
    /// </summary>
    public PerformanceStatistics? GetStatistics(string operationName)
    {
        lock (_lock)
        {
            if (!_metrics.TryGetValue(operationName, out var list) || list.Count == 0)
                return null;

            var successfulMetrics = list.Where(m => m.Success).Select(m => m.ElapsedMilliseconds).ToList();
            if (successfulMetrics.Count == 0)
                return null;

            var times = successfulMetrics.OrderBy(t => t).ToList();
            var avg = (long)times.Average();
            var min = times.First();
            var max = times.Last();
            var median = times[times.Count / 2];
            var p95 = times[(int)(times.Count * 0.95)];
            var p99 = times[(int)(times.Count * 0.99)];

            return new PerformanceStatistics
            {
                OperationName = operationName,
                SampleCount = successfulMetrics.Count,
                FailureCount = list.Count(m => !m.Success),
                AverageMs = avg,
                MinMs = min,
                MaxMs = max,
                MedianMs = median,
                P95Ms = p95,
                P99Ms = p99
            };
        }
    }

    /// <summary>
    /// Gets all recorded statistics
    /// </summary>
    public List<PerformanceStatistics> GetAllStatistics()
    {
        var stats = new List<PerformanceStatistics>();

        lock (_lock)
        {
            foreach (var operationName in _metrics.Keys)
            {
                var stat = GetStatistics(operationName);
                if (stat != null)
                    stats.Add(stat);
            }
        }

        return stats;
    }

    /// <summary>
    /// Clears all metrics
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _metrics.Clear();
        }

        _logger.LogInformation("Performance metrics cleared");
    }

    /// <summary>
    /// Clears metrics for specific operation
    /// </summary>
    public void ClearOperation(string operationName)
    {
        lock (_lock)
        {
            _metrics.Remove(operationName);
        }

        _logger.LogDebug("Performance metrics cleared for operation {Operation}", operationName);
    }

    /// <summary>
    /// Gets number of operations being tracked
    /// </summary>
    public int GetTrackedOperationCount()
    {
        lock (_lock)
        {
            return _metrics.Count;
        }
    }

    /// <summary>
    /// Gets total number of recorded metrics
    /// </summary>
    public int GetTotalMetricCount()
    {
        lock (_lock)
        {
            return _metrics.Values.Sum(list => list.Count);
        }
    }
}

/// <summary>
/// Individual performance metric
/// </summary>
public class PerformanceMetric
{
    public string OperationName { get; set; } = string.Empty;
    public long ElapsedMilliseconds { get; set; }
    public bool Success { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Performance statistics for an operation
/// </summary>
public class PerformanceStatistics
{
    public string OperationName { get; set; } = string.Empty;
    public int SampleCount { get; set; }
    public int FailureCount { get; set; }
    public long AverageMs { get; set; }
    public long MinMs { get; set; }
    public long MaxMs { get; set; }
    public long MedianMs { get; set; }
    public long P95Ms { get; set; }
    public long P99Ms { get; set; }

    public double SuccessRate => SampleCount > 0 ? (100.0 * SampleCount / (SampleCount + FailureCount)) : 0;

    public override string ToString()
    {
        return $"{OperationName}: Avg={AverageMs}ms, Min={MinMs}ms, Max={MaxMs}ms, P95={P95Ms}ms, " +
               $"Success={SuccessRate:F1}% ({SampleCount} samples)";
    }
}
