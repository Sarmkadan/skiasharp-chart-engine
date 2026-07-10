# ChartConfiguration

`ChartConfiguration` is the central options class for controlling the visual appearance and data scaling of charts rendered by the skiasharp-chart-engine. It provides properties for axis labels, margins, colors, legend and grid visibility, data-point label toggles, and explicit axis range clamping. An instance of this class is passed to the chart renderer to produce a fully styled output.

## API

### Subtitle
`public string? Subtitle`

Gets or sets the subtitle text displayed below the main chart title. When `null` or empty, no subtitle is rendered.

### XAxisLabel
`public string? XAxisLabel`

Gets or sets the label text placed along the X axis. A `null` value suppresses the label.

### YAxisLabel
`public string? YAxisLabel`

Gets or sets the label text placed along the Y axis. A `null` value suppresses the label.

### BackgroundColor
`public string BackgroundColor`

Gets or sets the background color of the entire chart canvas as a string. The value must be a valid color string parseable by the rendering layer (e.g., `"#FFFFFF"` or `"white"`). An invalid string will cause a rendering exception.

### GridColor
`public string GridColor`

Gets or sets the color of the grid lines. Accepts the same color-string format as `BackgroundColor`. Only visible when `ShowGrid` is `true`.

### AxisColor
`public string AxisColor`

Gets or sets the color used for the X and Y axis lines and tick marks. Accepts standard color strings.

### TextColor
`public string TextColor`

Gets or sets the default color for all chart text elements including axis labels, legend entries, data-point labels, and the subtitle. Individual label visibility is governed by their respective `Show*` properties.

### MarginTop
`public int MarginTop`

Gets or sets the top margin in device-independent pixels between the chart boundary and the outermost chart elements. Must be non-negative; negative values are clamped to zero at render time.

### MarginBottom
`public int MarginBottom`

Gets or sets the bottom margin in device-independent pixels. Negative values are clamped to zero.

### MarginLeft
`public int MarginLeft`

Gets or sets the left margin in device-independent pixels. Negative values are clamped to zero.

### MarginRight
`public int MarginRight`

Gets or sets the right margin in device-independent pixels. Negative values are clamped to zero.

### ShowLegend
`public bool ShowLegend`

Gets or sets whether the legend is drawn. When `false`, no legend area is reserved and no series identifiers are displayed.

### ShowGrid
`public bool ShowGrid`

Gets or sets whether horizontal and vertical grid lines are drawn across the plot area. Grid lines use `GridColor`.

### ShowAxisLabels
`public bool ShowAxisLabels`

Gets or sets whether the numeric or categorical labels along the axes are rendered. When `false`, tick marks may still appear depending on the renderer implementation, but no text is drawn beside them.

### ShowDataPointLabels
`public bool ShowDataPointLabels`

Gets or sets whether each data point’s value label is drawn near its marker. Enabling this on dense datasets may cause label overlap; no automatic collision detection is performed.

### XAxisScaleType
`public AxisScaleType XAxisScaleType`

Gets or sets the scale transformation applied to the X axis. `AxisScaleType` is an enum defining options such as `Linear` and `Logarithmic`. Changing this property recalculates the axis tick spacing and range.

### YAxisScaleType
`public AxisScaleType YAxisScaleType`

Gets or sets the scale transformation applied to the Y axis. Behavior mirrors `XAxisScaleType`.

### XAxisMin
`public double? XAxisMin`

Gets or sets an explicit lower bound for the X axis range. When `null`, the minimum is derived automatically from the data. If set, data points below this value are still plotted but may fall outside the visible plot area unless clipping is enabled by the renderer.

### XAxisMax
`public double? XAxisMax`

Gets or sets an explicit upper bound for the X axis range. When `null`, the maximum is derived automatically. Setting a value lower than `XAxisMin` causes the renderer to throw an `ArgumentException` at render time.

### YAxisMin
`public double? YAxisMin`

Gets or sets an explicit lower bound for the Y axis range. When `null`, the minimum is derived automatically. A value greater than `YAxisMax` causes an `ArgumentException` at render time.

## Usage

### Example 1: Basic Styled Line Chart

```csharp
var config = new ChartConfiguration
{
    Subtitle = "Quarterly Revenue",
    XAxisLabel = "Quarter",
    YAxisLabel = "USD (millions)",
    BackgroundColor = "#FAFAFA",
    GridColor = "#E0E0E0",
    AxisColor = "#333333",
    TextColor = "#222222",
    MarginTop = 40,
    MarginBottom = 30,
    MarginLeft = 60,
    MarginRight = 20,
    ShowLegend = true,
    ShowGrid = true,
    ShowAxisLabels = true,
    ShowDataPointLabels = false,
    XAxisScaleType = AxisScaleType.Linear,
    YAxisScaleType = AxisScaleType.Linear
};

var chart = new LineChart(config);
chart.AddSeries(salesData);
chart.Render("revenue_chart.png");
```

### Example 2: Logarithmic Y Axis with Clamped Range

```csharp
var config = new ChartConfiguration
{
    Subtitle = null,
    XAxisLabel = "Time (s)",
    YAxisLabel = "Amplitude",
    BackgroundColor = "#FFFFFF",
    GridColor = "#CCCCCC",
    AxisColor = "#000000",
    TextColor = "#000000",
    MarginTop = 20,
    MarginBottom = 40,
    MarginLeft = 70,
    MarginRight = 15,
    ShowLegend = false,
    ShowGrid = true,
    ShowAxisLabels = true,
    ShowDataPointLabels = true,
    XAxisScaleType = AxisScaleType.Linear,
    YAxisScaleType = AxisScaleType.Logarithmic,
    YAxisMin = 0.1,
    YAxisMax = 1000.0
};

var chart = new ScatterChart(config);
chart.AddSeries(sensorReadings);
chart.Render("amplitude_chart.png");
```

## Notes

- All color properties require strings conforming to the underlying SkiaSharp color-parsing rules. Hexadecimal RGB/RGBA, named colors, and certain CSS-style function strings are typically accepted. An unparseable value causes a render-time exception.
- Margin properties accept negative integers but are clamped to zero internally during layout calculation. No error is raised for negative input.
- Setting `XAxisMin` greater than `XAxisMax` or `YAxisMin` greater than `YAxisMax` is not validated at property-assignment time; the inconsistency is detected when the chart is rendered and results in an `ArgumentException`.
- `ShowDataPointLabels` does not include an automatic layout engine to prevent label collisions. On charts with many overlapping points, labels will overlap and may become illegible.
- `ChartConfiguration` is not thread-safe. Modifying properties while a render operation is in progress on another thread leads to undefined behavior, including race conditions on the nullable axis bounds and color strings. External synchronization is required for concurrent access.
- When both `XAxisMin` and `XAxisMax` are `null`, the axis range is auto-calculated from the data with padding applied by the renderer. The same applies to the Y axis.
