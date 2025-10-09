// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharpChartEngine.Diagnostics;

/// <summary>
/// Collects and aggregates request-level metrics
/// Provides insights into API usage patterns and performance
/// </summary>
public class RequestMetricsCollector
{
    private readonly ConcurrentDictionary<string, List<RequestMetric>> _metrics;
    private readonly int _maxMetricsPerEndpoint = 10000;
    private readonly object _lock = new();

    public RequestMetricsCollector()
    {
        _metrics = new ConcurrentDictionary<string, List<RequestMetric>>();
    }

    /// <summary>
    /// Records a request metric
    /// </summary>
    public void RecordRequest(string endpoint, int statusCode, long elapsedMilliseconds, int requestSize = 0, int responseSize = 0)
    {
        if (string.IsNullOrEmpty(endpoint))
            return;

        var metric = new RequestMetric
        {
            Endpoint = endpoint,
            StatusCode = statusCode,
            ElapsedMilliseconds = elapsedMilliseconds,
            RequestSize = requestSize,
            ResponseSize = responseSize,
            Timestamp = DateTime.UtcNow,
            IsSuccess = statusCode >= 200 && statusCode < 300
        };

        if (!_metrics.TryGetValue(endpoint, out var list))
        {
            list = new List<RequestMetric>();
            _metrics.TryAdd(endpoint, list);
        }

        lock (_lock)
        {
            list.Add(metric);

            // Keep only recent metrics
            if (list.Count > _maxMetricsPerEndpoint)
            {
                list.RemoveRange(0, list.Count - _maxMetricsPerEndpoint);
            }
        }
    }

    /// <summary>
    /// Gets metrics for a specific endpoint
    /// </summary>
    public EndpointMetrics? GetEndpointMetrics(string endpoint)
    {
        if (!_metrics.TryGetValue(endpoint, out var list) || list.Count == 0)
            return null;

        lock (_lock)
        {
            var metrics = list.ToList();

            var statusCodeCounts = metrics
                .GroupBy(m => m.StatusCode)
                .ToDictionary(g => g.Key, g => g.Count());

            var successCount = metrics.Count(m => m.IsSuccess);
            var errorCount = metrics.Count(m => !m.IsSuccess);

            var times = metrics.Select(m => m.ElapsedMilliseconds).OrderBy(t => t).ToList();
            var requestSizes = metrics.Select(m => m.RequestSize).ToList();
            var responseSizes = metrics.Select(m => m.ResponseSize).ToList();

            return new EndpointMetrics
            {
                Endpoint = endpoint,
                RequestCount = metrics.Count,
                SuccessCount = successCount,
                ErrorCount = errorCount,
                SuccessRate = metrics.Count > 0 ? (100.0 * successCount / metrics.Count) : 0,
                AverageResponseTimeMs = (long)times.Average(),
                MinResponseTimeMs = times.First(),
                MaxResponseTimeMs = times.Last(),
                MedianResponseTimeMs = times[times.Count / 2],
                P95ResponseTimeMs = times[(int)(times.Count * 0.95)],
                P99ResponseTimeMs = times[(int)(times.Count * 0.99)],
                AverageRequestSize = (long)requestSizes.Average(),
                AverageResponseSize = (long)responseSizes.Average(),
                StatusCodeDistribution = statusCodeCounts,
                FirstSeenAt = metrics.First().Timestamp,
                LastSeenAt = metrics.Last().Timestamp
            };
        }
    }

    /// <summary>
    /// Gets metrics for all endpoints
    /// </summary>
    public List<EndpointMetrics> GetAllEndpointMetrics()
    {
        var allMetrics = new List<EndpointMetrics>();

        foreach (var endpoint in _metrics.Keys)
        {
            var metric = GetEndpointMetrics(endpoint);
            if (metric != null)
                allMetrics.Add(metric);
        }

        return allMetrics.OrderByDescending(m => m.RequestCount).ToList();
    }

    /// <summary>
    /// Gets overall system metrics
    /// </summary>
    public SystemMetrics GetSystemMetrics()
    {
        var allEndpoints = GetAllEndpointMetrics();

        if (allEndpoints.Count == 0)
        {
            return new SystemMetrics();
        }

        var totalRequests = allEndpoints.Sum(m => m.RequestCount);
        var totalSuccess = allEndpoints.Sum(m => m.SuccessCount);
        var totalErrors = allEndpoints.Sum(m => m.ErrorCount);

        var avgResponseTimes = allEndpoints.Select(m => m.AverageResponseTimeMs).ToList();

        return new SystemMetrics
        {
            TotalRequests = totalRequests,
            TotalSuccessful = totalSuccess,
            TotalErrors = totalErrors,
            OverallSuccessRate = totalRequests > 0 ? (100.0 * totalSuccess / totalRequests) : 0,
            AverageResponseTimeMs = (long)avgResponseTimes.Average(),
            MinResponseTimeMs = avgResponseTimes.Min(),
            MaxResponseTimeMs = avgResponseTimes.Max(),
            MonitoredEndpoints = _metrics.Count,
            SamplePeriod = CalculateSamplePeriod(allEndpoints)
        };
    }

