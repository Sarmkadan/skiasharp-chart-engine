# MetricsCollector

The `MetricsCollector` class provides a structured mechanism for profiling, tracking, and summarizing the execution performance of operations within the `skiasharp-chart-engine`. It enables developers to monitor execution duration, memory consumption, and thread context, allowing for granular analysis of performance bottlenecks and resource utilization during charting and rendering processes.

## API

### Constructors

*   **`MetricsCollector()`**
    Initializes a new instance of the `MetricsCollector` class.

### Methods

*   **`MetricsContext StartCollection(string operationName)`**
    Starts a new collection cycle for the specified operation. Returns a `MetricsContext` that should be used to scope the operation.
*   **`void EndCollection()`**
    Finalizes the current collection cycle, calculating elapsed time and memory usage.
*   **`OperationMetrics GetMetrics()`**
    Returns the `OperationMetrics` for the current operation.
*   **`MetricsSummary GetSummary()`**
    Generates a `MetricsSummary` based on the current collection data.
*   **`Dictionary<string, MetricsSummary> GetAllSummaries()`**
    Returns a dictionary mapping operation names to their respective `MetricsSummary` objects.
*   **`void Clear()`**
    Resets all internal metrics data.
*   **`int GetOperationCount()`**
    Returns the total number of operations collected.

### Properties

*   **`string OperationName`** (get)
    The name of the currently tracked operation.
*   **`DateTime StartTime`** (get)
    The timestamp when the collection began.
*   **`DateTime EndTime`** (get)
    The timestamp when the collection ended.
*   **`Stopwatch Stopwatch`** (get)
    The internal `System.Diagnostics.Stopwatch` instance used for timing.
*   **`long StartMemory`** (get)
    The memory usage (in bytes) at the start of the collection.
*   **`long EndMemory`** (get)
    The memory usage (in bytes) at the end of the collection.
*   **`long MemoryUsedBytes`** (get)
    The calculated memory difference during the operation.
*   **`long ElapsedMs`** (get)
    The elapsed time in milliseconds.
*   **`int ThreadId`** (get)
    The ID of the thread where the operation was executed.
*   **`List<ExecutionMetric> Executions`** (get)
    A list of individual `ExecutionMetric` records.
*   **`DateTime ExecutedAt`** (get)
    The timestamp when the current execution was recorded.

## Usage

### Example 1: Basic Operation Timing
```csharp
var collector = new MetricsCollector();

using (collector.StartCollection("RenderChart"))
{
    // Perform chart rendering logic
    RenderEngine.Draw(chartData);
    collector.EndCollection();
}

var metrics = collector.GetMetrics();
Console.WriteLine($"Operation: {collector.OperationName}, Elapsed: {collector.ElapsedMs}ms");
```

### Example 2: Aggregating Multiple Operations
```csharp
var collector = new MetricsCollector();

// Simulate multiple operations
for (int i = 0; i < 5; i++)
{
    collector.StartCollection($"DataProcessing_{i}");
    // Perform data transformation
    DataTransformer.Process(input);
    collector.EndCollection();
}

var allSummaries = collector.GetAllSummaries();
foreach (var entry in allSummaries)
{
    Console.WriteLine($"Operation: {entry.Key}, Average Elapsed: {entry.Value.AverageElapsedMs}ms");
}
```

## Notes

*   **Thread Safety:** The `MetricsCollector` instance is not thread-safe. If multiple operations are being profiled concurrently across different threads, maintain a separate `MetricsCollector` instance per thread or use appropriate synchronization mechanisms when accessing shared collector instances.
*   **Memory Tracking:** The `MemoryUsedBytes` property is derived from runtime memory snapshots and represents an approximation of managed memory consumed by the process. It does not account for native memory allocations outside the CLR's tracking.
*   **Invalid State:** Calling `EndCollection()` before `StartCollection()` or without an active `MetricsContext` will result in an invalid state for the current collection, potentially leading to inaccurate `ElapsedMs` or memory calculations. Ensure `EndCollection()` is only called following a successful `StartCollection()` invocation.
