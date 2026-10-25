# IRenderCacheService

`IRenderCacheService` defines the contract for caching rendered chart images as byte arrays, keyed by a string cache key, in the `skiasharp-chart-engine`. It covers reading an entry, storing an entry, removing a single entry or all entries, counting entries, checking whether a key exists, and listing all keys.

The engine's default implementation is `RenderCacheService`, an in-memory cache with a fixed entry limit. `AddSkiaSharpChartEngine` registers it as a singleton in `Configuration/ServiceCollectionExtensions.cs`, using `ChartEngineOptions.CacheSize` as the limit (default `ChartConstants.CacheSize`, which is 100). `ChartRenderingService` depends on this interface: it calls `Get` before rendering and `Set` after rendering. To use a different cache, such as a distributed one, implement this interface and register your implementation in place of `RenderCacheService`.

**Namespace:** `SkiaSharpChartEngine.Services`
**Declaration:** `public interface IRenderCacheService`
**Source:** `Services/IRenderCacheService.cs`

## API

### `byte[]? Get(string cacheKey)`
Returns the cached image data for the key, or `null` if there is no entry.
- **Parameters:** `cacheKey` - The key the image was stored under.
- **Returns:** The stored image bytes, or `null` on a miss.

### `void Set(string cacheKey, byte[] imageData)`
Stores image data under the key. If the key already exists, its entry is replaced.
- **Parameters:**
  - `cacheKey` - The key to store the image under.
  - `imageData` - The rendered image bytes.

### `void Remove(string cacheKey)`
Removes the entry for the key. If no entry exists, the call does nothing.
- **Parameters:** `cacheKey` - The key of the entry to remove.

### `void Clear()`
Removes all entries.

### `int GetCacheSize()`
Returns the number of entries currently stored. This counts entries, not bytes.
- **Returns:** The entry count.

### `bool Contains(string cacheKey)`
Checks whether an entry exists for the key without returning its data.
- **Parameters:** `cacheKey` - The key to check.
- **Returns:** `true` if an entry exists; otherwise `false`.

### `IEnumerable<string> GetAllKeys()`
Returns the keys of all stored entries.
- **Returns:** A list of cache keys. The list is empty if nothing is cached.

## Behavior of `RenderCacheService`

The following describes the default implementation. Other implementations of the interface may behave differently.

- **Null and blank keys.** `Get`, `Remove`, and `Contains` return `null`, do nothing, and return `false`, respectively, for a `null`, empty, or whitespace key. `Set` ignores such keys and also ignores a `null` `imageData`. None of these methods throws for bad keys.
- **Eviction.** The cache holds at most `CacheSize` entries. When `Set` is called on a full cache, the entry with the lowest `AccessCount` is evicted before the new entry is stored. Ties are broken by the oldest `CreatedAt`. `Get` increments `AccessCount` on each hit.
- **Replacement.** Calling `Set` with an existing key replaces the entry and resets its `AccessCount` to zero. The capacity check runs before the key lookup, so if the cache is full, replacing an existing key still evicts another entry first.
- **Thread safety.** All operations are guarded by a single lock, so the cache can be shared across concurrent render requests.
- **Returned data.** `Get` returns the stored array itself, not a copy. Callers that modify the returned bytes change the cached image.
- **Logging.** Hits, misses, stores, removals, clears, and evictions are logged through `ILogger<RenderCacheService>` at debug or information level.

## Usage

```csharp
// Resolve the cache from the service provider
var cache = serviceProvider.GetRequiredService<IRenderCacheService>();

// Check the cache before rendering, and store the result afterwards
var key = "line-chart-1";
var image = cache.Get(key);
if (image is null)
{
    image = RenderChart();   // your rendering call, returns byte[]
    cache.Set(key, image);
}

// Invalidate a single entry, or everything
cache.Remove(key);
cache.Clear();
```

