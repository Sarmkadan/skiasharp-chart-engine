# PerformanceMonitoringMiddlewareExtensions

Provides extension methods for instrumenting and evaluating the performance of operations within the chart rendering pipeline. This type offers a fluent, diagnostic-oriented API to start monitoring, retrieve aggregated statistics, identify slow operations, and generate human-readable performance reports.

## API

### StartMonitoring

```csharp
public static PerformanceContext StartMonitoring(this IPerformanceMonitor monitor, string operationName)
```

Initiates a performance measurement session for a named operation. Returns a `PerformanceContext` that captures the start timestamp and operation identity. The context must be passed to `EndMonitoring` to finalize the measurement.

- **Parameters**:
  - `monitor`: The performance monitor instance being extended.
  - `operationName`: A unique, descriptive name for the operation (e.g., `"RenderAxes"`, `"DataAggregation"`).
- **Returns**: A `PerformanceContext` representing the active measurement session.
- **Throws**: `ArgumentNullException` if `operationName` is null or empty.

### EndMonitoring

```csharp
public static PerformanceStatistics EndMonitoring(this IPerformanceMonitor monitor, PerformanceContext context)
```

Stops the measurement session identified by the given context and records the elapsed time. Returns a `PerformanceStatistics` object containing the operation name, duration, and completion timestamp.

- **Parameters**:
  - `monitor`: The performance monitor instance being extended.
  - `context`: The `PerformanceContext` previously obtained from `StartMonitoring`.
- **Returns**: A `PerformanceStatistics` instance with the finalized timing data.
- **Throws**: `InvalidOperationException` if the context has already been ended or was not started by this monitor.

### WasSlowOperation

```csharp
public static bool WasSlowOperation(this PerformanceStatistics statistics, TimeSpan threshold)
```

Determines whether the recorded operation exceeded a given duration threshold.

- **Parameters**:
  - `statistics`: The performance statistics to evaluate.
  - `threshold`: The `TimeSpan` above which an operation is considered slow.
- **Returns**: `true` if the operation duration is greater than `threshold`; otherwise `false`.

### GetPerformanceReport

```csharp
public static string GetPerformanceReport(this IPerformanceMonitor monitor)
```

Generates a formatted, multi-line string summarizing all recorded operations, their durations, and slow-operation flags based on the monitor's configured thresholds.

- **Parameters**:
  - `monitor`: The performance monitor instance being extended.
- **Returns**: A string containing the performance report.
- **Throws**: `InvalidOperationException` if no operations have been recorded.

### GetAllOperationStatistics

```csharp
public static Dictionary<string, PerformanceStatistics> GetAllOperationStatistics(this IPerformanceMonitor monitor)
```

Retrieves a dictionary of all finalized performance statistics, keyed by operation name. Each entry represents the most recent execution of that operation.

- **Parameters**:
  - `monitor`: The performance monitor instance being extended.
- **Returns**: A `Dictionary<string, PerformanceStatistics>` where keys are operation names and values are the corresponding statistics.
- **Remarks**: If an operation name is reused, only the latest statistics are retained.

### IsOperationSuccessful

```csharp
public static bool IsOperationSuccessful(this PerformanceStatistics statistics)
```

Indicates whether the operation completed without exceptions or cancellations during its measured execution.

- **Parameters**:
  - `statistics`: The performance statistics to evaluate.
- **Returns**: `true` if the operation finished successfully; `false` if it was faulted or cancelled.

### GetSlowestOperations

```csharp
public static List<PerformanceStatistics> GetSlowestOperations(this IPerformanceMonitor monitor, int count)
```

Returns a descending-ordered list of the slowest recorded operations, limited to the specified count.

- **Parameters**:
  - `monitor`: The performance monitor instance being extended.
  - `count`: The maximum number of slow operations to return.
- **Returns**: A `List<PerformanceStatistics>` sorted by duration in descending order.
- **Throws**: `ArgumentOutOfRangeException` if `count` is less than 1.

## Usage

### Example 1: Basic Instrumentation and Threshold Check

```csharp
var monitor = new ChartPerformanceMonitor();
var context = monitor.StartMonitoring("RenderScatterSeries");

// ... chart rendering work ...

var stats = monitor.EndMonitoring(context);
TimeSpan slowThreshold = TimeSpan.FromMilliseconds(200);

if (stats.WasSlowOperation(slowThreshold))
{
    Console.WriteLine($"Slow operation detected: {stats.OperationName} took {stats.Duration.TotalMilliseconds:F2} ms");
}
```

### Example 2: Generating a Report and Identifying Top Slow Operations

```csharp
var monitor = new ChartPerformanceMonitor();

// Instrument multiple pipeline stages
var ctx1 = monitor.StartMonitoring("DataAggregation");
// ... aggregation logic ...
monitor.EndMonitoring(ctx1);

var ctx2 = monitor.StartMonitoring("RenderAxes");
// ... axis rendering ...
monitor.EndMonitoring(ctx2);

var ctx3 = monitor.StartMonitoring("RenderGridlines");
// ... gridline rendering ...
monitor.EndMonitoring(ctx3);

// Generate full report
string report = monitor.GetPerformanceReport();
Console.WriteLine(report);

// Inspect top 2 slowest operations
var slowest = monitor.GetSlowestOperations(2);
foreach (var op in slowest)
{
    Console.WriteLine($"{op.OperationName}: {op.Duration.TotalMilliseconds:F2} ms (Success: {op.IsOperationSuccessful()})");
}
```

## Notes

- **Context Lifecycle**: A `PerformanceContext` returned by `StartMonitoring` must be paired with exactly one call to `EndMonitoring`. Reusing or discarding a context without ending it leaves the monitor in an incomplete state and may cause subsequent report generation to throw.
- **Thread Safety**: The extension methods delegate to the underlying `IPerformanceMonitor` implementation. If the monitor instance is not thread-safe, concurrent calls to `StartMonitoring`/`EndMonitoring` from multiple threads may produce corrupted statistics or race conditions. Synchronize access externally when sharing a monitor across threads.
- **Operation Name Uniqueness**: `GetAllOperationStatistics` retains only the most recent statistics per operation name. If multiple concurrent or sequential measurements use the same name, earlier results are overwritten. Use distinct names for parallel or repeated measurements that must be preserved independently.
- **Slow Threshold Sensitivity**: `WasSlowOperation` performs a strict greater-than comparison. An operation taking exactly the threshold duration is not flagged as slow.
- **Report Format Stability**: The string returned by `GetPerformanceReport` is intended for human consumption. Its exact formatting may vary across versions and should not be parsed programmatically.
