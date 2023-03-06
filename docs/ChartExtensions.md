# ChartExtensions
The `ChartExtensions` class provides a set of static methods for extending and manipulating `Chart` objects. These methods enable various operations such as applying default configurations, retrieving axis bounds, checking for empty charts, and applying palettes. They also support data normalization, series filtering, and conversion to builders for further customization.

## API
* `public static Chart WithDefaultConfiguration`: Applies the default configuration to a chart. Returns the chart with the default configuration applied.
* `public static (double minX, double maxX, double minY, double maxY) GetAxisBounds`: Retrieves the axis bounds of a chart. Returns a tuple containing the minimum and maximum x and y values.
* `public static int GetTotalDataPoints`: Returns the total number of data points in a chart.
* `public static bool IsEmpty`: Checks if a chart is empty. Returns `true` if the chart has no data, `false` otherwise.
* `public static Chart ApplyPalette`: Applies a palette to a chart. Returns the chart with the palette applied.
* `public static Chart NormalizeData`: Normalizes the data in a chart. Returns the chart with normalized data.
* `public static Chart FilterSeries`: Filters the series in a chart. Returns the chart with the filtered series.
* `public static ChartBuilder ToBuilder`: Converts a chart to a builder. Returns a `ChartBuilder` object representing the chart.

## Usage
```csharp
// Example 1: Applying default configuration and retrieving axis bounds
var chart = new Chart();
chart = ChartExtensions.WithDefaultConfiguration(chart);
var axisBounds = ChartExtensions.GetAxisBounds(chart);
Console.WriteLine($"Axis bounds: ({axisBounds.minX}, {axisBounds.maxX}, {axisBounds.minY}, {axisBounds.maxY})");

// Example 2: Normalizing data and filtering series
var chart2 = new Chart();
chart2 = ChartExtensions.NormalizeData(chart2);
chart2 = ChartExtensions.FilterSeries(chart2, series => series.Name == "Series1");
var builder = ChartExtensions.ToBuilder(chart2);
```

## Notes
When using `ChartExtensions`, note that the methods do not modify the original chart objects but instead return new instances with the applied changes. This ensures thread-safety and avoids unintended side effects. However, this also means that the original chart objects remain unchanged, and the returned instances should be used for further operations. Additionally, the `GetAxisBounds` method may throw an exception if the chart has no data or if the axis bounds cannot be determined. The `FilterSeries` method may also throw an exception if the filter criteria are invalid. It is recommended to handle these exceptions accordingly to ensure robust and reliable chart manipulation.
