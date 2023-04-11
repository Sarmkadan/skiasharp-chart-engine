# PerformanceMonitor

The `PerformanceMonitor` class is a specialized utility within the `skiasharp-chart-engine` designed to track and aggregate execution metrics for named operations. It enables developers to monitor performance characteristics, such as execution time and success rates, by capturing raw metrics and providing statistical summaries including averages, minimums, maximums, and percentiles.

## API

### Constructors

*   **`PerformanceMonitor()`**
    Initializes a new instance of the `PerformanceMonitor` class with no tracked data.

### Methods

*   **`void RecordMetric(string operationName, long elapsedMs, bool success)`**
    Records a single metric entry for the specified operation.
    *   `operationName`: The unique name of the operation being measured.
    *   `elapsedMs`: The execution time in milliseconds.
    *   `success`: A boolean indicating if the operation completed successfully.

*   **`PerformanceStatistics? GetStatistics(string operationName)`**
    Retrieves the aggregated statistics for a specific operation. Returns `null` if no metrics have been recorded for the operation.

*   **`List<PerformanceStatistics> GetAllStatistics()`**
    Returns a list containing the aggregated statistics for all operations currently tracked by the monitor.

*   **`void Clear()`**
    Removes all recorded metrics and resets the monitor to its initial empty state.

*   **`void ClearOperation(string operationName)`**
    Removes all recorded metrics associated with the specified operation.

*   **`int GetTrackedOperationCount()`**
    Returns the number of unique operations currently being tracked.

*   **`int GetTotalMetricCount()`**
    Returns the total number of metric entries recorded across all operations.

### Data Structure: PerformanceStatistics

The `PerformanceStatistics` object is returned by `GetStatistics` and `GetAllStatistics`, providing the following read-only properties:

*   `OperationName`: The name of the operation.
*   `ElapsedMilliseconds`: The raw total time (context dependent).
*   `Success`: The status of the last recorded operation.
*   `Timestamp`: The timestamp of the most recent recording.
*   `SampleCount`: Total number of recorded samples.
*   `FailureCount`: Total number of failed operations.
*   `AverageMs`: The average execution time in milliseconds.
*   `MinMs`: The minimum execution time recorded.
*   `MaxMs`: The maximum execution time recorded.
*   `MedianMs`: The median execution time in milliseconds.
*   `P95Ms`: The 95th percentile execution time in milliseconds.

## Usage

### Recording and Retrieving Metrics
```csharp
var monitor = new PerformanceMonitor();

// Record performance data for a rendering operation
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
bool success = PerformRenderOperation();
stopwatch.Stop();

monitor.RecordMetric("RenderLoop", stopwatch.ElapsedMilliseconds, success);

// Retrieve and display statistics
var stats = monitor.GetStatistics("RenderLoop");
if (stats != null)
{
    Console.WriteLine($"Average Render Time: {stats.AverageMs}ms");
    Console.WriteLine($"P95 Latency: {stats.P95Ms}ms");
}
```

### Batch Processing and Clearing
```csharp
var monitor = new PerformanceMonitor();

// Simulate multiple operations
for (int i = 0; i < 100; i++)
{
    monitor.RecordMetric("DataProcessing", 15 + (i % 5), true);
}

// Check total data
int totalMetrics = monitor.GetTotalMetricCount(); // 100

// Reset specific operation tracking
monitor.ClearOperation("DataProcessing");
```

## Notes

*   **Thread Safety**: The `PerformanceMonitor` class is not inherently thread-safe. If multiple threads record metrics simultaneously, the caller must implement external synchronization to prevent data corruption or inconsistent states.
*   **Memory Usage**: Metrics are stored in memory. In scenarios with a very high frequency of `RecordMetric` calls over long durations, monitor memory usage may grow. Regularly calling `Clear()` or `ClearOperation()` is recommended to prevent unbounded growth in long-running applications.
*   **Performance**: `RecordMetric` is designed to be lightweight, but intensive recording in extremely hot paths may still introduce measurable overhead.
