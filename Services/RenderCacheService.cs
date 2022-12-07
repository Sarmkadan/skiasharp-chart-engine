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

        if (_cache.TryGetValue(cacheKey, out var entry))
        {
            entry.AccessCount++;
            _logger.LogDebug($"Cache hit for key: {cacheKey}");
            return entry.ImageData;
        }

        _logger.LogDebug($"Cache miss for key: {cacheKey}");
        return null;
    }

    public void Set(string cacheKey, byte[] imageData)
    {
        if (string.IsNullOrWhiteSpace(cacheKey) || imageData == null)
            return;

        if (_cache.Count >= _maxCacheSize)
            EvictLeastUsedEntry();

        _cache[cacheKey] = new CacheEntry
        {
            ImageData = imageData,
            CreatedAt = DateTime.UtcNow,
            AccessCount = 0
        };

        _logger.LogInformation($"Cached render result: {cacheKey} ({imageData.Length} bytes)");
    }

    public void Remove(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
            return;

        if (_cache.Remove(cacheKey))
        {
            _logger.LogInformation($"Removed cache entry: {cacheKey}");
        }
    }

    public void Clear()
    {
        _cache.Clear();
        _logger.LogInformation("Cache cleared");
    }

    public int GetCacheSize() => _cache.Count;

    public bool Contains(string cacheKey) => !string.IsNullOrWhiteSpace(cacheKey) && _cache.ContainsKey(cacheKey);

    public IEnumerable<string> GetAllKeys() => _cache.Keys.ToList();

    private void EvictLeastUsedEntry()
    {
        if (_cache.Count == 0)
            return;

        var leastUsedKey = _cache
            .OrderBy(kvp => kvp.Value.AccessCount)
            .ThenBy(kvp => kvp.Value.CreatedAt)
            .First()
            .Key;

        _cache.Remove(leastUsedKey);
        _logger.LogInformation($"Evicted LRU cache entry: {leastUsedKey}");
    }
}
