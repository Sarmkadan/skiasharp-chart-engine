// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Caching;

namespace SkiaSharpChartEngine.Workers;

/// <summary>
/// Background worker for maintaining cache health
/// Periodically cleans up expired entries and manages memory
/// </summary>
public class CacheCleanupWorker : IDisposable
{
    private readonly ILogger<CacheCleanupWorker> _logger;
    private readonly DistributedCacheService _cacheService;
    private readonly Timer _timer;
    private readonly TimeSpan _cleanupInterval;
    private int _totalCleanups;
    private long _totalEntriesRemoved;

    public CacheCleanupWorker(
        ILogger<CacheCleanupWorker> logger,
        DistributedCacheService cacheService,
        TimeSpan? cleanupInterval = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _cleanupInterval = cleanupInterval ?? TimeSpan.FromMinutes(10);

        // Start cleanup timer
        _timer = new Timer(ExecuteCleanup, null, _cleanupInterval, _cleanupInterval);

        _logger.LogInformation("Cache cleanup worker started with interval {Interval} minutes",
            _cleanupInterval.TotalMinutes);
    }

    /// <summary>
    /// Executes a cleanup cycle
    /// Removes expired entries and monitors cache health
    /// </summary>
    private void ExecuteCleanup(object? state)
    {
        try
        {
            var startStats = _cacheService.GetStatistics();
            var startTime = DateTime.UtcNow;

            _logger.LogInformation("Cache cleanup started - Current size: {Size}/{Max} bytes",
                startStats.SizeInBytes,
                startStats.MaxSizeInBytes);

            // Get metadata of all entries
            var metadata = _cacheService.GetMetadata();
            var now = DateTime.UtcNow;
            var entriesRemoved = 0;

            foreach (var entry in metadata)
            {
                // Remove entries that should have expired
                var timeToExpiration = entry.GetTimeToExpiration();
                if (timeToExpiration.HasValue && timeToExpiration.Value.TotalSeconds <= 0)
                {
                    _cacheService.Remove(entry.Key);
                    entriesRemoved++;
                }
            }

            // Check if we're over capacity
            var endStats = _cacheService.GetStatistics();
            if (endStats.UtilizationPercentage > 90)
            {
                _logger.LogWarning("Cache utilization is {Percentage:F1}%, may need more aggressive cleanup",
                    endStats.UtilizationPercentage);

                // Remove low-priority entries
                RemoveLowPriorityEntries(metadata);
            }

            var duration = DateTime.UtcNow - startTime;
            _totalCleanups++;
            _totalEntriesRemoved += entriesRemoved;

            var finalStats = _cacheService.GetStatistics();
            _logger.LogInformation(
                "Cache cleanup completed in {Duration}ms - Removed {RemovedCount} entries, " +
                "Size: {Size}/{Max} bytes ({Utilization:F1}%)",
                duration.TotalMilliseconds,
                entriesRemoved,
                finalStats.SizeInBytes,
                finalStats.MaxSizeInBytes,
                finalStats.UtilizationPercentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache cleanup");
        }
    }

    /// <summary>
    /// Removes low-priority entries to free up space
    /// </summary>
    private void RemoveLowPriorityEntries(List<CacheEntryMetadata> metadata)
    {
        var lowPriorityEntries = metadata
            .Where(e => e.Priority == CachePriority.Low)
            .OrderBy(e => e.LastAccessedAt)
            .Take((int)(metadata.Count * 0.1)) // Remove bottom 10% by priority
            .ToList();

        foreach (var entry in lowPriorityEntries)
        {
            _cacheService.Remove(entry.Key);
            _logger.LogDebug("Removed low-priority cache entry: {Key}", entry.Key);
        }
    }

    /// <summary>
    /// Gets cleanup statistics
    /// </summary>
    public CleanupStatistics GetStatistics()
    {
        var cacheStats = _cacheService.GetStatistics();

        return new CleanupStatistics
        {
            TotalCleanups = _totalCleanups,
            TotalEntriesRemoved = _totalEntriesRemoved,
            CurrentCacheSize = cacheStats.SizeInBytes,
            MaxCacheSize = cacheStats.MaxSizeInBytes,
            CurrentEntryCount = cacheStats.EntryCount,
            CacheUtilizationPercentage = cacheStats.UtilizationPercentage
        };
    }

    /// <summary>
    /// Manually trigger a cleanup cycle
    /// </summary>
    public void TriggerCleanup()
    {
        _logger.LogInformation("Manual cache cleanup triggered");
        ExecuteCleanup(null);
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _logger.LogInformation("Cache cleanup worker disposed - Total cleanups: {Total}, " +
            "Total entries removed: {Removed}",
            _totalCleanups,
            _totalEntriesRemoved);
    }
}

/// <summary>
/// Cleanup worker statistics
/// </summary>
public class CleanupStatistics
{
    public int TotalCleanups { get; set; }
    public long TotalEntriesRemoved { get; set; }
    public long CurrentCacheSize { get; set; }
    public long MaxCacheSize { get; set; }
    public int CurrentEntryCount { get; set; }
    public double CacheUtilizationPercentage { get; set; }

    public override string ToString()
    {
        return $"Cleanups: {TotalCleanups}, Removed: {TotalEntriesRemoved}, " +
               $"Size: {CurrentCacheSize}/{MaxCacheSize} ({CacheUtilizationPercentage:F1}%), " +
               $"Entries: {CurrentEntryCount}";
    }
}
