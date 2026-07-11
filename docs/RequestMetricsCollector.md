# RequestMetricsCollector

The `RequestMetricsCollector` class provides a centralized mechanism for tracking, aggregating, and analyzing performance metrics for HTTP endpoints within the `skiasharp-chart-engine`. It captures request counts, success and error rates, response time distributions, and data transfer sizes, enabling developers to monitor system health and identify performance bottlenecks efficiently.

## API

### Constructors and Methods

*   **`RequestMetricsCollector()`**
    Initializes a new instance of the `RequestMetricsCollector` class.

*   **`void RecordRequest(long durationMs, bool success, long requestSize, long responseSize)`**
    Records metrics for a single request.
    *   `durationMs`: The time taken to process the request, in milliseconds.
    *   `success`: A boolean indicating whether the request was successful.
    *   `requestSize`: The size of the request payload, in bytes.
    *   `responseSize`: The size of the response payload, in bytes.

*   **`EndpointMetrics? GetEndpointMetrics(string endpoint)`**
    Retrieves the aggregated metrics for the specified endpoint.
    *   `endpoint`: The identifier of the endpoint to query.
    *   **Returns**: An `EndpointMetrics` object if data exists for the endpoint; otherwise, `null`.

*   **`List<EndpointMetrics> GetAllEndpointMetrics()`**
    Retrieves a list of metrics for all tracked endpoints.
    *   **Returns**: A `List<EndpointMetrics>` containing metrics for all endpoints.

*   **`SystemMetrics GetSystemMetrics()`**
    Retrieves a summary of metrics across all tracked endpoints.
    *   **Returns**: A `SystemMetrics` object representing the aggregated system-wide performance.

*   **`EndpointMetrics? GetEndpointMetricsForPeriod(string endpoint, DateTime start, DateTime end)`**
    Retrieves metrics for a specific endpoint within a defined time period.
    *   `endpoint`: The identifier of the endpoint to query.
    *   `start`: The start of the time period.
    *   `end`: The end of the time period.
    *   **Returns**: An `EndpointMetrics` object for the specified period if data exists; otherwise, `null`.

*   **`void Clear()`**
    Resets all collected metrics to their initial state.

### Properties

| Property | Description |
| :--- | :--- |
| `string Endpoint` | Gets the identifier for the endpoint associated with this collector instance. |
| `int RequestCount` | Gets the total number of requests recorded. |
| `int SuccessCount` | Gets the total number of successful requests. |
| `int ErrorCount` | Gets the total number of failed requests. |
| `double SuccessRate` | Gets the calculated success rate as a value between 0.0 and 1.0. |
| `long AverageResponseTimeMs` | Gets the average response time for all requests in milliseconds. |
| `long MinResponseTimeMs` | Gets the minimum response time recorded in milliseconds. |
| `long MaxResponseTimeMs` | Gets the maximum response time recorded in milliseconds. |
| `long MedianResponseTimeMs` | Gets the median response time in milliseconds. |
| `long P95ResponseTimeMs` | Gets the 95th percentile response time in milliseconds. |
| `long P99ResponseTimeMs` | Gets the 99th percentile response time in milliseconds. |
| `long AverageRequestSize` | Gets the average size of request payloads in bytes. |
| `long AverageResponseSize` | Gets the average size of response payloads in bytes. |

## Usage

### Example 1: Recording Request Metrics

```csharp
var collector = new RequestMetricsCollector();

// Record a successful request
collector.RecordRequest(durationMs: 150, success: true, requestSize: 512, responseSize: 2048);

// Record a failed request
collector.RecordRequest(durationMs: 50, success: false, requestSize: 256, responseSize: 0);

Console.WriteLine($"Total requests: {collector.RequestCount}");
Console.WriteLine($"Success rate: {collector.SuccessRate * 100}%");
```

### Example 2: Analyzing Performance

```csharp
var collector = new RequestMetricsCollector();
// ... assume metrics are recorded over time ...

var metrics = collector.GetEndpointMetrics("api/chart/render");
if (metrics != null)
{
    Console.WriteLine($"P95 Latency: {metrics.P95ResponseTimeMs}ms");
    Console.WriteLine($"Average Response Size: {metrics.AverageResponseSize} bytes");
}
else
{
    Console.WriteLine("No metrics found for the specified endpoint.");
}
```

## Notes

*   **Thread Safety**: The `RequestMetricsCollector` is designed to be thread-safe, allowing `RecordRequest` to be called concurrently from multiple threads without corruption of the internal metrics state.
*   **Performance Impact**: While optimized for high-throughput scenarios, frequent calls to calculate percentile-based metrics (P95, P99) under extreme load may have a marginal impact on performance.
*   **Persistence**: Metrics collected by `RequestMetricsCollector` are stored in memory. Calling `Clear()` or restarting the application will result in the loss of all accumulated metric data unless persisted externally.
*   **Precision**: Response time calculations are dependent on the resolution of the system clock used during the request processing cycle.
