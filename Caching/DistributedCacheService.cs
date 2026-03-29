// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Caching;

/// <summary>
/// In-memory distributed cache service for chart renders
/// Provides thread-safe caching with expiration and eviction policies
/// </summary>
public class DistributedCacheService : IDisposable
{
    private readonly ILogger<DistributedCacheService> _logger;
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly ConcurrentDictionary<string, CacheEntryMetadata> _metadata;
    private readonly Timer _cleanupTimer;
    private readonly long _maxSizeBytes;
    private long _currentSizeBytes;
    private readonly object _sizeLock = new();

    public DistributedCacheService(ILogger<DistributedCacheService> logger, long maxSizeBytes = 100_000_000)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = new ConcurrentDictionary<string, CacheEntry>();
        _metadata = new ConcurrentDictionary<string, CacheEntryMetadata>();
        _maxSizeBytes = maxSizeBytes;
        _currentSizeBytes = 0;

        // Cleanup expired entries every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Sets a cache entry with the specified policy
    /// </summary>
    public void Set<T>(string key, T value, CachePolicy? policy = null) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be empty", nameof(key));

        try
        {
            policy ??= CachePolicy.Default;

            if (!policy.IsValid())
                throw new ArgumentException("Invalid cache policy", nameof(policy));

            var serialized = JsonSerializer.Serialize(value);
            var sizeInBytes = System.Text.Encoding.UTF8.GetByteCount(serialized);

            var entry = new CacheEntry
            {
                Key = key,
                Value = serialized,
                ValueType = typeof(T).FullName ?? "unknown",
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                ExpiresAt = policy.GetAbsoluteExpiration(),
                SlidingExpiration = policy.SlidingExpiration,
                Priority = policy.Priority,
                SizeInBytes = sizeInBytes
            };

            // Check if we need to evict entries
            EnsureCapacity(sizeInBytes);

            _cache.AddOrUpdate(key, entry, (k, old) =>
            {
                // Reduce size by old entry
                lock (_sizeLock)
                {
                    _currentSizeBytes -= old.SizeInBytes ?? 0;
                }
                return entry;
            });

            lock (_sizeLock)
            {
                _currentSizeBytes += sizeInBytes;
            }

            UpdateMetadata(key, entry);

            _logger.LogDebug("Cache SET: {Key} ({Size} bytes)", key, sizeInBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache entry {Key}", key);
            throw;
        }
    }

    /// <summary>
    /// Gets a cache entry if it exists and hasn't expired
    /// </summary>
    public bool TryGet<T>(string key, out T? value) where T : class
    {
        value = null;

        if (string.IsNullOrEmpty(key))
            return false;

        try
        {
            if (!_cache.TryGetValue(key, out var entry))
                return false;

            if (entry.IsExpired())
            {
                Remove(key);
                return false;
            }

            // Update sliding expiration
            if (entry.SlidingExpiration.HasValue)
            {
                entry.LastAccessedAt = DateTime.UtcNow;
                entry.ExpiresAt = DateTime.UtcNow.Add(entry.SlidingExpiration.Value);
            }

            // Update metadata
            if (_metadata.TryGetValue(key, out var meta))
            {
                meta.LastAccessedAt = DateTime.UtcNow;
                meta.AccessCount++;
            }

            value = JsonSerializer.Deserialize<T>(entry.Value);
            _logger.LogDebug("Cache HIT: {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache entry {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Removes a cache entry
    /// </summary>
    public void Remove(string key)
    {
        if (string.IsNullOrEmpty(key))
            return;

        try
        {
            if (_cache.TryRemove(key, out var entry))
            {
                lock (_sizeLock)
                {
                    _currentSizeBytes -= entry.SizeInBytes ?? 0;
                }

                _metadata.TryRemove(key, out _);
                _logger.LogDebug("Cache REMOVE: {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entry {Key}", key);
        }
    }

    /// <summary>
    /// Removes all entries matching a pattern
    /// </summary>
    public int RemovePattern(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return 0;

        var keysToRemove = _cache.Keys
            .Where(k => k.StartsWith(pattern.Replace("*", "")))
            .ToList();

        foreach (var key in keysToRemove)
        {
            Remove(key);
        }

        return keysToRemove.Count;
    }

    /// <summary>
    /// Clears all cache entries
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        _metadata.Clear();
        lock (_sizeLock)
        {
            _currentSizeBytes = 0;
        }
        _logger.LogInformation("Cache cleared");
    }

    /// <summary>
    /// Gets cache statistics
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        return new CacheStatistics
        {
            EntryCount = _cache.Count,
            SizeInBytes = _currentSizeBytes,
            MaxSizeInBytes = _maxSizeBytes,
            UtilizationPercentage = _maxSizeBytes > 0 ? (_currentSizeBytes * 100.0 / _maxSizeBytes) : 0
        };
    }

    /// <summary>
    /// Gets metadata for all entries
    /// </summary>
    public List<CacheEntryMetadata> GetMetadata()
    {
        return _metadata.Values.ToList();
    }

    private void EnsureCapacity(long requiredBytes)
    {
        lock (_sizeLock)
        {
            if (_currentSizeBytes + requiredBytes <= _maxSizeBytes)
                return;

            // Evict low-priority entries
            var entriesToEvict = _cache.Values
                .OrderBy(e => e.Priority)
                .ThenBy(e => e.LastAccessedAt)
                .ToList();

            long freedBytes = 0;
            foreach (var entry in entriesToEvict)
            {
                if (_currentSizeBytes + requiredBytes <= _maxSizeBytes)
                    break;

                _cache.TryRemove(entry.Key, out _);
                _metadata.TryRemove(entry.Key, out _);
                freedBytes += entry.SizeInBytes ?? 0;
                _currentSizeBytes -= entry.SizeInBytes ?? 0;

                _logger.LogDebug("Evicted cache entry {Key} to free space", entry.Key);
            }
        }
    }

    private void CleanupExpiredEntries(object? state)
    {
        try
        {
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired())
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                Remove(key);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogDebug("Cleaned up {Count} expired cache entries", expiredKeys.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache cleanup");
        }
    }

    private void UpdateMetadata(string key, CacheEntry entry)
    {
        var metadata = new CacheEntryMetadata
        {
            Key = key,
            CreatedAt = entry.CreatedAt,
            LastAccessedAt = entry.LastAccessedAt,
            Priority = entry.Priority,
            ExpiresAt = entry.ExpiresAt,
            SizeInBytes = entry.SizeInBytes
        };

        _metadata.AddOrUpdate(key, metadata, (k, old) => metadata);
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

internal class CacheEntry
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public CachePriority Priority { get; set; }
    public long? SizeInBytes { get; set; }

    public bool IsExpired() => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}

/// <summary>
/// Cache statistics
/// </summary>
public class CacheStatistics
{
    public int EntryCount { get; set; }
    public long SizeInBytes { get; set; }
    public long MaxSizeInBytes { get; set; }
    public double UtilizationPercentage { get; set; }
}
