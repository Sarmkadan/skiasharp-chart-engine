# PerformanceMonitoringMiddleware

The `PerformanceMonitoringMiddleware` class provides a simple mechanism to measure execution time, memory usage, and collect aggregated statistics for operations within the skiasharp-chart-engine pipeline. It is intended to be instantiated per logical flow (e.g., per request) and used to bracket code sections with `StartRequest` and `EndRequest` calls.

## API

### PerformanceMonitoringMiddleware()
Initializes a new instance with default values. No parameters are required. This constructor does not throw any exceptions.

### PerformanceContext StartRequest(string operationName, string requestId = null)
Begins monitoring a new operation.

- **Parameters**
  - `operationName`: The name of the operation to monitor. Must not be null or empty.
  - `requestId`: An optional identifier for the request (e.g., a trace identifier). May be null.
- **Return value**: A `PerformanceContext` token that represents the started operation and must be passed to `EndRequest`.
- **Exceptions**
  - `ArgumentNullException` if `operationName` is null or empty.
  - No other exceptions are thrown under normal use.

### void EndRequest(PerformanceContext context)
Stops monitoring for the operation represented by `context` and updates internal statistics.

- **Parameters**
  - `context`: The `PerformanceContext` returned by a prior call to `StartRequest` on this instance.
- **Exceptions**
  - `ArgumentNullException` if `context` is null.
  - `InvalidOperationException` if `context` was not obtained from this instance or if `EndRequest` has already been called for the same context.

### PerformanceStatistics GetStatistics()
Returns a snapshot of accumulated statistics for all monitored operations since the instance was created or last reset.

- **Parameters**: None.
- **Return value**: A `PerformanceStatistics` object containing metrics such as total calls, average, min, max elapsed milliseconds, etc.
- **Exceptions**: None.

### void SetSlowThreshold(long milliseconds)
Defines the threshold (in milliseconds) above which an operation is considered slow for statistical tracking.

- **Parameters**
  - `milliseconds`: The threshold value; must be greater than zero.
- **Exceptions**
  - `ArgumentOutOfRangeException` if `milliseconds` is less than or equal to zero.

### string RequestId { get; set; }
Gets or sets the identifier of the last monitored request. If no request has been monitored, the getter returns null.

### string OperationName { get; }
Gets the name of the operation most recently monitored via `StartRequest`/`EndRequest`. Returns null if no operation has been recorded.

### DateTime StartTime { get; }
Gets the UTC timestamp when the last monitored operation started. Returns `DateTime.MinValue` if no operation has been started.

### DateTime EndTime { get; }
Gets the UTC timestamp when the last monitored operation ended. Returns `DateTime.MinValue` if the operation has not yet ended.

### Stopwatch Stopwatch { get; }
Gets the underlying `System.Diagnostics.Stopwatch` used for timing the last operation. The instance is reset between calls; manual manipulation may break internal timing.

### long StartMemory { get; }
Gets the memory usage (in bytes) reported by `GC.GetTotalMemory(false)` at the start of the last operation. Returns 0 if memory measurement has not been performed.

### long EndMemory { get; }
Gets the memory usage at the end of the last operation. Returns 0 if measurement has not been performed.

### long MemoryUsedBytes { get; }
Gets the difference between `EndMemory` and `StartMemory` for the last operation. May be negative if memory decreased.

### long ElapsedMilliseconds { get; }
Gets the elapsed time in milliseconds for the last operation as measured by the internal `Stopwatch`.

### long AverageMs { get; }
Gets the average elapsed time (ms) across all operations recorded since instantiation or the last reset.

### long MinMs { get; }
Gets the minimum elapsed time (ms) observed among all recorded operations.

### long MaxMs { get; }
Gets the maximum elapsed time (ms) observed among all recorded operations.

### long TotalCalls { get; }
Gets the total number of operations that have been successfully monitored (i.e., complete `StartRequest`/`EndRequest` pairs).

### DateTime CollectedAt { get; }
Gets the UTC timestamp when the statistics snapshot returned by `GetStatistics` was last updated. This corresponds to the `EndTime` of the most recent completed operation.

## Usage

### Example 1: Monitoring a single request in an ASP.NET Core middleware

```csharp
public class MyMiddleware
{
    private readonly PerformanceMonitoringMiddleware _monitor = new PerformanceMonitoringMiddleware();

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var ctx = _monitor.StartRequest("HandleRequest", context.TraceIdentifier);
        try
        {
            await next(context);
        }
        finally
        {
            _monitor.EndRequest(ctx);
        }
    }
}
```

### Example 2: Collecting and reporting statistics after a batch of operations

```csharp
var monitor = new PerformanceMonitoringMiddleware();
monitor.SetSlowThreshold(200); // treat operations >200 ms as slow

// Simulate several operations
for (int i = 0; i < 5; i++)
{
    var c = monitor.StartRequest($"Operation{i}");
    // … perform work …
    monitor.EndRequest(c);
}

var stats = monitor.GetStatistics();
Console.WriteLine($"Total calls: {stats.TotalCalls}");
Console.WriteLine($"Average ms: {stats.AverageMs}");
Console.WriteLine($"Max ms: {stats.MaxMs}");
Console.WriteLine($"Slow threshold breaches: {stats.SlowCalls}");
```

## Notes

- The class is **not thread‑safe**. Concurrent calls to `StartRequest`/`EndRequest` from multiple threads will corrupt internal state. Use one instance per logical thread or synchronize access externally.
- Each `StartRequest` must be matched with a corresponding `EndRequest`. Failure to call `EndRequest` leaves the internal `Stopwatch` running and memory counters stale, causing subsequent statistics to be inaccurate.
- `RequestId` and `OperationName` reflect only the most recent completed operation; they are overwritten on each new monitoring cycle.
- Memory measurements rely on `GC.GetTotalMemory(false)` and are approximate; they do not account for native allocations.
- Adjusting the slow threshold via `SetSlowThreshold` affects only future statistical calculations; previously collected data is not re‑evaluated.
- `GetStatistics` returns a snapshot; mutating the returned `PerformanceStatistics` object does not affect the middleware’s internal state.
- Passing a null or empty `operationName` to `StartRequest` throws `ArgumentNullException`. Passing a null `context` to `EndRequest` throws `ArgumentNullException`; supplying a context not obtained from this instance throws `InvalidOperationException`.
- The exposed `Stopwatch` property is intended for advanced inspection only. Resetting or modifying it manually will break the middleware’s timing logic.
