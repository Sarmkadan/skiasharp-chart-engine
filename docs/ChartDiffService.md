# ChartDiffService

The `ChartDiffService` is responsible for comparing two versions of a chart configuration within the `skiasharp-chart-engine` and identifying the differences between them. It facilitates audit tracking, version comparison, and change reporting by generating structured `ChartDiff` objects that detail individual property modifications.

## API

### `ChartDiffService`

`public class ChartDiffService`

The core service used to perform comparison operations between chart states.

*   **`ChartDiffService()`**
    Initializes a new instance of the `ChartDiffService` class.

*   **`public ChartDiff ComputeDiff(Chart oldChart, Chart newChart)`**
    Compares the state of an old chart configuration against a new one and returns a `ChartDiff` object containing the identified changes.
    *   **Parameters:**
        *   `oldChart`: The original `Chart` object.
        *   `newChart`: The updated `Chart` object.
    *   **Returns:** A `ChartDiff` object detailing the differences found between the two charts.

*   **`public string GenerateDiffReport(ChartDiff diff)`**
    Produces a human-readable string report based on a provided `ChartDiff` object.
    *   **Parameters:**
        *   `diff`: The `ChartDiff` object to be formatted.
    *   **Returns:** A formatted string containing a summary of the property changes.

### `ChartDiff`

The result object returned by `ComputeDiff`, representing the set of differences between two charts.

*   **`public string ChartId`**
    The unique identifier of the chart that was compared.

*   **`public DateTime ComputedAt`**
    The timestamp indicating when the comparison was performed.

*   **`public List<Change> Changes`**
    A list of `Change` objects representing each specific property change identified.

### `Change`

A class representing a single property-level change between two chart versions.

*   **`public string Property`**
    The name of the property that was modified.

*   **`public string OldValue`**
    The value of the property in the original chart configuration.

*   **`public string NewValue`**
    The value of the property in the new chart configuration.

*   **`public DateTime ChangedAt`**
    The timestamp indicating when the change occurred or was recorded.

## Usage

### Generating a Diff Report
```csharp
var service = new ChartDiffService();
Chart oldChart = GetChartById("chart-001");
Chart newChart = GetUpdatedChart("chart-001");

ChartDiff diff = service.ComputeDiff(oldChart, newChart);
string report = service.GenerateDiffReport(diff);

Console.WriteLine(report);
```

### Programmatic Access to Changes
```csharp
var service = new ChartDiffService();
ChartDiff diff = service.ComputeDiff(v1, v2);

foreach (var change in diff.Changes)
{
    Console.WriteLine($"Property '{change.Property}' changed from '{change.OldValue}' to '{change.NewValue}' at {change.ChangedAt}");
}
```

## Notes

*   **Thread Safety:** The `ChartDiffService` implementation is designed to be stateless, allowing a single instance to be shared across multiple threads. The `ChartDiff` and `Change` objects are data transfer objects and are not inherently thread-safe if modified after creation.
*   **Edge Cases:** If `ComputeDiff` is called with identical `Chart` objects, it returns a `ChartDiff` instance with an empty `Changes` list. Providing `null` for either `oldChart` or `newChart` may result in an `ArgumentNullException` depending on the underlying implementation.
*   **Report Generation:** `GenerateDiffReport` relies on the `ChartDiff` object being fully populated. If a null or improperly initialized `ChartDiff` is passed, the method behavior is undefined.
