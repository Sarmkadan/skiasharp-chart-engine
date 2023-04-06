# ChartBuilder

The `ChartBuilder` class provides a fluent API for configuring, styling, and populating chart visualizations within the `skiasharp-chart-engine`. It facilitates the incremental construction of complex chart objects, allowing for detailed customization of layout, axis parameters, and animation settings before final rendering.

## API

*   **`ChartBuilder()`**: Initializes a new instance of the `ChartBuilder` class.
*   **`WithTitle(string title)`**: Sets the chart title.
*   **`WithSubtitle(string subtitle)`**: Sets the chart subtitle.
*   **`WithSize(int width, int height)`**: Sets the pixel dimensions for the rendered chart.
*   **`WithMargins(int left, int top, int right, int bottom)`**: Sets the margins in pixels around the chart area.
*   **`WithAxisLabels(string xAxisLabel, string yAxisLabel)`**: Sets the display labels for the horizontal and vertical axes.
*   **`WithAxisScales(double min, double max)`**: Sets the scale boundaries for the primary axis.
*   **`WithAxisRange(double xMin, double xMax, double yMin, double yMax)`**: Sets the explicit view range for the chart axes.
*   **`WithBackgroundColor(string color)`**: Sets the background color of the chart area, specified as a hex string or named color.
*   **`WithGridColor(string color)`**: Sets the color of the grid lines, specified as a hex string or named color.
*   **`ShowGrid(bool show)`**: Enables or disables the visibility of the chart grid lines.
*   **`ShowLegend(bool show)`**: Enables or disables the display of the chart legend.
*   **`ShowAxisLabels(bool show)`**: Enables or disables the visibility of axis labels.
*   **`ShowDataPointLabels(bool show)`**: Enables or disables the visibility of labels for individual data points.
*   **`EnableAnimation(bool enable)`**: Enables or disables entry animations for the chart.
*   **`WithAnimationDuration(int ms)`**: Sets the duration of the chart animation in milliseconds.
*   **`EnableAntiAliasing(bool enable)`**: Enables or disables anti-aliasing for improved rendering quality.
*   **`AddSeries(string name)`**: Adds a new, empty data series to the chart.
*   **`AddSeriesWithData(string name, IEnumerable<DataPoint> data)`**: Adds a new data series populated with the provided data points.
*   **`AddDataPointToLastSeries(DataPoint point)`**: Appends a data point to the series most recently added to the builder. Throws `InvalidOperationException` if no series has been defined.

## Usage

### Basic Chart Configuration
```csharp
var chart = new ChartBuilder()
    .WithSize(800, 600)
    .WithTitle("Quarterly Revenue")
    .WithBackgroundColor("#FFFFFF")
    .ShowGrid(true)
    .EnableAntiAliasing(true)
    .AddSeries("Q1 Data")
    .AddDataPointToLastSeries(new DataPoint(1, 100))
    .AddDataPointToLastSeries(new DataPoint(2, 150));
```

### Fluent Series Population
```csharp
var data = new List<DataPoint> { new DataPoint(0, 10), new DataPoint(1, 20) };
var chart = new ChartBuilder()
    .WithTitle("Sensor Readings")
    .AddSeriesWithData("Primary Sensor", data)
    .ShowLegend(true)
    .EnableAnimation(true)
    .WithAnimationDuration(500);
```

## Notes

*   **Thread Safety**: The `ChartBuilder` is not thread-safe. Instances are intended to be used by a single thread during the configuration phase.
*   **State Dependency**: Methods that modify specific series, such as `AddDataPointToLastSeries`, require that at least one series has been initialized via `AddSeries` or `AddSeriesWithData`. Attempting to add a point without an existing series will result in an `InvalidOperationException`.
*   **Fluent Interface**: Every configuration method returns the `ChartBuilder` instance to enable method chaining.
*   **Validation**: Input values for dimensions, ranges, and durations are validated upon rendering; invalid configurations (e.g., negative dimensions) may throw exceptions during the build process.
