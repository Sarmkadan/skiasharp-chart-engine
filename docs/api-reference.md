# API Reference

Complete reference documentation for the SkiaSharp Chart Engine public API.

## Table of Contents

- [ChartEngine](#chartengine)
- [Chart Models](#chart-models)
- [Services](#services)
- [Configuration](#configuration)
- [Export](#export)
- [Results](#results)
- [Enumerations](#enumerations)

---

## ChartEngine

The main facade providing access to all chart operations.

### Static Methods

#### `ChartEngine.Create()`

```csharp
public static ChartEngine Create()
```

Creates a new `ChartEngine` instance with default configuration.

**Returns**: `ChartEngine` - New engine instance

**Example**:
```csharp
var engine = ChartEngine.Create();
```

#### `ChartEngine.Create(Action<IServiceCollection>)`

```csharp
public static ChartEngine Create(Action<IServiceCollection> configureServices)
```

Creates a new `ChartEngine` with custom service configuration.

**Parameters**:
- `configureServices`: Delegate to configure dependency injection container

**Returns**: `ChartEngine` - Configured engine instance

**Example**:
```csharp
var engine = ChartEngine.Create(services =>
{
    services.AddLogging();
    services.AddSkiaSharpChartEngine(options =>
    {
        options.CacheEnabled = true;
        options.MaxConcurrentRenders = 10;
    });
});
```

### Instance Methods

#### Rendering Methods

##### `RenderChart(Chart)`

```csharp
public RenderResult RenderChart(Chart chart)
```

Synchronously renders chart to byte array.

**Parameters**:
- `chart`: Chart to render

**Returns**: `RenderResult` - Rendering result with bytes

**Throws**: 
- `ArgumentNullException` - If chart is null
- Exceptions wrapped in `RenderResult` on error

**Example**:
```csharp
var result = engine.RenderChart(chart);
if (result.IsSuccessful)
{
    File.WriteAllBytes("chart.png", result.Data as byte[]);
}
```

##### `RenderChartAsync(Chart, CancellationToken)`

```csharp
public async Task<RenderResult> RenderChartAsync(
    Chart chart, 
    CancellationToken cancellationToken = default)
```

Asynchronously renders chart to byte array.

**Parameters**:
- `chart`: Chart to render
- `cancellationToken`: Cancellation token (optional)

**Returns**: `Task<RenderResult>` - Rendering result with bytes

**Example**:
```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
var result = await engine.RenderChartAsync(chart, cts.Token);
```

#### Export Methods

##### `ExportChart(Chart, ExportOptions)`

```csharp
public RenderResult ExportChart(Chart chart, ExportOptions options)
```

Synchronously exports chart to file in specified format.

**Parameters**:
- `chart`: Chart to export
- `options`: Export configuration

**Returns**: `RenderResult` - Export result with file path

**Throws**:
- `ArgumentNullException` - If chart or options is null
- `DirectoryNotFoundException` - If output path invalid

**Example**:
```csharp
var options = new ExportOptions("report", ExportFormat.PDF, "/output/");
var result = engine.ExportChart(chart, options);
```

##### `ExportChartAsync(Chart, ExportOptions, CancellationToken)`

```csharp
public async Task<RenderResult> ExportChartAsync(
    Chart chart,
    ExportOptions options,
    CancellationToken cancellationToken = default)
```

Asynchronously exports chart to file.

**Parameters**:
- `chart`: Chart to export
- `options`: Export configuration
- `cancellationToken`: Cancellation token (optional)

**Returns**: `Task<RenderResult>` - Export result with file path

#### Repository Methods

##### `SaveChart(Chart)`

```csharp
public string SaveChart(Chart chart)
```

Saves chart to in-memory repository.

**Parameters**:
- `chart`: Chart to save

**Returns**: `string` - Chart ID

**Example**:
```csharp
var id = engine.SaveChart(chart);
```

##### `SaveChartAsync(Chart, CancellationToken)`

```csharp
public async Task<string> SaveChartAsync(
    Chart chart,
    CancellationToken cancellationToken = default)
```

Asynchronously saves chart to repository.

**Parameters**:
- `chart`: Chart to save
- `cancellationToken`: Cancellation token (optional)

**Returns**: `Task<string>` - Chart ID

##### `GetChart(string)`

```csharp
public Chart? GetChart(string chartId)
```

Retrieves chart from repository by ID.

**Parameters**:
- `chartId`: Chart identifier

**Returns**: `Chart?` - Chart or null if not found

##### `GetChartAsync(string, CancellationToken)`

```csharp
public async Task<Chart?> GetChartAsync(
    string chartId,
    CancellationToken cancellationToken = default)
```

Asynchronously retrieves chart from repository.

**Parameters**:
- `chartId`: Chart identifier
- `cancellationToken`: Cancellation token (optional)

**Returns**: `Task<Chart?>` - Chart or null if not found

##### `UpdateChart(Chart)`

```csharp
public bool UpdateChart(Chart chart)
```

Updates existing chart in repository.

**Parameters**:
- `chart`: Chart with updated data

**Returns**: `bool` - True if successful

##### `UpdateChartAsync(Chart, CancellationToken)`

```csharp
public async Task<bool> UpdateChartAsync(
    Chart chart,
    CancellationToken cancellationToken = default)
```

Asynchronously updates chart in repository.

##### `DeleteChart(string)`

```csharp
public bool DeleteChart(string chartId)
```

Deletes chart from repository.

**Parameters**:
- `chartId`: Chart identifier

**Returns**: `bool` - True if successful

##### `DeleteChartAsync(string, CancellationToken)`

```csharp
public async Task<bool> DeleteChartAsync(
    string chartId,
    CancellationToken cancellationToken = default)
```

Asynchronously deletes chart from repository.

#### Configuration Methods

##### `GetDefaultConfiguration()`

```csharp
public ChartConfiguration GetDefaultConfiguration()
```

Gets default chart configuration.

**Returns**: `ChartConfiguration` - Default configuration instance

##### `GetConfigurationTemplate(ChartType)`

```csharp
public ChartConfiguration GetConfigurationTemplate(ChartType chartType)
```

Gets pre-configured template for chart type.

**Parameters**:
- `chartType`: Type of chart

**Returns**: `ChartConfiguration` - Template configuration

##### `GetSupportedExportFormats()`

```csharp
public IEnumerable<ExportFormat> GetSupportedExportFormats()
```

Gets list of supported export formats.

**Returns**: `IEnumerable<ExportFormat>` - Available formats

##### `PrewarmRenderCache(Chart)`

```csharp
public void PrewarmRenderCache(Chart chart)
```

Pre-renders and caches chart output.

**Parameters**:
- `chart`: Chart to cache

#### Service Access

##### `GetServiceProvider()`

```csharp
public IServiceProvider GetServiceProvider()
```

Gets underlying service provider for advanced usage.

**Returns**: `IServiceProvider` - DI container

---

## Chart Models

### Chart

Main chart container.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | string | Unique identifier |
| `Type` | ChartType | Chart type |
| `Series` | List<ChartSeries> | Data series |
| `Configuration` | ChartConfiguration | Display settings |
| `CreatedAt` | DateTime | Creation timestamp |
| `ModifiedAt` | DateTime? | Last modification |
| `CreatedBy` | string? | Creator identifier |
| `IsTemplate` | bool | Template flag |
| `Tags` | Dictionary<string, object>? | Custom metadata |

#### Methods

##### `AddSeries(ChartSeries)`

```csharp
public void AddSeries(ChartSeries series)
```

Adds data series to chart.

**Parameters**:
- `series`: Series to add

**Throws**:
- `ArgumentNullException` - If series is null
- `InvalidOperationException` - If max series reached

##### `RemoveSeries(int)`

```csharp
public void RemoveSeries(int index)
```

Removes series by index.

**Parameters**:
- `index`: Series index

##### `RemoveSeriesByName(string)`

```csharp
public void RemoveSeriesByName(string name)
```

Removes series by name.

**Parameters**:
- `name`: Series name

##### `GetSeriesByName(string)`

```csharp
public ChartSeries? GetSeriesByName(string name)
```

Gets series by name.

**Parameters**:
- `name`: Series name

**Returns**: `ChartSeries?` - Series or null

##### `GetDataBounds()`

```csharp
public (double minX, double maxX, double minY, double maxY) GetDataBounds()
```

Calculates data value bounds.

**Returns**: Tuple with min/max X and Y values

##### `ValidateForRendering()`

```csharp
public bool ValidateForRendering()
```

Validates chart is ready for rendering.

**Returns**: `bool` - True if valid

**Throws**: `InvalidChartDataException` - If validation fails

##### `Clone()`

```csharp
public Chart Clone()
```

Creates deep copy of chart.

**Returns**: `Chart` - Cloned chart

### ChartSeries

Single data series within a chart.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | string | Series name |
| `Color` | string | Hex color code |
| `DataPoints` | List<DataPoint> | X/Y coordinates |
| `LineWidth` | float | Line width (pixels) |
| `IsVisible` | bool | Visibility flag |

#### Methods

##### `AddDataPoint(double, double)`

```csharp
public void AddDataPoint(double x, double y)
```

Adds data point to series.

**Parameters**:
- `x`: X coordinate
- `y`: Y coordinate

##### `RemoveDataPoint(int)`

```csharp
public void RemoveDataPoint(int index)
```

Removes data point by index.

##### `ClearDataPoints()`

```csharp
public void ClearDataPoints()
```

Removes all data points.

##### `GetDataPointCount()`

```csharp
public int GetDataPointCount()
```

Gets number of data points.

**Returns**: `int` - Data point count

##### `Clone()`

```csharp
public ChartSeries Clone()
```

Creates copy of series.

**Returns**: `ChartSeries` - Cloned series

### DataPoint

Individual X/Y coordinate pair.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `X` | double | X coordinate |
| `Y` | double | Y coordinate |
| `Label` | string? | Optional label |

---

## Configuration

### ChartConfiguration

Chart visual and structural settings.

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Width` | int | 800 | Canvas width (px) |
| `Height` | int | 600 | Canvas height (px) |
| `MarginTop` | int | 50 | Top margin (px) |
| `MarginBottom` | int | 50 | Bottom margin (px) |
| `MarginLeft` | int | 50 | Left margin (px) |
| `MarginRight` | int | 50 | Right margin (px) |
| `Title` | string | "" | Chart title |
| `XAxisLabel` | string | "" | X-axis label |
| `YAxisLabel` | string | "" | Y-axis label |
| `BackgroundColor` | string | "#FFFFFF" | Background color (hex) |
| `AxisColor` | string | "#000000" | Axis color (hex) |
| `GridColor` | string | "#E0E0E0" | Grid color (hex) |
| `TextColor` | string | "#000000" | Text color (hex) |
| `ShowGrid` | bool | true | Show grid lines |
| `ShowLegend` | bool | true | Show legend |
| `ShowAxisLabels` | bool | true | Show axis labels |
| `AntiAlias` | bool | true | Anti-aliasing |

#### Methods

##### `Validate()`

```csharp
public void Validate()
```

Validates configuration values.

**Throws**: `InvalidOperationException` - If invalid

---

## Export

### ExportOptions

Controls export behavior and destination.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `FileName` | string | Output filename |
| `Format` | ExportFormat | Output format |
| `OutputPath` | string? | Destination directory |

#### Methods

##### `Constructor`

```csharp
public ExportOptions(string fileName, ExportFormat format, string? outputPath = null)
```

**Parameters**:
- `fileName`: Filename without extension
- `format`: Export format
- `outputPath`: Optional output directory

##### `GetFullPath()`

```csharp
public string GetFullPath()
```

Gets complete output file path.

**Returns**: `string` - Full path with extension

---

## Results

### RenderResult

Contains operation result with metrics.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsSuccessful` | bool | Operation success |
| `ChartId` | string | Chart identifier |
| `Data` | object? | Result data (bytes or path) |
| `RenderTimeMs` | long | Render time (ms) |
| `Format` | ExportFormat | Output format |
| `ErrorMessage` | string? | Error message if failed |
| `Exception` | Exception? | Exception if occurred |

#### Static Methods

##### `CreateSuccess(...)`

```csharp
public static RenderResult CreateSuccess(
    string chartId,
    object data,
    long renderTimeMs,
    ExportFormat format)
```

Creates successful result.

##### `CreateFailure(...)`

```csharp
public static RenderResult CreateFailure(
    string chartId,
    string errorMessage,
    Exception? exception = null)
```

Creates failure result.

---

## Enumerations

### ChartType

```csharp
public enum ChartType
{
    LineChart = 0,        // Line chart
    BarChart = 1,         // Horizontal bar chart
    ColumnChart = 2,      // Vertical bar chart
    PieChart = 3,         // Pie chart
    HeatmapChart = 4,     // Heatmap
    AreaChart = 5,        // Area chart
    ScatterChart = 6      // Scatter plot
}
```

### ExportFormat

```csharp
public enum ExportFormat
{
    PNG = 0,     // Portable Network Graphics
    JPEG = 1,    // Joint Photographic Experts Group
    WebP = 2,    // WebP format
    SVG = 3,     // Scalable Vector Graphics
    PDF = 4      // Portable Document Format
}
```

---

## Extension Methods

### ServiceCollectionExtensions

#### `AddSkiaSharpChartEngine(IServiceCollection, Action<ChartEngineOptions>?)`

```csharp
public static IServiceCollection AddSkiaSharpChartEngine(
    this IServiceCollection services,
    Action<ChartEngineOptions>? configureOptions = null)
```

Registers chart engine services.

**Parameters**:
- `services`: Service collection
- `configureOptions`: Optional configuration

**Example**:
```csharp
services.AddSkiaSharpChartEngine(options =>
{
    options.CacheEnabled = true;
    options.CacheDurationSeconds = 300;
    options.MaxConcurrentRenders = 10;
});
```

---

## Exception Types

### InvalidChartDataException

```csharp
public class InvalidChartDataException : Exception
```

Thrown when chart data is invalid.

---

## Common Patterns

### Error Handling

```csharp
var result = engine.RenderChart(chart);

if (!result.IsSuccessful)
{
    _logger.LogError($"Render failed: {result.ErrorMessage}");
    if (result.Exception != null)
        _logger.LogError(result.Exception, "Exception details");
}
```

### Async Pattern

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

try
{
    var result = await engine.RenderChartAsync(chart, cts.Token);
}
catch (OperationCanceledException)
{
    _logger.LogWarning("Render timed out");
}
```

### Batch Processing

```csharp
var charts = GetCharts();
var tasks = charts.Select(c => engine.RenderChartAsync(c));
var results = await Task.WhenAll(tasks);
```

---

For more information, see [Getting Started](getting-started.md) and [Architecture](architecture.md).
