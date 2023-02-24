# RenderResultCacheTests
The `RenderResultCacheTests` class is a test suite designed to validate the functionality of a render result cache. This cache is responsible for storing and retrieving render results, managing cache size, and tracking cache statistics. The tests cover various scenarios, including caching with null keys or results, storing and retrieving valid data, handling cache size limits, and tracking cache statistics.

## API
* `public RenderResultCacheTests()`: The constructor for the `RenderResultCacheTests` class.
* `public void Cache_WithNullKey_DoesNotCrash()`: Tests that caching with a null key does not cause the application to crash.
* `public void Cache_WithNullResult_DoesNotCrash()`: Tests that caching with a null result does not cause the application to crash.
* `public void Cache_WithValidData_StoresResultWithDefaultTtl()`: Tests that caching with valid data stores the result with the default time-to-live (TTL).
* `public void Cache_WithCustomTtl_RespectsTtlValue()`: Tests that caching with a custom TTL respects the specified TTL value.
* `public void Cache_StoresMultipleEntriesIndependently()`: Tests that the cache stores multiple entries independently.
* `public void Cache_WhenSizeExceedsLimit_EvictsLRU()`: Tests that the cache evicts the least recently used (LRU) entry when the cache size exceeds the limit.
* `public void Cache_ReplacesExistingEntry()`: Tests that the cache replaces an existing entry.
* `public void Get_WithNonExistentKey_ReturnsNull()`: Tests that getting a result with a non-existent key returns null.
* `public void Get_WithValidKey_ReturnsResult()`: Tests that getting a result with a valid key returns the result.
* `public void Get_UpdatesAccessCountAndLastAccessedTime()`: Tests that getting a result updates the access count and last accessed time.
* `public void Get_WithExpiredEntry_ReturnsNullAndRemovesEntry()`: Tests that getting a result with an expired entry returns null and removes the entry.
* `public void Remove_WithExistingKey_ReturnsTrue()`: Tests that removing an existing entry returns true.
* `public void Remove_WithNonExistentKey_ReturnsFalse()`: Tests that removing a non-existent entry returns false.
* `public void Remove_DecreasesCacheSizeCounter()`: Tests that removing an entry decreases the cache size counter.
* `public void Clear_RemovesAllEntries()`: Tests that clearing the cache removes all entries.
* `public void Clear_ResetsSizeCounter()`: Tests that clearing the cache resets the size counter.
* `public void GetStatistics_WithEmptyCache_ReturnsZeroStats()`: Tests that getting statistics with an empty cache returns zero statistics.
* `public void GetStatistics_TracksEntryCountAndSize()`: Tests that getting statistics tracks the entry count and size.
* `public void GetStatistics_TracksHitCount()`: Tests that getting statistics tracks the hit count.

## Usage
The following examples demonstrate how to use the `RenderResultCacheTests` class:
```csharp
// Example 1: Testing cache functionality
RenderResultCacheTests tests = new RenderResultCacheTests();
tests.Cache_WithValidData_StoresResultWithDefaultTtl();
tests.Get_WithValidKey_ReturnsResult();

// Example 2: Testing cache statistics
RenderResultCacheTests statsTests = new RenderResultCacheTests();
statsTests.Cache_WithValidData_StoresResultWithDefaultTtl();
statsTests.GetStatistics_TracksEntryCountAndSize();
```

## Notes
The `RenderResultCacheTests` class is designed to be thread-safe, allowing multiple tests to run concurrently. However, it is essential to note that the cache itself may not be thread-safe, and additional synchronization mechanisms may be required to ensure correct behavior in a multi-threaded environment. Additionally, the cache size limit and TTL values can significantly impact the cache's performance and behavior, and careful consideration should be given to these settings when using the cache in a production environment.
