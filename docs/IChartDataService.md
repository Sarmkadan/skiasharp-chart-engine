# IChartDataService

`IChartDataService` defines the contract for validating, measuring, and transforming chart data in the `skiasharp-chart-engine`. It covers structural validation of `Chart`, `ChartSeries`, and `DataPoint` objects, computation of axis ranges, normalization of point coordinates, and functional transformation and filtering of data. The engine's default implementation, `ChartDataService`, is registered as a singleton in `Configuration/ServiceCollectionExtensions.cs`. `ChartEngine` and the `DataController` Web API controller resolve the interface from the service provider.

**Namespace:** `SkiaSharpChartEngine.Services`
**Declaration:** `public interface IChartDataService`
**Source:** `Services/IChartDataService.cs`

## API

### Validation

### `void ValidateChart(Chart chart)`
Validates a chart and everything it contains.
- **Parameters:** `chart` - The chart to validate.
- **Behavior:** Requires at least one series, at least one data point across all series, and valid series and points. The chart's configuration is validated as well.
- **Exceptions:** `ArgumentNullException` if `chart` is `null`. `InvalidChartDataException` if any rule is violated.

### `void ValidateSeries(ChartSeries series)`
Validates a single series and its data points.
- **Parameters:** `series` - The series to validate.
- **Behavior:** Requires a non-blank `Name`, no more than `ChartConstants.MaxDataPoints` points, valid points, and a `LineWidth` greater than `0`.
- **Exceptions:** `ArgumentNullException` if `series` is `null`. `InvalidChartDataException` if any rule is violated.

### `void ValidateDataPoint(DataPoint point)`
Validates a single data point.
- **Parameters:** `point` - The point to validate.
- **Behavior:** Rejects `X` or `Y` values that are `NaN` or infinite.
- **Exceptions:** `ArgumentNullException` if `point` is `null`. `InvalidChartDataException` if a coordinate is invalid.

### `Task<bool> ValidateChartAsync(Chart chart, CancellationToken cancellationToken = default)`
Runs `ValidateChart` on a thread-pool thread and reports the outcome as a boolean.
- **Parameters:** `chart` - The chart to validate. `cancellationToken` - Cancels the asynchronous operation.
- **Returns:** `true` if validation passes; `false` if any exception is thrown during validation. The failure is logged and not rethrown.

Because every exception is converted to `false`, callers cannot distinguish a validation failure from, for example, a `null` chart. Use `ValidateChart` when the reason for failure matters.

### Axis Range

### `(double min, double max) CalculateAxisRange(IEnumerable<double> values, AxisScaleType scaleType)`
Computes the minimum and maximum of a set of axis values for the given scale type.
- **Parameters:** `values` - The values to measure. `NaN` and infinite values are ignored. `scaleType` - The `AxisScaleType` that determines how the range is adjusted.
- **Returns:** A tuple `(min, max)`. Returns `(0, 1)` if `values` is `null`, empty, or contains no finite values.

For `AxisScaleType.Logarithmic` the minimum is clamped to at least `1` and the maximum to at least `2`. For `Linear`, `Categorical`, and `DateTimeLinear` the raw minimum and maximum are returned.

### Transformation

### `void NormalizeDataPoints(List<DataPoint> points)`
Rescales point coordinates in place into the range `[0, 1]`.
- **Parameters:** `points` - The points to normalize. The list is modified in place.
- **Behavior:** The X and Y ranges are computed from points where neither coordinate is `NaN`. Each point in the list is then mapped with `(value - min) / range`. A zero-width range is treated as `1` to avoid division by zero. Do nothing if `points` is `null` or empty, or if no point is free of `NaN` coordinates.

Points that contain `NaN` are still included in the rescaling step, so their coordinates remain `NaN` after normalization.

### `Chart TransformChartData(Chart chart, Func<DataPoint, DataPoint> transformer)`
Applies a transformation to every data point of a chart and returns the result as a new chart.
- **Parameters:** `chart` - The source chart. It is cloned before any change is made. `transformer` - A function that receives a copy of each point and returns its replacement.
- **Returns:** A transformed copy of `chart`. The original chart is not modified.
- **Exceptions:** `ArgumentNullException` if `chart` or `transformer` is `null`.

### Filtering

### `List<DataPoint> FilterDataPoints(List<DataPoint> points, Func<DataPoint, bool> predicate)`
Selects the points that satisfy a condition.
- **Parameters:** `points` - The points to filter. `predicate` - The condition each point is tested against.
- **Returns:** A new `List<DataPoint>` containing the matching points, in their original order. The input list is not modified.
- **Exceptions:** `ArgumentNullException` if `points` or `predicate` is `null`.

## Behavior Notes

- Validation methods throw on failure, except `ValidateChartAsync`, which returns a boolean.
- `TransformChartData` returns a copy, while `NormalizeDataPoints` changes its input in place. Check which behavior a call site expects.
- `CalculateAxisRange` never throws for empty or non-finite input. It returns the fallback `(0, 1)` instead, so callers do not need to guard against empty data.
- The interface members carry no XML documentation comments in `Services/IChartDataService.cs`. The descriptions above come from the behavior of `ChartDataService`, the implementation shipped with the engine.

## Usage

### Validate a Chart Before Rendering
```csharp
public void Render(IChartDataService dataService, Chart chart)
{
    dataService.ValidateChart(chart);
    // Proceed with rendering. Invalid data throws InvalidChartDataException.
}
```

### Check Validity Without Exceptions
```csharp
public async Task<bool> IsRenderableAsync(IChartDataService dataService, Chart chart)
{
    return await dataService.ValidateChartAsync(chart);
}
```

### Compute an Axis Range
```csharp
var values = new[] { 3.2, 7.5, double.NaN, 1.1 };
var (min, max) = dataService.CalculateAxisRange(values, AxisScaleType.Linear);
// min = 1.1, max = 7.5
```

### Scale Values to a Unit Interval
```csharp
var points = new List<DataPoint>
{
    new DataPoint { X = 0, Y = 10 },
    new DataPoint { X = 5, Y = 20 },
    new DataPoint { X = 10, Y = 30 }
};

dataService.NormalizeDataPoints(points);
// Every X and Y is now between 0 and 1.
```

### Filter and Transform
```csharp
var positive = dataService.FilterDataPoints(points, p => p.Y > 0);

var doubled = dataService.TransformChartData(chart,
    p => new DataPoint { X = p.X, Y = p.Y * 2 });
```
