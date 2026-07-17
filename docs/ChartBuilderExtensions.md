# ChartBuilderExtensions
The `ChartBuilderExtensions` class provides a set of extension methods for the `ChartBuilder` class, allowing for a more fluent and flexible way to configure and build charts. These methods enable developers to customize various aspects of chart creation, such as themes, series defaults, data points, custom settings, export formats, and series addition.

## API
* `public static ChartBuilder WithTheme`: Sets the theme for the chart. Parameters: none specified in the method signature, but presumably takes a theme object or identifier. Return value: the `ChartBuilder` instance with the theme applied. Throws: not specified, but may throw if the theme is invalid or cannot be applied.
* `public static ChartBuilder WithSeriesDefaults`: Sets the default settings for series in the chart. Parameters: presumably takes a series defaults object or configuration. Return value: the `ChartBuilder` instance with the series defaults applied. Throws: not specified, but may throw if the series defaults are invalid or cannot be applied.
* `public static ChartBuilder AddDataPoints`: Adds data points to the chart. Parameters: presumably takes a collection of data points or a data point object. Return value: the `ChartBuilder` instance with the data points added. Throws: not specified, but may throw if the data points are invalid or cannot be added.
* `public static ChartBuilder WithCustomSetting`: Applies a custom setting to the chart. Parameters: presumably takes a custom setting object or identifier and a value. Return value: the `ChartBuilder` instance with the custom setting applied. Throws: not specified, but may throw if the custom setting is invalid or cannot be applied.
* `public static ChartBuilder WithExportFormat`: Sets the export format for the chart. Parameters: presumably takes an export format object or identifier. Return value: the `ChartBuilder` instance with the export format applied. Throws: not specified, but may throw if the export format is invalid or cannot be applied.
* `public static ChartBuilder WithExportDPI`: Sets the export DPI (dots per inch) for the chart. Parameters: presumably takes an integer or DPI object. Return value: the `ChartBuilder` instance with the export DPI applied. Throws: not specified, but may throw if the export DPI is invalid or cannot be applied.
* `public static ChartBuilder AddSeries`: Adds a series to the chart. Parameters: presumably takes a series object or configuration (overload 1). Alternatively, takes a series object or configuration and additional parameters (overload 2). Return value: the `ChartBuilder` instance with the series added. Throws: not specified, but may throw if the series is invalid or cannot be added.

## Usage
The following examples demonstrate how to use the `ChartBuilderExtensions` class to create and customize charts:
```csharp
// Example 1: Create a simple chart with a custom theme and series
var chart = new ChartBuilder()
    .WithTheme("dark")
    .AddSeries(new LineSeries { Name = "Series 1" })
    .AddDataPoints(new[] { new DataPoint(1, 10), new DataPoint(2, 20) })
    .Build();

// Example 2: Create a chart with multiple series and custom export settings
var chart2 = new ChartBuilder()
    .WithSeriesDefaults(new SeriesDefaults { StrokeWidth = 2 })
    .AddSeries(new LineSeries { Name = "Series 1" })
    .AddSeries(new BarSeries { Name = "Series 2" })
    .WithExportFormat(ExportFormat.Pdf)
    .WithExportDPI(300)
    .Build();
```

## Notes
When using the `ChartBuilderExtensions` class, consider the following edge cases and thread-safety remarks:
* The `ChartBuilder` instance is not thread-safe, so it should not be shared across multiple threads.
* The `WithTheme`, `WithSeriesDefaults`, `WithCustomSetting`, `WithExportFormat`, and `WithExportDPI` methods may throw exceptions if the provided parameters are invalid or cannot be applied.
* The `AddDataPoints` and `AddSeries` methods may throw exceptions if the provided data points or series are invalid or cannot be added.
* The `Build` method should be called after all configuration and customization methods have been applied to the `ChartBuilder` instance.
