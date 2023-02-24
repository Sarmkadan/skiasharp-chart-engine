# RenderCacheServiceTests

Test suite for the `RenderCacheService` class, verifying correct behavior of caching, retrieval, insertion, removal, and eviction logic.

## API

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `Get_WithNullKey_ReturnsNull` | Confirms that requesting a cached image with a `null` key yields `null`. | none | `void` (test passes if assertion succeeds) | Throws if the method under test does not return `null`. |
| `Get_WithEmptyKey_ReturnsNull` | Confirms that requesting a cached image with an empty string key yields `null`. | none | `void` | Throws if the method under test does not return `null`. |
| `Get_WithNonExistentKey_ReturnsNull` | Confirms that requesting a cached image with a key that has never been stored yields `null`. | none | `void` | Throws if the method under test does not return `null`. |
| `Get_WithExistingKey_ReturnsImageData` | Confirms that a previously stored image can be retrieved correctly by its key. | none | `void` | Throws if the retrieved data does not match the stored image or if `null` is returned. |
| `Get_IncrementsAccessCount` | Verifies that each successful `Get` operation increments the internal access counter for the key (used for LRU tracking). | none | `void` | Throws if the access count is not incremented as expected. |
| `Set_WithNullKey_DoesNotCrash` | Ensures that calling `Set` with a `null` key does not throw an exception. | none | `void` | Throws if an exception is observed. |
| `Set_WithEmptyKey_DoesNotCrash` | Ensures that calling `Set` with an empty string key does not throw an exception. | none | `void` | Throws if an exception is observed. |
| `Set_WithNullImageData_DoesNotCrash` | Ensures that calling `Set` with a `null` image payload does not throw an exception. | none | `void` | Throws if an exception is observed. |
| `Set_WithValidData_StoresEntry` | Confirms that a valid key/image pair is stored and can be retrieved later. | none | `void` | Throws if the entry is not retrievable after insertion. |
| `Set_ReplacesExistingEntry` | Confirms that inserting a new image with an existing key overwrites the previous entry. | none | `void` | Throws if the old value is still present after replacement. |
| `Set_WhenMaxCacheSizeReached_EvictsLRU` | Verifies that when the cache exceeds its configured maximum size, the least‑recently‑used entry is removed. | none | `void` | Throws if eviction does not follow LRU policy or if the cache size exceeds the limit. |
| `Set_EvicsLeastAccessedWhenTied` | Verifies that when multiple entries have the same access count, the one that was least recently accessed is evicted. | none | `void` | Throws if tie‑breaking does not respect the expected LRU order. |
| `Set_WhenBucketFull_AndKeyAccessedThenOtherEvicted` | Tests a scenario where a bucket (hash‑collision chain) is full; accessing a key within the bucket changes its recency, causing a different entry to be evicted on the next insertion. | none | `void` | Throws if the wrong entry is evicted. |
| `Remove_WithNullKey_DoesNotCrash` | Ensures that calling `Remove` with a `null` key does not throw an exception. | none | `void` | Throws if an exception is observed. |
| `Remove_WithEmptyKey_DoesNotCrash` | Ensures that calling `Remove` with an empty string key does not throw an exception. | none | `void` | Throws if an exception is observed. |
| `Remove_WithExistingKey_RemovesEntry` | Confirms that removing an existing key deletes its entry from the cache. | none | `void` | Throws if the entry remains present after removal. |
| `Remove_WithNonExistentKey_DoesNotThrow` | Confirms that attempting to remove a key that is not present does not throw an exception. | none | `void` | Throws if an exception is observed. |
| `Clear_RemovesAllEntries` | Verifies that `Clear` empties the cache, removing all stored entries. | none | `void` | Throws if any entry remains after `Clear`. |
| `GetCacheSize_WithEmptyCache_ReturnsZero` | Confirms that the cache size reporter returns zero when no entries are stored. | none | `void` | Throws if the reported size is not zero. |

## Usage

The test class is intended to be executed by a unit‑test runner (e.g., MSTest, xUnit, NUnit). Below are two illustrative ways to invoke individual tests manually.

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiasharpChartEngine.Tests; // adjust namespace as needed

[TestClass]
public class ManualTestRunner
{
    [TestMethod]
    public void TestGetWithNullKey()
    {
        var testInstance = new RenderCacheServiceTests();
        testInstance.Get_WithNullKey_ReturnsNull(); // passes if no assertion failure
    }

    [TestMethod]
    public void TestLruEvictionOnMaxSize()
    {
        var testInstance = new RenderCacheServiceTests();
        testInstance.Set_WhenMaxCacheSizeReached_EvictsLRU(); // passes if LRU eviction observed
    }
}
```

Alternatively, using a simple console harness:

```csharp
using SkiasharpChartEngine.Tests;

var tests = new RenderCacheServiceTests();

try
{
    tests.Get_WithExistingKey_ReturnsImageData();
    Console.WriteLine("Get_WithExistingKey_ReturnsImageData passed");
}
catch (AssertionFailedException ex)
{
    Console.WriteLine($"Get_WithExistingKey_ReturnsImageData failed: {ex.Message}");
}
```

## Notes

- **Null/empty keys**: All public methods treat `null` or empty string keys as benign inputs; they never store data and never throw.
- **LRU eviction**: The cache evicts entries based on a combination of access count and recency. When access counts are tied, the entry accessed longest ago is removed.
- **Thread safety**: `RenderCacheService` (and consequently its tests) does not provide internal synchronization. Concurrent calls from multiple threads may lead to undefined behavior; external locking is required for thread‑safe usage.
- **Exception behavior**: Test methods only fail via assertions; they do not deliberately throw other exceptions. Any unexpected exception indicates a defect in the implementation under test.
- **State isolation**: Each test method assumes a clean cache state. The test class does not rely on shared state between methods; however, if the underlying service uses static globals, test order could affect outcomes. In the current repository, each test creates its own service instance, ensuring isolation.
