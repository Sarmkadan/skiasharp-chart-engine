// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Caching;

/// <summary>
/// In-memory cache specifically for rendered chart results.
/// Tracks cache hits/misses and automatically evicts stale entries.
/// </summary>
public class RenderResultCache : IDisposable
{
    private readonly Dictionary<string, CachedRenderResult> _cache;
    private readonly ILogger<RenderResultCache> _logger;
    private readonly long _maxCacheSizeBytes;
    private long _currentSizeBytes;
    private Timer _evictionTimer;
    private readonly object _lockObject = new object();

    public RenderResultCache(ILogger<RenderResultCache> logger, long maxCacheSizeBytes = 104_857_600)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxCacheSizeBytes = maxCacheSizeBytes; // 100 MB default
        _cache = new Dictionary<string, CachedRenderResult>();
        _currentSizeBytes = 0;
        _startEvictionTimer();
    }

    // Cache render result
    public void Cache(string cacheKey, RenderResult result, TimeSpan? ttl = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cacheKey) || result == null)
                return;

            lock (_lockObject)
            {
                var data = result.ImageData ?? Array.Empty<byte>();
                var size = data.Length;

                // Check if adding this would exceed cache size
                if (_currentSizeBytes + size > _maxCacheSizeBytes)
                {
                    _evictLRU();
                }

                _cache[cacheKey] = new CachedRenderResult
                {
                    CacheKey = cacheKey,
                    Result = result,
                    CachedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.Add(ttl ?? TimeSpan.FromHours(1)),
                    Size = size,
                    AccessCount = 0
                };

                _currentSizeBytes += size;
                _logger.LogDebug("Render result cached: {CacheKey}, Size: {Size}KB", cacheKey, size / 1024);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching render result");
        }
    }

    // Get cached result
    public RenderResult Get(string cacheKey)
    {
        try
        {
            lock (_lockObject)
            {
                if (!_cache.TryGetValue(cacheKey, out var cached))
                {
                    _logger.LogDebug("Cache miss: {CacheKey}", cacheKey);
                    return null;
                }

                // Check if expired
                if (DateTime.UtcNow > cached.ExpiresAt)
                {
                    _cache.Remove(cacheKey);
                    _currentSizeBytes -= cached.Size;
                    _logger.LogDebug("Cache expired: {CacheKey}", cacheKey);
                    return null;
                }

                cached.AccessCount++;
                cached.LastAccessedAt = DateTime.UtcNow;
                _logger.LogDebug("Cache hit: {CacheKey}, Access count: {AccessCount}", cacheKey, cached.AccessCount);

                return cached.Result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache");
            return null;
        }
    }

    // Remove specific cache entry
    public bool Remove(string cacheKey)
    {
        lock (_lockObject)
        {
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                _cache.Remove(cacheKey);
                _currentSizeBytes -= cached.Size;
                _logger.LogInformation("Cache entry removed: {CacheKey}", cacheKey);
                return true;
            }

            return false;
        }
    }

    // Clear all cache
    public void Clear()
    {
        lock (_lockObject)
        {
            var count = _cache.Count;
            _cache.Clear();
            _currentSizeBytes = 0;
            _logger.LogInformation("Cache cleared: {EntryCount} entries removed", count);
        }
    }

    // Get cache statistics
    public RenderCacheStatistics GetStatistics()
    {
        lock (_lockObject)
        {
            var totalHits = _cache.Values.Sum(c => c.AccessCount);
            var totalMisses = _cache.Values.Count(c => c.AccessCount == 0);

            return new RenderCacheStatistics
            {
                TotalEntries = _cache.Count,
                TotalSize = _currentSizeBytes,
                MaxSize = _maxCacheSizeBytes,
                TotalHits = totalHits,
                EvictionCount = 0,
                OldestEntry = _cache.Values.MinBy(c => c.CachedAt)?.CachedAt,
                NewestEntry = _cache.Values.MaxBy(c => c.CachedAt)?.CachedAt
            };
        }
    }

    private void _evictLRU()
    {
        try
        {
            if (_cache.Count == 0)
                return;

            // Remove least recently used entry
            var lru = _cache.Values.OrderBy(c => c.LastAccessedAt ?? c.CachedAt).FirstOrDefault();
            if (lru != null)
            {
                _cache.Remove(lru.CacheKey);
                _currentSizeBytes -= lru.Size;
                _logger.LogDebug("LRU eviction: {CacheKey}", lru.CacheKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LRU eviction");
        }
    }

    private void _startEvictionTimer()
    {
        _evictionTimer = new Timer(_ => _evictExpiredEntries(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    private void _evictExpiredEntries()
    {
        try
        {
            lock (_lockObject)
            {
                var expired = _cache.Where(kvp => DateTime.UtcNow > kvp.Value.ExpiresAt).ToList();

                foreach (var kvp in expired)
                {
                    _cache.Remove(kvp.Key);
                    _currentSizeBytes -= kvp.Value.Size;
                }

                if (expired.Count > 0)
                {
                    _logger.LogDebug("Evicted {Count} expired cache entries", expired.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during expiration eviction");
        }
    }

    public void Dispose()
    {
        _evictionTimer?.Dispose();
    }
}

public class CachedRenderResult
{
    public string CacheKey { get; set; }
    public RenderResult Result { get; set; }
    public DateTime CachedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public long Size { get; set; }
    public long AccessCount { get; set; }
}

public class RenderCacheStatistics
{
    public int TotalEntries { get; set; }
    public long TotalSize { get; set; }
    public long MaxSize { get; set; }
    public long TotalHits { get; set; }
    public int EvictionCount { get; set; }
    public DateTime? OldestEntry { get; set; }
    public DateTime? NewestEntry { get; set; }
}