    /// <summary>
    /// Gets metrics for a time window
    /// </summary>
    public EndpointMetrics? GetEndpointMetricsForPeriod(string endpoint, TimeSpan period)
    {
        if (!_metrics.TryGetValue(endpoint, out var list))
            return null;

        var cutoffTime = DateTime.UtcNow - period;

        lock (_lock)
        {
            var recentMetrics = list.Where(m => m.Timestamp >= cutoffTime).ToList();

            if (recentMetrics.Count == 0)
                return null;

            var statusCodeCounts = recentMetrics
                .GroupBy(m => m.StatusCode)
                .ToDictionary(g => g.Key, g => g.Count());

            var successCount = recentMetrics.Count(m => m.IsSuccess);
            var errorCount = recentMetrics.Count(m => !m.IsSuccess);

            var times = recentMetrics.Select(m => m.ElapsedMilliseconds).OrderBy(t => t).ToList();

            return new EndpointMetrics
            {
                Endpoint = endpoint,
                RequestCount = recentMetrics.Count,
                SuccessCount = successCount,
                ErrorCount = errorCount,
                SuccessRate = recentMetrics.Count > 0 ? (100.0 * successCount / recentMetrics.Count) : 0,
                AverageResponseTimeMs = (long)times.Average(),
                MinResponseTimeMs = times.First(),
                MaxResponseTimeMs = times.Last(),
                StatusCodeDistribution = statusCodeCounts,
                FirstSeenAt = recentMetrics.First().Timestamp,
                LastSeenAt = recentMetrics.Last().Timestamp
            };
        }
    }

    /// <summary>
    /// Clears all metrics
    /// </summary>
    public void Clear()
    {
        _metrics.Clear();
    }

    private TimeSpan CalculateSamplePeriod(List<EndpointMetrics> metrics)
    {
        var earliestTime = metrics.Select(m => m.FirstSeenAt).Min();
        var latestTime = metrics.Select(m => m.LastSeenAt).Max();
        return latestTime - earliestTime;
    }
}

/// <summary>
/// Endpoint metrics
/// </summary>
public class EndpointMetrics
{
    public string Endpoint { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public double SuccessRate { get; set; }
    public long AverageResponseTimeMs { get; set; }
    public long MinResponseTimeMs { get; set; }
    public long MaxResponseTimeMs { get; set; }
    public long MedianResponseTimeMs { get; set; }
    public long P95ResponseTimeMs { get; set; }
    public long P99ResponseTimeMs { get; set; }
    public long AverageRequestSize { get; set; }
    public long AverageResponseSize { get; set; }
    public Dictionary<int, int> StatusCodeDistribution { get; set; } = new();
    public DateTime FirstSeenAt { get; set; }
    public DateTime LastSeenAt { get; set; }

    public override string ToString()
    {
        return $"{Endpoint}: {RequestCount} requests, {SuccessRate:F1}% success, " +
               $"Avg={AverageResponseTimeMs}ms, P95={P95ResponseTimeMs}ms";
    }
}

/// <summary>
/// System-wide metrics
/// </summary>
public class SystemMetrics
{
    public long TotalRequests { get; set; }
    public long TotalSuccessful { get; set; }
    public long TotalErrors { get; set; }
    public double OverallSuccessRate { get; set; }
    public long AverageResponseTimeMs { get; set; }
    public long MinResponseTimeMs { get; set; }
    public long MaxResponseTimeMs { get; set; }
    public int MonitoredEndpoints { get; set; }
    public TimeSpan SamplePeriod { get; set; }

    public override string ToString()
    {
        return $"System: {TotalRequests} total requests, {OverallSuccessRate:F1}% success, " +
               $"Avg={AverageResponseTimeMs}ms, Period={SamplePeriod.TotalMinutes:F1}min";
    }
}

/// <summary>
/// Individual request metric
/// </summary>
public class RequestMetric
{
    public string Endpoint { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public int RequestSize { get; set; }
    public int ResponseSize { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsSuccess { get; set; }
}
