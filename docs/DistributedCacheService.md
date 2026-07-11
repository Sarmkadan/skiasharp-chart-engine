# DistributedCacheService

The `DistributedCacheService` provides a centralized mechanism for caching data across distributed environments within the `skiasharp-chart-engine`. It facilitates temporary data storage to optimize performance by reducing redundant calculations or data retrieval operations, supporting expiration policies, priority management, and basic cache entry metadata tracking.

## API

### Methods

*   **`DistributedCacheService()`**
    Initializes a new instance of the `DistributedCacheService`.

*   **`void Set<T>(string key, T value, TimeSpan? slidingExpiration = null, CachePriority priority = CachePriority.Normal)`**
    Stores an item in the cache. 
    *   **Parameters:** `key` (string), `value` (T), `slidingExpiration` (optional), `priority` (optional).
    *   **Throws:** `ArgumentNullException` if the key is null.

*   **`bool TryGet<T>(string key, out T value)`**
    Attempts to retrieve a cached item. 
    *   **Parameters:** `key` (string), `value` (output).
    *   **Returns:** `true` if found and not expired; otherwise, `false`.

*   **`void Remove(string key)`**
    Removes a specific entry from the cache.

*   **`int RemovePattern(string pattern)`**
    Removes all cache entries matching the specified pattern.
    *   **Returns:** The number of entries removed.

*   **`void Clear()`**
    Removes all entries from the cache.

*   **`CacheStatistics GetStatistics()`**
    Retrieves current cache utilization and performance statistics.

*   **`List<CacheEntryMetadata> GetMetadata()`**
    Retrieves metadata for all entries currently stored in the cache.

*   **`void Dispose()`**
    Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.

### Properties

*   **`string Key`**: Gets the unique identifier for a cache entry.
*   **`string Value`**: Gets the string representation of the cached value.
*   **`string ValueType`**: Gets the type name of the cached object.
*   **`DateTime CreatedAt`**: Gets the timestamp when the entry was created.
*   **`DateTime LastAccessedAt`**: Gets the timestamp of the last access to the entry.
*   **`DateTime? ExpiresAt`**: Gets the timestamp when the entry expires.
*   **`TimeSpan? SlidingExpiration`**: Gets the sliding expiration duration, if applicable.
*   **`CachePriority Priority`**: Gets the priority level of the cache entry.
*   **`long? SizeInBytes`**: Gets the approximate size of the entry in bytes.
*   **`bool IsExpired`**: Indicates whether the entry has passed its expiration time.
*   **`int EntryCount`**: Gets the total number of entries currently in the cache.

## Usage

### Example 1: Basic Cache Operations
```csharp
var cacheService = new DistributedCacheService();

// Storing data
cacheService.Set("chart_data_123", complexDataObj);

// Retrieving data
if (cacheService.TryGet<ChartData>("chart_data_123", out var data))
{
    // Use cached data
}
```

### Example 2: Managing Cache Entries
```csharp
var cacheService = new DistributedCacheService();

// Removing entries matching a pattern
int removedCount = cacheService.RemovePattern("chart_data_*");

// Monitoring cache state
var stats = cacheService.GetStatistics();
Console.WriteLine($"Current entries: {cacheService.EntryCount}");
```

## Notes

*   **Thread Safety**: The `DistributedCacheService` is designed to be thread-safe, allowing concurrent read and write operations from multiple threads without external synchronization.
*   **Resource Management**: Because `DistributedCacheService` implements `IDisposable`, it is crucial to dispose of the service instance when it is no longer required to ensure that underlying network connections or memory resources are released properly.
*   **Expiration**: `IsExpired` is evaluated lazily; an entry might exist in the cache even if `IsExpired` returns `true` until it is evicted or explicitly removed.
*   **Pattern Matching**: The `RemovePattern` method supports basic wildcard matching, though exact implementation details depend on the underlying storage provider.
