# CacheKeyBuilder

The `CacheKeyBuilder` is a static utility class designed to facilitate the creation, validation, and parsing of unique string identifiers used for caching components, rendering states, and configuration data within the `skiasharp-chart-engine`. It ensures cache keys adhere to a consistent structure, enabling reliable cache retrieval, targeted invalidation, and efficient resource management.

## API

*   **`BuildChartKey(string chartId)`**
    Generates a unique cache key for a specific chart instance.
    *   **Parameters:** `chartId` (string) - The unique identifier for the chart.
    *   **Returns:** A formatted string key representing the chart.

*   **`BuildRenderKey(string chartId, string renderId)`**
    Creates a unique key for a specific rendering iteration of a chart.
    *   **Parameters:** `chartId` (string), `renderId` (string) - The identifiers for the chart and the rendering task.
    *   **Returns:** A formatted string key for the specific render state.

*   **`BuildSeriesKey(string chartId, string seriesId)`**
    Generates a unique key for a specific data series within a chart.
    *   **Parameters:** `chartId` (string), `seriesId` (string) - The identifiers for the chart and the data series.
    *   **Returns:** A formatted string key for the series.

*   **`BuildConfigurationKey(string chartId)`**
    Generates a unique key for the configuration settings of a specific chart.
    *   **Parameters:** `chartId` (string) - The unique identifier for the chart.
    *   **Returns:** A formatted string key for the chart configuration.

*   **`BuildAxisKey(string chartId, string axisId)`**
    Creates a unique key for the definition of a specific chart axis.
    *   **Parameters:** `chartId` (string), `axisId` (string) - The identifiers for the chart and the axis.
    *   **Returns:** A formatted string key for the axis.

*   **`BuildPaletteKey(string chartId)`**
    Generates a unique key for the color palette associated with a specific chart.
    *   **Parameters:** `chartId` (string) - The unique identifier for the chart.
    *   **Returns:** A formatted string key for the chart palette.

*   **`ExtractChartIdFromKey(string key)`**
    Parses the chart identifier from a previously generated cache key.
    *   **Parameters:** `key` (string) - The cache key to parse.
    *   **Returns:** A string containing the extracted `chartId`, or `null` if the key format is invalid.

*   **`BuildInvalidationPattern(string chartId)`**
    Generates a pattern string used to identify all cache keys associated with a specific chart for invalidation.
    *   **Parameters:** `chartId` (string) - The unique identifier for the chart.
    *   **Returns:** A string representing the pattern for invalidation matching.

*   **`IsValidCacheKey(string key)`**
    Validates whether a provided string conforms to the expected internal cache key format.
    *   **Parameters:** `key` (string) - The string to validate.
    *   **Returns:** `true` if the key format is valid; otherwise, `false`.

*   **`BuildCompositeKey(params string[] parts)`**
    Constructs a hierarchical, delimiter-separated string key from an arbitrary number of components.
    *   **Parameters:** `parts` (string[]) - An array of strings representing the key components.
    *   **Returns:** A single composite string key.

## Usage

```csharp
// Example 1: Generating a key for cache storage
string chartId = "sales-dashboard-01";
string axisId = "x-axis-01";
string axisCacheKey = CacheKeyBuilder.BuildAxisKey(chartId, axisId);

_cache.Set(axisCacheKey, axisData);

// Example 2: Parsing an ID during cache cleanup
string keyToProcess = "chart:sales-dashboard-01:render:v1";
string? extractedChartId = CacheKeyBuilder.ExtractChartIdFromKey(keyToProcess);

if (extractedChartId != null)
{
    // Proceed with specific logic for the chart
}
```

## Notes

*   **Thread-Safety:** The methods in `CacheKeyBuilder` are stateless and perform string manipulations, making them inherently thread-safe for concurrent operations.
*   **Edge Cases:** Passing `null` or empty strings as identifiers to the build methods may result in malformed keys, depending on the underlying implementation of delimiter concatenation. Always ensure identifiers are validated before generating keys.
*   **Consistency:** The `BuildInvalidationPattern` should be used in conjunction with a cache provider that supports prefix-based or pattern-based key removal or retrieval.
