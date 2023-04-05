# RenderResultCache

The `RenderResultCache` provides an efficient caching mechanism for rendering results within the `skiasharp-chart-engine`. It is designed to store and retrieve compute-intensive render outputs, reducing redundant processing for static or slowly-evolving visual components.

## API

### Methods

- `RenderResultCache()`: Initializes a new instance of the `RenderResultCache`.
- `void Cache(...)`: Stores a `RenderResult` associated with a specific key.
- `RenderResult Get(...)`: Retrieves a cached `RenderResult` associated with the specified key.
- `bool Remove(...)`: Removes the `RenderResult` associated with the specified key from the cache. Returns `true` if the item was found and removed; otherwise, `false`.
- `void Clear()`: Removes all entries from the cache.
- `RenderCacheStatistics GetStatistics()`: Returns an object containing performance and usage metrics for the cache.
- `void Dispose()`: Performs cleanup of cached resources and internal structures.

### Properties

- `CacheKey`: Gets or sets the unique identifier for the current cache entry.
- `Result`: Gets the `RenderResult` associated with this cache instance.
- `CachedAt`: Gets the `DateTime` when the item was added to the cache.
- `ExpiresAt`: Gets the `DateTime` when the cached item is scheduled to expire.
- `LastAccessedAt`: Gets the `DateTime?` indicating when the item was last accessed, or `null` if it has not been accessed.
- `Size`: Gets the size of the current cached item.
- `AccessCount`: Gets the total number of times the current cached item has been accessed.
- `TotalEntries`: Gets the total number of entries currently stored in the cache.
- `TotalSize`: Gets the aggregate size of all cached entries.
- `MaxSize`: Gets or sets the maximum allowed size for the cache.
- `TotalHits`: Gets the total number of cache hits since the cache was initialized or last cleared.
- `EvictionCount`: Gets the total number of items evicted from the cache due to size limits or expiration.
- `OldestEntry`: Gets the `DateTime?` representing the creation timestamp of the oldest entry in the cache.

## Usage

```csharp
using SkiaSharp;
using SkiasharpChartEngine;

// Example 1: Basic caching and retrieval
var cache = new RenderResultCache();
string chartKey = "sales_chart_2026_Q2";

// Cache a rendered result
cache.Cache(chartKey, myRenderResult);

// Retrieve the result
RenderResult cachedResult = cache.Get(chartKey);
if (cachedResult != null)
{
    // Use the cached result
}
```

```csharp
// Example 2: Monitoring cache statistics
var stats = cache.GetStatistics();
Console.WriteLine($"Total Hits: {stats.TotalHits}");
Console.WriteLine($"Total Entries: {stats.TotalEntries}");

if (cache.TotalSize > cache.MaxSize * 0.9)
{
    // Optionally clear or optimize if nearing capacity
    cache.Clear();
}
```

## Notes

- **Thread-Safety**: This implementation is designed for thread-safe access in high-performance rendering scenarios. Multiple threads can concurrently retrieve cached items.
- **Resource Management**: As `RenderResultCache` implements `IDisposable`, it should be disposed of when no longer needed to ensure internal SkiaSharp resources are correctly released.
- **Eviction Policy**: The cache automatically evicts entries based on internal heuristics, such as expiration times (`ExpiresAt`) and memory pressure (governed by `MaxSize`).
- **Memory Overhead**: While `RenderResultCache` significantly improves rendering performance, setting an appropriate `MaxSize` is critical to prevent excessive memory consumption.
