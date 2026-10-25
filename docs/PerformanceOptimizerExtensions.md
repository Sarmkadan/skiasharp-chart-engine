# PerformanceOptimizerExtensions

Extension methods for `PerformanceOptimizer` that add a quick "should I optimize?" check, a one-line analysis summary, and a downsampling helper that is safe to call when the data is already small. The class lives in the `SkiaSharpChartEngine.Utilities` namespace and is defined in `Utilities/PerformanceOptimizerExtensions.cs`.

For the optimizer itself (`AnalyzeChart`, `Downsample`, `EstimateMemoryUsage`) see [PerformanceOptimizer](PerformanceOptimizer.md).

## API

### IsOptimizationStronglyRecommended

**Purpose**  
Runs `AnalyzeChart` on the chart and reports whether any recommendation has a severity of `High` or `Critical`.

**Parameters**  
- `optimizer` – The `PerformanceOptimizer` instance (the `this` argument).  
- `chart` – The `Chart` to analyze.

**Return value**  
`true` if at least one recommendation has `Severity >= Severity.High`; otherwise `false`. Also returns `false` when `AnalyzeChart` returns `null`, which happens when the optimizer fails internally (it logs the error and swallows the exception).

With the analysis rules currently in `PerformanceOptimizer`, the only `High` recommendation is "Data Size", raised when the chart has more than 10,000 data points in total. In practice this method returns `true` exactly when that threshold is exceeded. Series-count and resolution findings are `Medium` and never affect the result.

**Exceptions**  
- `ArgumentNullException` – If `optimizer` or `chart` is `null`.

**Example**

```csharp
if (optimizer.IsOptimizationStronglyRecommended(chart))
{
    var points = optimizer.SafeDownsample(series.DataPoints, 2000);
}
```

### GetOptimizationSummary

**Purpose**  
Produces a short, human-readable line describing the analysis result and the estimated memory footprint. Intended for logs and diagnostics output, not for parsing.

**Parameters**  
- `optimizer` – The `PerformanceOptimizer` instance (the `this` argument).  
- `chart` – The `Chart` to analyze.

**Return value**  
A `string` in one of two forms:

- When analysis succeeds:  
  `Chart {ChartId} has {N} recommendations. Memory estimate: {KB} KB.`  
  `{KB}` is `EstimateMemoryUsage(chart) / 1024` using integer division, so fractions of a kilobyte are dropped.
- When `AnalyzeChart` returns `null`:  
  `No analysis available.`

If `EstimateMemoryUsage` fails internally it returns `0`, so the summary then reads `Memory estimate: 0 KB.`

**Exceptions**  
- `ArgumentNullException` – If `optimizer` or `chart` is `null`.

**Example**

```csharp
logger.LogInformation("{Summary}", optimizer.GetOptimizationSummary(chart));
// Example output: Chart 3f2c... has 1 recommendations. Memory estimate: 812 KB.
```

### SafeDownsample

**Purpose**  
Downsamples a list of data points to at most `target` items, but returns the input untouched when it is already at or below that size. This avoids the work of building a new list and avoids the stride arithmetic in `Downsample` for small inputs.

**Parameters**  
- `optimizer` – The `PerformanceOptimizer` instance (the `this` argument).  
- `data` – The data points to downsample. May be `null`.  
- `target` – The desired maximum number of data points. Not validated by this method; see the notes below.

**Return value**  
A `List<DataPoint>`:

- If `data` is `null`, a new empty list.  
- If `data.Count <= target`, the **same list instance** that was passed in (no copy is made).  
- Otherwise, the result of `optimizer.Downsample(data, target)`.

**Exceptions**  
- `ArgumentNullException` – If `optimizer` is `null`. A `null` `data` argument is not an error; it yields an empty list.

**Notes**  
- Because the original list is returned as-is when no reduction is needed, mutating the result also mutates the caller's list in that case.  
- When downsampling happens, `Downsample` keeps every *n*-th point, where *n* = ⌈`data.Count` / `target`⌉, always starts at the first point, and appends the last point if the stride skipped it. The result therefore has at most `target + 1` points.  
- `target` is not checked. Passing zero or a negative value while `data` has more items than `target` produces a meaningless stride. `Downsample` catches any exception this causes, logs it, and returns the original list, so the caller gets the full data back rather than an error. Pass a positive value.

**Example**

```csharp
// Returns the same list if it has 500 points or fewer.
List<DataPoint> reduced = optimizer.SafeDownsample(series.DataPoints, 500);
```

## Related

- [PerformanceOptimizer](PerformanceOptimizer.md) – the class these methods extend.  
- [PerformanceMonitor](PerformanceMonitor.md) – runtime measurement, as opposed to static analysis.
