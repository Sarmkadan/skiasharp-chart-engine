# RequestMetricsCollectorExtensions

Provides analysis helpers for `RequestMetricsCollector` in the `SkiaSharpChartEngine.Diagnostics` namespace. The static class adds extension methods that rank endpoints by request volume, error rate, or response time, and that format a plain-text summary of the collected metrics. The helpers read from the collector through its public API (`GetAllEndpointMetrics()` and `GetSystemMetrics()`) and do not change its state.

The source file is `Diagnostics/RequestMetricsCollectorExtensions.cs`.

## Usage

```csharp
using SkiaSharpChartEngine.Diagnostics;

var collector = new RequestMetricsCollector();
collector.RecordRequest("/api/charts/line", 200, 45, 512, 20480);
collector.RecordRequest("/api/charts/pie", 500, 1200, 256, 0);

// Top 5 endpoints by request count
var busiest = collector.GetTopEndpointsByRequests(5);

// Endpoints whose average response time is above 500 ms
var slow = collector.GetEndpointsExceedingResponseTime(500);

// Plain-text report
string report = collector.GetMetricsSummary();
```

## API

### `public static IReadOnlyList<EndpointMetrics> GetTopEndpointsByRequests`

```csharp
public static IReadOnlyList<EndpointMetrics> GetTopEndpointsByRequests(this RequestMetricsCollector collector, int count = 10)
```

Returns the endpoints with the highest request counts.

**Parameters**
- `collector` (`RequestMetricsCollector`, `this`): The metrics collector to query.
- `count` (`int`, optional): Maximum number of endpoints to return. Must be at least 1. Default: `10`.

**Return Value**
- `IReadOnlyList<EndpointMetrics>`: Endpoint metrics ordered by `RequestCount` in descending order. Contains fewer than `count` items when fewer endpoints have been recorded. Returns an empty list when nothing has been recorded.

**Exceptions**
- `ArgumentNullException`: Thrown when `collector` is `null`.
- `ArgumentOutOfRangeException`: Thrown when `count` is less than 1.

---

### `public static IReadOnlyList<EndpointMetrics> GetEndpointsWithHighestErrorRates`

```csharp
public static IReadOnlyList<EndpointMetrics> GetEndpointsWithHighestErrorRates(this RequestMetricsCollector collector, int count = 10)
```

Returns the endpoints with the lowest success rates, which are the endpoints with the highest error rates.

**Parameters**
- `collector` (`RequestMetricsCollector`, `this`): The metrics collector to query.
- `count` (`int`, optional): Maximum number of endpoints to return. Must be at least 1. Default: `10`.

**Return Value**
- `IReadOnlyList<EndpointMetrics>`: Endpoint metrics ordered by `SuccessRate` in ascending order, so the worst endpoints come first. Endpoints with no recorded requests are excluded.

**Exceptions**
- `ArgumentNullException`: Thrown when `collector` is `null`.
- `ArgumentOutOfRangeException`: Thrown when `count` is less than 1.

**Notes**
- An endpoint with 100% success is still returned if it is among the lowest success rates. The method does not filter by a minimum error rate. Filter the result yourself if you only want endpoints with `ErrorCount > 0`.

---

### `public static IReadOnlyList<EndpointMetrics> GetEndpointsExceedingResponseTime`

```csharp
public static IReadOnlyList<EndpointMetrics> GetEndpointsExceedingResponseTime(this RequestMetricsCollector collector, long thresholdMs)
```

Returns every endpoint whose average response time is strictly greater than the threshold.

**Parameters**
- `collector` (`RequestMetricsCollector`, `this`): The metrics collector to query.
- `thresholdMs` (`long`): Response time threshold in milliseconds. Must be greater than zero.

**Return Value**
- `IReadOnlyList<EndpointMetrics>`: Endpoints where `AverageResponseTimeMs > thresholdMs`, ordered by `AverageResponseTimeMs` in descending order. An endpoint whose average equals the threshold is not included.

**Exceptions**
- `ArgumentNullException`: Thrown when `collector` is `null`.
- `ArgumentOutOfRangeException`: Thrown when `thresholdMs` is zero or negative.

**Notes**
- The comparison uses the average response time, not the P95 or P99 values. To check tail latency, compare `P95ResponseTimeMs` or `P99ResponseTimeMs` on the returned items yourself.

---

### `public static string GetMetricsSummary`

```csharp
public static string GetMetricsSummary(this RequestMetricsCollector collector, CultureInfo? cultureInfo = null)
```

Builds a human-readable, multi-line report of system-wide metrics and the top five endpoints by request volume.

**Parameters**
- `collector` (`RequestMetricsCollector`, `this`): The metrics collector to summarize.
- `cultureInfo` (`CultureInfo?`, optional): Culture used to format numbers. Default: `CultureInfo.InvariantCulture`.

**Return Value**
- `string`: The report text. Lines are separated by `\n`.

**Exceptions**
- `ArgumentNullException`: Thrown when `collector` is `null`.

**Output format**

The report always starts with the system section. The endpoint section is added only when at least one endpoint has been recorded.

```
=== Request Metrics Summary ===
Period: 12.5 minutes
Endpoints: 2
Total Requests: 1,000
Successful: 950 (95.0%)
Errors: 50
Avg Response Time: 120 ms
Min/Max Response Time: 45/1200 ms

=== Top 5 Endpoints by Request Volume ===
/api/charts/line: 600 requests, 98.0% success, 45ms avg
/api/charts/pie: 400 requests, 90.0% success, 1200ms avg
```

The values in this example are illustrative.

**Field sources**
- `Period` is `SystemMetrics.SamplePeriod`, the time between the earliest `FirstSeenAt` and the latest `LastSeenAt` across endpoints, shown in minutes with one decimal place.
- `Avg Response Time`, `Min/Max Response Time` come from `SystemMetrics`. These values are computed from the per-endpoint averages, not from individual requests. `Min` and `Max` are therefore the smallest and largest endpoint averages.
- Endpoint lines come from `EndpointMetrics` for the five busiest endpoints.

**Notes**
- When the collector is empty, the summary still returns with zero counts (`Endpoints: 0`, `Successful: 0 (0.0%)`) and no endpoint section.
- The method calls `GetSystemMetrics()` and `GetTopEndpointsByRequests(5)`. Each one collects metrics for all endpoints, so the method scans the stored data twice. Call it on a schedule, not on every request.

## Behavior Notes

- All methods are pure reads. They do not modify the collector.
- Each method calls `GetAllEndpointMetrics()`, which computes statistics (percentiles, status code distribution) for every tracked endpoint. Cost grows with the number of stored requests, which the collector caps at 10,000 per endpoint.
- Results are snapshots. Requests recorded after the call are not reflected in the returned objects.
- Argument validation runs before any metrics are read. A `null` collector is checked first, then the numeric argument.
- The `RequestMetricsCollector` class and its `EndpointMetrics` and `SystemMetrics` types are defined in `Diagnostics/RequestMetricsCollector.cs`. See [RequestMetricsCollector](RequestMetricsCollector.md) for the collector's own API.
