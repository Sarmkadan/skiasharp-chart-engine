# RenderMetricsExtensions

`RenderMetricsExtensions` provides convenience methods for interpreting and comparing `RenderMetrics` instances. It translates raw timing and data-point counts into human-readable throughput figures, produces formatted summary strings, applies a configurable threshold to decide whether a render qualifies as “fast,” and supports baseline comparison with a descriptive delta report.

## API

### GetMegabytesPerSecond
```csharp
public static double GetMegabytesPerSecond(this RenderMetrics metrics)
```
**Purpose:** Calculates the effective memory throughput of a render operation in megabytes per second.  
**Parameters:** `metrics` – the `RenderMetrics` instance to evaluate (must not be null).  
**Return value:** Throughput as `double`, derived from the total bytes processed divided by elapsed rendering time.  
**Throws:** `ArgumentNullException` when `metrics` is null; `DivideByZeroException` when the elapsed time is zero.

### GetDataPointsPerSecond
```csharp
public static double GetDataPointsPerSecond(this RenderMetrics metrics)
```
**Purpose:** Calculates the number of data points processed per second during rendering.  
**Parameters:** `metrics` – the `RenderMetrics` instance to evaluate (must not be null).  
**Return value:** Data-point throughput as `double`.  
**Throws:** `ArgumentNullException` when `metrics` is null; `DivideByZeroException` when the elapsed time is zero.

### ToDetailedString
```csharp
public static string ToDetailedString(this RenderMetrics metrics)
```
**Purpose:** Produces a multi-line formatted string summarising all key metrics, including elapsed time, data-point count, memory throughput, and the fast/slow classification.  
**Parameters:** `metrics` – the `RenderMetrics` instance to format (must not be null).  
**Return value:** A `string` containing the formatted summary.  
**Throws:** `ArgumentNullException` when `metrics` is null.

### IsFastRender
```csharp
public static bool IsFastRender(this RenderMetrics metrics)
```
**Purpose:** Determines whether the render meets the predefined “fast” threshold, typically based on data-point throughput or total elapsed time.  
**Parameters:** `metrics` – the `RenderMetrics` instance to evaluate (must not be null).  
**Return value:** `true` if the render is considered fast; otherwise `false`.  
**Throws:** `ArgumentNullException` when `metrics` is null.

### CompareToBaseline
```csharp
public static string CompareToBaseline(this RenderMetrics metrics, RenderMetrics baseline)
```
**Purpose:** Compares the current metrics against a baseline and returns a human-readable delta report (e.g. percentage change in throughput, absolute time difference).  
**Parameters:**  
- `metrics` – the current `RenderMetrics` instance (must not be null).  
- `baseline` – the baseline `RenderMetrics` to compare against (must not be null).  
**Return value:** A `string` describing the comparison, including direction and magnitude of change.  
**Throws:** `ArgumentNullException` if either argument is null; `ArgumentException` if the baseline elapsed time is zero and throughput ratios cannot be computed.

## Usage

### Example 1: Logging render performance after a chart update
```csharp
RenderMetrics metrics = pipeline.RenderChart(chartData);
double mbps = metrics.GetMegabytesPerSecond();
double dps = metrics.GetDataPointsPerSecond();

Console.WriteLine(metrics.ToDetailedString());

if (!metrics.IsFastRender())
{
    logger.Warn("Render below fast threshold – consider simplifying geometry.");
}
```

### Example 2: Comparing current render against a stored baseline
```csharp
RenderMetrics current = engine.Render(chart);
RenderMetrics baseline = baselineStore.Load("chart-v2");

string report = current.CompareToBaseline(baseline);
Console.WriteLine($"Performance delta: {report}");

if (current.GetDataPointsPerSecond() < baseline.GetDataPointsPerSecond() * 0.9)
{
    alertService.RaisePerformanceRegression(report);
}
```

## Notes

- All extension methods guard against null input and will throw `ArgumentNullException` immediately.
- `GetMegabytesPerSecond` and `GetDataPointsPerSecond` assume the underlying `RenderMetrics` contains valid elapsed time greater than zero; a zero elapsed time causes a `DivideByZeroException`.
- `IsFastRender` relies on an internal threshold that may be tuned via configuration; consult the `RenderMetrics` documentation for the exact default.
- `CompareToBaseline` produces a string intended for logs and diagnostics, not for machine parsing. The format may include percentage symbols, directional arrows, or absolute deltas.
- These methods perform arithmetic on `double` values and are not guaranteed to be bitwise deterministic across platforms; minor floating-point differences are expected.
- **Thread safety:** The methods are pure static calculations over immutable (or effectively read-only) `RenderMetrics` instances. They do not mutate state and are safe to call concurrently from multiple threads as long as each call supplies its own `RenderMetrics` instance.
