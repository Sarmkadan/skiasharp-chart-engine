# ChartSeries
The `ChartSeries` type represents a series of data points in a chart, providing properties to customize its appearance and behavior, as well as methods to manage its data points. It is a fundamental component of the `skiasharp-chart-engine` project, allowing developers to create and manipulate chart series in their applications.

## API
### Properties
* `SeriesType`: Gets or sets the type of chart series (e.g., line, bar, scatter).
* `LineWidth`: Gets or sets the width of the line used to draw the series.
* `IsVisible`: Gets or sets a value indicating whether the series is visible.
* `YAxisMin` and `YAxisMax`: Gets or sets the minimum and maximum values of the Y-axis for this series.
* `Description`: Gets or sets a description of the series.
* `ZIndex`: Gets or sets the Z-index of the series, determining its stacking order.
* `CustomProperties`: Gets or sets a dictionary of custom properties associated with the series.

### Constructors
* `ChartSeries()`: Initializes a new instance of the `ChartSeries` class.
* Overloaded constructors: Initializes a new instance of the `ChartSeries` class with various parameters (not specified).

### Methods
* `AddDataPoint`: Adds a single data point to the series.
	+ Parameters: The data point to add.
	+ Return value: None.
	+ Throws: Not specified.
* `AddDataPoints`: Adds multiple data points to the series.
	+ Parameters: The data points to add.
	+ Return value: None.
	+ Throws: Not specified.
* `RemoveDataPoint`: Removes a data point from the series.
	+ Parameters: The data point to remove.
	+ Return value: None.
	+ Throws: Not specified.
* `ClearDataPoints`: Removes all data points from the series.
	+ Parameters: None.
	+ Return value: None.
	+ Throws: Not specified.
* `GetDataPointCount`: Gets the number of data points in the series.
	+ Parameters: None.
	+ Return value: The number of data points.
	+ Throws: Not specified.
* `GetYAxisRange` and `GetXAxisRange`: Gets the minimum and maximum values of the Y-axis and X-axis, respectively, for this series.
	+ Parameters: None.
	+ Return value: A tuple containing the minimum and maximum values.
	+ Throws: Not specified.
* `Clone`: Creates a copy of the series.
	+ Parameters: None.
	+ Return value: A new `ChartSeries` instance that is a copy of the current instance.
	+ Throws: Not specified.

## Usage
```csharp
// Example 1: Creating a simple line chart series
var series = new ChartSeries();
series.SeriesType = ChartType.Line;
series.LineWidth = 2;
series.AddDataPoint(new DataPoint(1, 10));
series.AddDataPoint(new DataPoint(2, 20));
series.AddDataPoint(new DataPoint(3, 30));

// Example 2: Creating a bar chart series with custom properties
var barSeries = new ChartSeries();
barSeries.SeriesType = ChartType.Bar;
barSeries.IsVisible = true;
barSeries.Description = "Sales figures";
barSeries.CustomProperties = new Dictionary<string, object>
{
    { "Category", "Sales" },
    { "Region", "North America" }
};
barSeries.AddDataPoints(new[] { new DataPoint(1, 100), new DataPoint(2, 200), new DataPoint(3, 300) });
```

## Notes
* The `ChartSeries` class is not thread-safe, and its instances should not be accessed or modified concurrently by multiple threads.
* The `CustomProperties` dictionary can be used to store arbitrary data associated with the series, but its contents are not validated or constrained by the `ChartSeries` class.
* The `GetYAxisRange` and `GetXAxisRange` methods may return `null` or empty tuples if the series does not contain any data points or if the axis ranges are not defined.
* The `Clone` method creates a shallow copy of the series, which means that it copies the references to the data points, but not the data points themselves. If the data points are modified after cloning, the changes will be reflected in both the original and cloned series.
