# ChartDataService

The `ChartDataService` provides essential functionality for validating, preparing, and manipulating data structures within the `skiasharp-chart-engine`. This service acts as an intermediary layer ensuring data integrity through comprehensive validation routines and facilitates necessary data processing tasks, including axis range calculation, data normalization, and functional transformations required to render charts accurately.

## API

### `public ChartDataService()`
Initializes a new instance of the `ChartDataService` class.

### `public void ValidateChart(Chart chart)`
Validates the structural integrity and configuration of a `Chart` object.
- **Parameters:** `chart` - The `Chart` instance to validate.
- **Throws:** `ArgumentNullException` if `chart` is null; `ArgumentException` if the chart structure violates engine constraints.

### `public void ValidateSeries(Series series)`
Validates the configuration of a `Series` object.
- **Parameters:** `series` - The `Series` instance to validate.
- **Throws:** `ArgumentNullException` if `series` is null.

### `public void ValidateDataPoint(DataPoint point)`
Validates a single `DataPoint` instance for numerical correctness.
- **Parameters:** `point` - The `DataPoint` to validate.
- **Throws:** `ArgumentNullException` if `point` is null; `ArgumentException` if data values are invalid (e.g., NaN or Infinity).

### `public async Task<bool> ValidateChartAsync(Chart chart)`
Performs an asynchronous validation of a `Chart` object, suitable for complex validation scenarios involving external data sources.
- **Parameters:** `chart` - The `Chart` instance to validate.
- **Returns:** A `Task<bool>` that completes with `true` if the chart is valid, otherwise `false`.

### `public (double min, double max) CalculateAxisRange(IEnumerable<DataPoint> data)`
Calculates the minimum and maximum values for an axis based on a provided collection of data points.
- **Parameters:** `data` - The `IEnumerable<DataPoint>` to analyze.
- **Returns:** A tuple containing the calculated `min` and `max` values.

### `public void NormalizeDataPoints(List<DataPoint> data, double targetMin, double targetMax)`
Normalizes the values within a list of `DataPoint` objects to fit within a specified target range in-place.
- **Parameters:**
  - `data` - The list of `DataPoint` objects to normalize.
  - `targetMin` - The target minimum value.
  - `targetMax` - The target maximum value.

### `public Chart TransformChartData(Chart inputChart, Func<DataPoint, DataPoint> transformation)`
Creates and returns a new `Chart` instance where all `DataPoint` objects have been transformed by the provided function.
- **Parameters:**
  - `inputChart` - The source `Chart`.
  - `transformation` - A function defining how each `DataPoint` should be transformed.
- **Returns:** A new `Chart` instance with transformed data.

### `public List<DataPoint> FilterDataPoints(IEnumerable<DataPoint> data, Func<DataPoint, bool> predicate)`
Filters a collection of `DataPoint` objects based on a provided predicate.
- **Parameters:**
  - `data` - The source `IEnumerable<DataPoint>`.
  - `predicate` - The condition used to filter points.
- **Returns:** A `List<DataPoint>` containing only the points that satisfy the predicate.

## Usage

### Validation of Chart Data
```csharp
var service = new ChartDataService();
var myChart = new Chart { /* ... configuration ... */ };

try 
{
    service.ValidateChart(myChart);
    // Proceed to rendering
}
catch (ArgumentException ex)
{
    // Handle validation errors
}
```

### Data Filtering and Range Calculation
```csharp
var service = new ChartDataService();
var rawData = new List<DataPoint> { /* ... */ };

// Filter out negative values before calculating range
var filteredData = service.FilterDataPoints(rawData, p => p.Value >= 0);
var (min, max) = service.CalculateAxisRange(filteredData);

Console.WriteLine($"Axis Range: {min} to {max}");
```

## Notes

- **Thread Safety:** The `ChartDataService` is designed to be stateless. Instances can be safely shared across multiple threads, provided that the `Chart`, `Series`, and `DataPoint` objects being passed to methods are not concurrently modified by other threads.
- **Error Handling:** Synchronous validation methods throw exceptions when data integrity is compromised. When using `ValidateChartAsync`, validation failures result in a `false` return value rather than an exception.
- **In-place Operations:** The `NormalizeDataPoints` method modifies the provided `List<DataPoint>` directly. If the original data must be preserved, consider creating a shallow or deep copy of the list before passing it to this method.
