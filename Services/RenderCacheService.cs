// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Constants;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// In-memory cache service for rendered chart images
/// </summary>
public class RenderCacheService : IRenderCacheService
{
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly object _lock = new();
    private readonly ILogger<RenderCacheService> _logger;
    private readonly int _maxCacheSize;

    private class CacheEntry
    {
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public DateTime CreatedAt { get; set; }
        public int AccessCount { get; set; }
    }

    public RenderCacheService(ILogger<RenderCacheService> logger, int? maxCacheSize = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxCacheSize = maxCacheSize ?? ChartConstants.CacheSize;
    }

    public byte[]? Get(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
            return null;

        lock (_lock)
        {
            if (_cache.TryGetValue(cacheKey, out var entry))
            {
                entry.AccessCount++;
                _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                return entry.ImageData;
            }

            _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
            return null;
        }
    }

    public void Set(string cacheKey, byte[] imageData)
    {
        if (string.IsNullOrWhiteSpace(cacheKey) || imageData == null)
            return;

        lock (_lock)
        {
            if (_cache.Count >= _maxCacheSize)
                EvictLeastUsedEntry();

            _cache[cacheKey] = new CacheEntry
            {
                ImageData = imageData,
                CreatedAt = DateTime.UtcNow,
                AccessCount = 0
            };

            _logger.LogInformation("Cached render result: {CacheKey} ({ImageDataLength} bytes)", cacheKey, imageData.Length);
        }
    }

    public void Remove(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
            return;

        lock (_lock)
        {
            if (_cache.Remove(cacheKey))
            {
                _logger.LogInformation("Removed cache entry: {CacheKey}", cacheKey);
            }
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _cache.Clear();
            _logger.LogInformation("Cache cleared");
        }
    }

    public int GetCacheSize()
    {
        lock (_lock)
        {
            return _cache.Count;
        }
    }

    public bool Contains(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
            return false;

        lock (_lock)
        {
            return _cache.ContainsKey(cacheKey);
        }
    }

    public IEnumerable<string> GetAllKeys()
    {
        lock (_lock)
        {
            return _cache.Keys.ToList();
        }
    }

    public override string ToString()
    {
        lock (_lock)
        {
            if (_cache.Count == 0)
                return "RenderCacheService { ImageData = null, CreatedAt = 0001-01-01 00:00:00Z, AccessCount = 0 }";
            var entry = _cache.First().Value;
            return $"RenderCacheService {{ ImageData = {entry.ImageData.Length} bytes, CreatedAt = {entry.CreatedAt}, AccessCount = {entry.AccessCount} }}";
        }
    }

    private void EvictLeastUsedEntry()
    {
        lock (_lock)
        {
            if (_cache.Count == 0)
                return;

            var leastUsedKey = _cache
                .OrderBy(kvp => kvp.Value.AccessCount)
                .ThenBy(kvp => kvp.Value.CreatedAt)
                .First()
                .Key;

            _cache.Remove(leastUsedKey);
            _logger.LogInformation("Evicted LRU cache entry: {CacheKey}", leastUsedKey);
        }
    }
}
