# RenderCacheService

`RenderCacheService` provides an in-memory caching mechanism for rendered chart images within the skiasharp-chart-engine. It stores byte arrays representing pre-rendered image data alongside metadata such as creation time and access frequency, enabling fast retrieval of previously generated outputs and reducing redundant rendering operations.

## API

### Constructors

#### `RenderCacheService()`
Initializes a new instance of the cache with an empty backing store. No pre-existing entries are loaded.

### Properties

#### `byte[] ImageData`
Gets the raw byte array of the most recently accessed or stored cached image. This property reflects the data of the last cache entry interacted with via `Get` or `Set`. Returns `null` if no such interaction has occurred since construction or the last `Clear`.

#### `DateTime CreatedAt`
Gets the creation timestamp of the most recently accessed or stored cached image. Corresponds to the entry whose `ImageData` is currently exposed. Returns `DateTime.MinValue` if no entry is active.

#### `int AccessCount`
Gets the total number of times the currently active cache entry has been retrieved via `Get`. Returns `0` if no entry is active or the entry has never been accessed.

### Methods

#### `byte[]? Get(string key)`
Retrieves the cached image data associated with the specified key.
- **Parameters**: `key` — the unique identifier for the cached entry.
- **Returns**: the byte array if the key exists; `null` otherwise.
- **Side effects**: increments the `AccessCount` for the entry and updates `ImageData` and `CreatedAt` to reflect the retrieved entry.
- **Throws**: `ArgumentNullException` if `key` is `null`.

#### `void Set(string key, byte[] data)`
Stores or overwrites image data for the given key. The entry’s creation timestamp is set to the current UTC time, and its access count is reset to zero.
- **Parameters**: `key` — the unique identifier for the entry; `data` — the byte array to cache.
- **Side effects**: updates `ImageData` and `CreatedAt` to reflect the newly stored entry.
- **Throws**: `ArgumentNullException` if `key` or `data` is `null`.

#### `void Remove(string key)`
Removes the cached entry identified by the specified key. If the removed entry was the currently active one, `ImageData` becomes `null`, `CreatedAt` resets to `DateTime.MinValue`, and `AccessCount` resets to `0`.
- **Parameters**: `key` — the unique identifier for the entry to remove.
- **Throws**: `ArgumentNullException` if `key` is `null`. No exception is thrown if the key does not exist.

#### `void Clear()`
Removes all entries from the cache. Resets `ImageData` to `null`, `CreatedAt` to `DateTime.MinValue`, and `AccessCount` to `0`.

#### `int GetCacheSize()`
Returns the total number of entries currently held in the cache.

#### `bool Contains(string key)`
Determines whether an entry with the specified key exists in the cache.
- **Parameters**: `key` — the unique identifier to check.
- **Returns**: `true` if the key exists; `false` otherwise.
- **Throws**: `ArgumentNullException` if `key` is `null`.

#### `IEnumerable<string> GetAllKeys()`
Returns an enumerable collection of all keys currently present in the cache. The order is non-deterministic and may vary between calls.

## Usage

### Example 1: Basic Store and Retrieve
```csharp
var cache = new RenderCacheService();
byte[] chartPng = RenderChartToPng("sales-q4");

// Store the rendered image
cache.Set("report/sales-q4", chartPng);

// Later, retrieve without re-rendering
byte[]? cached = cache.Get("report/sales-q4");
if (cached != null)
{
    Console.WriteLine($"Cache hit. Access count: {cache.AccessCount}");
    File.WriteAllBytes("output.png", cached);
}
```

### Example 2: Cache Invalidation and Inspection
```csharp
var cache = new RenderCacheService();
cache.Set("dashboard/2025-03", RenderChartToPng("dashboard-march"));
cache.Set("dashboard/2025-04", RenderChartToPng("dashboard-april"));

// Check existence before retrieval
if (cache.Contains("dashboard/2025-03"))
{
    byte[]? image = cache.Get("dashboard/2025-03");
    // image is non-null here
}

// Invalidate stale entry
cache.Remove("dashboard/2025-03");

Console.WriteLine($"Remaining entries: {cache.GetCacheSize()}");
foreach (string key in cache.GetAllKeys())
{
    Console.WriteLine($"Cached key: {key}");
}

// Clear everything
cache.Clear();
```

## Notes

- The `ImageData`, `CreatedAt`, and `AccessCount` properties reflect the state of the *last entry touched* by `Get` or `Set`. After `Remove` or `Clear`, these properties revert to their default empty states. They do not provide aggregate or queryable metadata for arbitrary keys.
- `Get` returns `null` for missing keys rather than throwing an exception. Always check the return value before consuming it.
- `Set` overwrites without warning. If a key already exists, its previous data, timestamp, and access count are replaced silently.
- `GetAllKeys()` returns a snapshot at the time of invocation. Concurrent modifications during enumeration are not reflected and may cause non-deterministic behavior if the underlying collection is mutated.
- This service is not thread-safe by design. Concurrent calls to `Set`, `Remove`, `Clear`, or `Get` from multiple threads may lead to race conditions, inconsistent property states, or enumeration exceptions. External synchronization is required if shared across threads.
- The cache imposes no size limit or eviction policy. Memory consumption grows unboundedly with each `Set` call. Consumers must manage cache size manually via `Remove` or `Clear` to avoid excessive memory pressure.
