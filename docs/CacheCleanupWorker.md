# CacheCleanupWorker

The `CacheCleanupWorker` is responsible for managing the memory and capacity lifecycle of assets stored within the `skiasharp-chart-engine`. It monitors current cache usage against defined thresholds, maintains performance by enforcing storage limits, and provides telemetry data to facilitate proactive cache management and prevent memory overflow during rendering operations.

## API

*   **`CacheCleanupWorker()`**
    Initializes a new instance of the `CacheCleanupWorker` class.

*   **`CleanupStatistics GetStatistics()`**
    Returns a `CleanupStatistics` object containing detailed historical and current metrics related to cache eviction and cleanup operations.

*   **`void TriggerCleanup()`**
    Manually initiates an immediate cache cleanup operation, regardless of the current automatic threshold settings.

*   **`void Dispose()`**
    Releases all resources, including background timers or monitoring handles, utilized by the worker.

*   **`int TotalCleanups`**
    Gets the total number of cleanup cycles performed by this worker since initialization.

*   **`long TotalEntriesRemoved`**
    Gets the cumulative count of cache entries that have been evicted or removed during cleanup operations.

*   **`long CurrentCacheSize`**
    Gets the current total size of the cache in bytes.

*   **`long MaxCacheSize`**
    Gets the configured maximum size limit of the cache in bytes.

*   **`int CurrentEntryCount`**
    Gets the current number of active entries stored in the cache.

*   **`double CacheUtilizationPercentage`**
    Gets the ratio of current cache usage to the maximum allowed size, represented as a percentage (e.g., 85.5 for 85.5%).

*   **`string ToString()`**
    Returns a string representation of the worker's current state, including summary statistics and current utilization.

## Usage

### Manual Cleanup Trigger
This example demonstrates how to check the current cache utilization and manually invoke a cleanup if the usage exceeds a specific threshold.

```csharp
using (var worker = new CacheCleanupWorker())
{
    // Check if usage exceeds 80%
    if (worker.CacheUtilizationPercentage > 80.0)
    {
        worker.TriggerCleanup();
    }
}
```

### Retrieving and Displaying Statistics
This example retrieves the detailed `CleanupStatistics` object and outputs key monitoring metrics to the console.

```csharp
var worker = new CacheCleanupWorker();

// Retrieve and log worker metrics
var stats = worker.GetStatistics();
Console.WriteLine($"Total Entries Removed: {worker.TotalEntriesRemoved}");
Console.WriteLine($"Current Size: {worker.CurrentCacheSize} bytes");
Console.WriteLine($"Utilization: {worker.CacheUtilizationPercentage:F2}%");
```

## Notes

*   **Thread Safety:** The monitoring properties (e.g., `CurrentCacheSize`, `CacheUtilizationPercentage`) are designed to be thread-safe for reading. However, manual calls to `TriggerCleanup` may temporarily block or affect ongoing cache operations, depending on the underlying implementation of the eviction policy.
*   **Dispose:** The `Dispose` method should be called when the `CacheCleanupWorker` is no longer required to ensure that any background tasks, event listeners, or timer-based cleanup cycles are properly terminated and resources are released.
*   **Edge Cases:** If `MaxCacheSize` is configured as zero or a very small value, frequent cleanup operations may occur, which could impact rendering performance. Ensure that thresholds are balanced appropriately for the system's memory constraints.
