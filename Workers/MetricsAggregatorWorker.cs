// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Workers;

/// <summary>
/// Background worker for aggregating performance metrics at regular intervals.
/// Collects statistics, generates reports, and triggers alerts for anomalies.
/// </summary>
public class MetricsAggregatorWorker
{
    private readonly ILogger<MetricsAggregatorWorker> _logger;
    private readonly Dictionary<string, MetricData> _metrics;
    private Timer _timer;
    private readonly TimeSpan _aggregationInterval;
    private bool _isRunning;

    public event EventHandler<MetricsAggregatedEventArgs> MetricsAggregated;

    public MetricsAggregatorWorker(ILogger<MetricsAggregatorWorker> logger, TimeSpan? aggregationInterval = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = new Dictionary<string, MetricData>();
        _aggregationInterval = aggregationInterval ?? TimeSpan.FromMinutes(5);
        _isRunning = false;
    }

    // Start the metrics aggregator worker
    public void Start()
    {
        if (_isRunning)
        {
            _logger.LogWarning("Metrics aggregator is already running");
            return;
        }

        _isRunning = true;
        _timer = new Timer(_ => _aggregateMetrics(), null, _aggregationInterval, _aggregationInterval);
        _logger.LogInformation("Metrics aggregator worker started");
    }

    // Stop the worker
    public void Stop()
    {
        if (!_isRunning)
            return;

        _timer?.Dispose();
        _isRunning = false;
        _logger.LogInformation("Metrics aggregator worker stopped");
    }

    // Record metric value
    public void RecordMetric(string metricName, double value, string tags = "")
    {
        try
        {
            if (!_metrics.TryGetValue(metricName, out var metricData))
            {
                metricData = new MetricData { Name = metricName };
                _metrics[metricName] = metricData;
            }

            metricData.Values.Add(value);
            metricData.LastUpdated = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(tags))
            {
                metricData.Tags = tags;
            }

            _logger.LogDebug("Metric recorded: {MetricName}={Value}", metricName, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording metric");
        }
    }

    // Get metric statistics
    public MetricStatistics GetMetricStatistics(string metricName)
    {
        if (!_metrics.TryGetValue(metricName, out var metricData))
            return null;

        if (metricData.Values.Count == 0)
            return null;

        var values = metricData.Values.OrderBy(v => v).ToList();
        var sum = values.Sum();
        var avg = sum / values.Count;

        // Calculate standard deviation
        var variance = values.Average(v => Math.Pow(v - avg, 2));
        var stdDev = Math.Sqrt(variance);

        return new MetricStatistics
        {
            Name = metricName,
            Count = values.Count,
            Sum = sum,
            Average = avg,
            Min = values.First(),
            Max = values.Last(),
            Median = values[values.Count / 2],
            StdDev = stdDev,
            Tags = metricData.Tags,
            LastUpdated = metricData.LastUpdated
        };
    }

    // Get all metrics
    public Dictionary<string, MetricStatistics> GetAllMetrics()
    {
        var result = new Dictionary<string, MetricStatistics>();

        foreach (var kvp in _metrics)
        {
            var stats = GetMetricStatistics(kvp.Key);
            if (stats != null)
                result[kvp.Key] = stats;
        }

        return result;
    }

    // Clear metrics
    public void ClearMetrics()
    {
        _metrics.Clear();
        _logger.LogInformation("All metrics cleared");
    }

    // Get metrics count
    public int GetMetricsCount() => _metrics.Count;

    private void _aggregateMetrics()
    {
        try
        {
            _logger.LogDebug("Aggregating metrics from {Count} metric groups", _metrics.Count);

            var allStats = GetAllMetrics();

            // Check for anomalies (values > 2 sigma)
            var anomalies = new List<AnomalyAlert>();
            foreach (var stat in allStats.Values)
            {
                if (stat.StdDev > 0)
                {
                    var threshold = stat.Average + (2 * stat.StdDev);
                    if (stat.Max > threshold)
                    {
                        anomalies.Add(new AnomalyAlert
                        {
                            MetricName = stat.Name,
                            Value = stat.Max,
                            Threshold = threshold,
                            DetectedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            if (anomalies.Count > 0)
            {
                _logger.LogWarning("Found {AnomalyCount} metric anomalies", anomalies.Count);
            }

            // Raise event
            MetricsAggregated?.Invoke(this, new MetricsAggregatedEventArgs
            {
                Statistics = allStats,
                Anomalies = anomalies,
                AggregatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating metrics");
        }
    }
}

public class MetricData
{
    public string Name { get; set; }
    public List<double> Values { get; set; } = new List<double>();
    public string Tags { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class MetricStatistics
{
    public string Name { get; set; }
    public int Count { get; set; }
    public double Sum { get; set; }
    public double Average { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public double Median { get; set; }
    public double StdDev { get; set; }
    public string Tags { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class AnomalyAlert
{
    public string MetricName { get; set; }
    public double Value { get; set; }
    public double Threshold { get; set; }
    public DateTime DetectedAt { get; set; }
}

public class MetricsAggregatedEventArgs : EventArgs
{
    public Dictionary<string, MetricStatistics> Statistics { get; set; }
    public List<AnomalyAlert> Anomalies { get; set; }
    public DateTime AggregatedAt { get; set; }
}
