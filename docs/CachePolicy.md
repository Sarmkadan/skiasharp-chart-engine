# CachePolicy

The `CachePolicy` class defines the configuration and lifecycle metadata for items stored within the `skiasharp-chart-engine` caching subsystems. It encapsulates expiration logic, eviction priorities, and usage statistics, providing a unified mechanism to manage the retention and invalidation of cached chart data.

## API

### Properties

*   **`AbsoluteExpirationRelativeToNow`** (`TimeSpan?`): Gets or sets the absolute expiration time relative to the moment the item is added to the cache.
*   **`AccessCount`** (`int`): Gets the total number of times this cache item has been accessed.
*   **`CreatedAt`** (`DateTime`): Gets the timestamp indicating when this policy instance was created.
*   **`ExpiresAt`** (`DateTime?`): Gets the absolute timestamp when the cache item will expire, if defined.
*   **`GetAbsoluteExpiration`** (`DateTime?`): Gets the absolute expiration date and time, if set.
*   **`GetAge`** (`TimeSpan`): Calculates and returns the duration since the item was created.
*   **`GetTimeToExpiration`** (`TimeSpan?`): Calculates the remaining duration until the item expires, or null if no expiration is set.
*   **`IsExpired`** (`bool`): Indicates whether the item has passed its expiration threshold based on current time.
*   **`IsValid`** (`bool`): Determines if the policy currently satisfies all conditions to remain in the cache.
*   **`Key`** (`string`): Gets the unique identifier for the associated cache entry.
*   **`LastAccessedAt`** (`DateTime`): Gets the timestamp of the most recent access to this cache entry.
*   **`PostEvictionCallbacks`** (`Action<string>?`): An optional delegate invoked when the item is removed from the cache.
*   **`Priority`** (`CachePriority`): Gets or sets the eviction priority, determining how aggressively the item should be removed under memory pressure.
*   **`SizeInBytes`** (`long?`): Gets or sets the estimated size of the cached item in bytes, used for capacity management.
*   **`SlidingExpiration`** (`TimeSpan?`): Gets or sets the duration after which the item expires if it has not been accessed.

### Methods

*   **`static CachePolicy Create(...)`**: Creates a new instance of `CachePolicy` with default settings or specified configuration parameters.
*   **`static CachePolicy ExpiresAfterHours(int hours)`**: A factory method that returns a `CachePolicy` configured with an absolute expiration of the specified number of hours.
*   **`static CachePolicy ExpiresAfterMinutes(int minutes)`**: A factory method that returns a `CachePolicy` configured with an absolute expiration of the specified number of minutes.

## Usage

### Simple Absolute Expiration

```csharp
// Configure an item to expire 30 minutes after being added to the cache
var policy = CachePolicy.ExpiresAfterMinutes(30);

cacheService.Set("chart_data_001", data, policy);
```

### Complex Policy Configuration

```csharp
// Configure a high-priority item with sliding expiration and post-eviction logging
var policy = CachePolicy.Create(
    priority: CachePriority.High,
    slidingExpiration: TimeSpan.FromMinutes(5),
    postEvictionCallbacks: (key) => Console.WriteLine($"Cache item {key} was evicted.")
);

cacheService.Set("complex_chart_state", state, policy);
```

## Notes

*   **Thread Safety**: While `CachePolicy` instances hold state (like `AccessCount` and `LastAccessedAt`), they are designed to be used in conjunction with thread-safe cache providers. Concurrent updates to `AccessCount` or `LastAccessedAt` from multiple threads on the same `CachePolicy` instance may not be atomic without external synchronization.
*   **Expiration Logic**: When both `AbsoluteExpirationRelativeToNow` and `SlidingExpiration` are set, the item will be evicted whichever threshold is reached first.
*   **Memory Pressure**: The `Priority` setting is utilized by the cache engine when the system approaches memory limits. Items with `CachePriority.Low` will be evicted before items with `CachePriority.High`.
