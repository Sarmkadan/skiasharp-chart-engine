// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Metrics and statistics for chart rendering operations
/// </summary>
public class RenderMetrics
{
    public string? ChartId { get; set; }

    public DateTime RenderedAt { get; set; } = DateTime.UtcNow;

    public long RenderTimeMs { get; set; }

    public long ImageSizeBytes { get; set; }

    public int SeriesCount { get; set; }

    public int DataPointCount { get; set; }

    public ExportFormat ExportFormat { get; set; } = ExportFormat.PNG;

    public int CacheSizeAtRenderTime { get; set; }

    public bool WasCached { get; set; }

    public Dictionary<string, object>? AdditionalMetrics { get; set; }

    public double GetMegabytesPerSecond()
    {
        if (RenderTimeMs == 0)
            return 0;

        var megabytes = ImageSizeBytes / (1024.0 * 1024.0);
        var seconds = RenderTimeMs / 1000.0;
        return megabytes / seconds;
    }

    public double GetDataPointsPerSecond()
    {
        if (RenderTimeMs == 0)
            return 0;

        return (DataPointCount * 1000.0) / RenderTimeMs;
    }

    public override string ToString()
    {
        return $"RenderMetrics(ChartId={ChartId}, Time={RenderTimeMs}ms, Size={ImageSizeBytes} bytes, Points={DataPointCount})";
    }
}

/// <summary>
/// Metrics collector for tracking engine statistics
/// </summary>
public class MetricsCollector
{
    private readonly List<RenderMetrics> _metrics = new();
    private readonly object _lock = new();

    public void RecordMetrics(RenderMetrics metrics)
    {
        if (metrics == null)
            throw new ArgumentNullException(nameof(metrics));

        lock (_lock)
        {
            _metrics.Add(metrics);
        }
    }

    public List<RenderMetrics> GetAllMetrics()
    {
        lock (_lock)
        {
            return new List<RenderMetrics>(_metrics);
        }
    }

    public RenderMetrics? GetLastMetrics()
    {
        lock (_lock)
        {
            return _metrics.LastOrDefault();
        }
    }

    public double GetAverageRenderTimeMs()
    {
        lock (_lock)
        {
            if (_metrics.Count == 0)
                return 0;

            return _metrics.Average(m => m.RenderTimeMs);
        }
    }

    public double GetAverageImageSizeBytes()
    {
        lock (_lock)
        {
            if (_metrics.Count == 0)
                return 0;

            return _metrics.Average(m => m.ImageSizeBytes);
        }
    }

    public int GetCacheHitCount()
    {
        lock (_lock)
        {
            return _metrics.Count(m => m.WasCached);
        }
    }

    public double GetCacheHitPercentage()
    {
        lock (_lock)
        {
            if (_metrics.Count == 0)
                return 0;

            var cacheHits = _metrics.Count(m => m.WasCached);
            return (cacheHits / (double)_metrics.Count) * 100;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _metrics.Clear();
        }
    }

    public int GetMetricsCount()
    {
        lock (_lock)
        {
            return _metrics.Count;
        }
    }
}
