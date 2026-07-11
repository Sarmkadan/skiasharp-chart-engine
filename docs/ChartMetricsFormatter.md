# ChartMetricsFormatter

`ChartMetricsFormatter` is a utility class in the SkiaSharp Chart Engine that provides methods for formatting various chart-related metrics and statistics into human-readable strings or structured formats such as JSON. It is designed to assist in debugging, logging, and monitoring chart rendering performance by exposing detailed information about memory usage, data points, chart configuration, and comparison metrics.

## API

### `ChartMetricsFormatter`
The default constructor initializes a new instance of the `ChartMetricsFormatter` class. No parameters are required, and the instance will track memory statistics internally.

### `FormatRenderMetrics`
```csharp
public string FormatRenderMetrics()
```
Formats a human-readable summary of the current rendering metrics, including memory usage and performance statistics. The output is intended for console or log output and is not structured for machine parsing.

**Returns**
A string containing formatted render metrics.

### `FormatRenderMetricsAsJson`
```csharp
public string FormatRenderMetricsAsJson()
```
Formats the current rendering metrics as a JSON string. This method provides a structured, machine-readable output suitable for APIs or logging systems that require JSON.

**Returns**
A JSON-formatted string containing render metrics.

**Throws**
`InvalidOperationException` if JSON serialization fails (e.g., due to circular references or unsupported types).

### `FormatChartConfiguration`
```csharp
public string FormatChartConfiguration()
```
Formats the current chart configuration as a human-readable string. This includes properties such as chart type, axes, series, and other configuration details.

**Returns**
A string containing the formatted chart configuration.

### `FormatChartSummary`
```csharp
public string FormatChartSummary()
```
Generates a concise summary of the chart, including key metrics such as data point count, memory usage, and rendering time (if available).

**Returns**
A string summarizing the chart’s state and metrics.

### `FormatComparison`
```csharp
public string FormatComparison()
```
Formats a comparison of current metrics against a previous state (e.g., before and after a rendering operation). Useful for performance regression testing.

**Returns**
A string containing the comparison output.

### `FormatDataPoint`
```csharp
public string FormatDataPoint()
```
Formats the current data point (if set) as a readable string, including its value, position, and metadata.

**Returns**
A string representing the data point.

### `FormatBytes`
```csharp
public static string FormatBytes(long bytes)
```
Formats a byte count into a human-readable string with appropriate units (e.g., KB, MB, GB).

**Parameters**
- `bytes`: The number of bytes to format.

**Returns**
A string such as "1.2 MB".

### `FormatMemoryStatistics`
```csharp
public string FormatMemoryStatistics()
```
Formats detailed memory statistics, including total, used, and available memory, in a human-readable way.

**Returns**
A string containing memory usage statistics.

### `TotalMemory`
```csharp
public long TotalMemory { get; }
```
Gets the total available memory in bytes for the current process or system (platform-dependent).

**Returns**
The total memory in bytes.

### `UsedMemory`
```csharp
public long UsedMemory { get; }
```
Gets the currently used memory in bytes.

**Returns**
The used memory in bytes.

### `AvailableMemory`
```csharp
public long AvailableMemory { get; }
```
Gets the currently available memory in bytes.

**Returns**
The available memory in bytes.

## Usage

### Example 1: Logging Render Metrics
```csharp
var formatter = new ChartMetricsFormatter();
var metrics = formatter.FormatRenderMetrics();
Console.WriteLine(metrics);
```

### Example 2: Exporting Metrics as JSON
```csharp
var formatter = new ChartMetricsFormatter();
var jsonMetrics = formatter.FormatRenderMetricsAsJson();
File.WriteAllText("metrics.json", jsonMetrics);
```

## Notes

- Memory statistics (`TotalMemory`, `UsedMemory`, `AvailableMemory`) are platform-dependent and may not reflect actual system-wide memory on all platforms.
- The `FormatRenderMetricsAsJson` method may throw `InvalidOperationException` if the internal state contains unserializable objects (e.g., delegates or complex SkiaSharp types).
- Thread safety: Instances of `ChartMetricsFormatter` are not thread-safe. If used across threads, external synchronization (e.g., `lock`) is required.
- Edge cases: If memory statistics are unavailable (e.g., on unsupported platforms), `TotalMemory`, `UsedMemory`, and `AvailableMemory` may return 0 or throw `PlatformNotSupportedException`.
- The `FormatBytes` method handles negative values by returning "0 B".
