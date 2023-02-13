# ChartEngine

ChartEngine is a core component of the skiasharp-chart-engine library responsible for managing chart lifecycle operations, including rendering, exporting, and configuration management. It provides both synchronous and asynchronous APIs for interacting with chart data and visual representations, leveraging SkiaSharp for rendering capabilities.

## API

### `public ChartEngine`
Default constructor for initializing a new instance of the ChartEngine. Typically used internally by factory methods.

### `public static ChartEngine Create`
Factory method to create a default ChartEngine instance with standard configuration.

### `public static ChartEngine Create`
Overloaded factory method to create a ChartEngine instance with custom configuration parameters.

### `public async Task<RenderResult> RenderChartAsync`
Asynchronously renders a chart to an in-memory image representation. Returns a RenderResult containing the rendered image data. May throw exceptions if the chart is invalid or rendering fails due to resource constraints.

### `public async Task<RenderResult> ExportChartAsync`
Asynchronously exports a chart to a specified format (e.g., PNG, PDF). Returns a RenderResult with exported data. Throws exceptions for unsupported formats or export failures.

### `public RenderResult RenderChart`
Synchronously renders a chart to an in-memory image. Blocks the calling thread until rendering completes. Throws exceptions for invalid charts or rendering errors.

### `public RenderResult ExportChart`
Synchronously exports a chart to a specified format. Blocks until export completes. Throws exceptions for unsupported formats or export failures.

### `public async Task<string> SaveChartAsync`
Asynchronously saves a chart to a file path. Returns the file path of the saved chart. Throws exceptions for invalid paths or write permissions.

### `public string SaveChart`
Synchronously saves a chart to a file. Blocks until the operation completes. Throws exceptions for invalid paths or write failures.

### `public async Task<Chart?> GetChartAsync`
Asynchronously retrieves a chart by identifier. Returns null if the chart does not exist. May throw exceptions for invalid identifiers or data access errors.

### `public Chart? GetChart`
Synchronously retrieves a chart by identifier. Returns null if not found. Throws exceptions for invalid identifiers or data access issues.

### `public async Task<bool> UpdateChartAsync`
Asynchronously updates an existing chart's configuration. Returns true if successful. Throws exceptions for invalid updates or data persistence failures.

### `public bool UpdateChart`
Synchronously updates a chart's configuration. Returns true on success. Throws exceptions for invalid operations or data access errors.

### `public async Task<bool> DeleteChartAsync`
Asynchronously deletes a chart by identifier. Returns true if deletion succeeds. Throws exceptions for invalid identifiers or deletion failures.

### `public bool DeleteChart`
Synchronously deletes a chart by identifier. Returns true on success. Throws exceptions for invalid identifiers or data access issues.

### `public ChartConfiguration GetDefaultConfiguration`
Returns a default ChartConfiguration object for initializing new charts.

### `public ChartConfiguration GetConfigurationTemplate`
Returns a configuration template that can be customized for specific chart types.

### `public IEnumerable<ExportFormat> GetSupportedExportFormats`
Enumerates all supported export formats (e.g., PNG, JPEG, PDF, SVG).

### `public void PrewarmRenderCache`
Preloads rendering resources into cache to improve subsequent render performance. No return value.

### `public IServiceProvider GetServiceProvider`
Provides access to the underlying dependency injection service provider for advanced customization.

## Usage

### Example 1: Rendering a Chart
```csharp
var engine = ChartEngine.Create();
var chart = engine.GetChart("sales-data-2023");
if (chart != null)
{
    var result = await engine.RenderChartAsync(chart);
    // Use result.ImageStream to access rendered image data
}
```

### Example 2: Exporting and Saving a Chart
```csharp
var engine = ChartEngine.Create();
var chart = engine.GetConfigurationTemplate();
chart.Title = "Quarterly Revenue";
engine.UpdateChart(chart);

var exportResult = engine.ExportChart(chart, ExportFormat.Pdf);
await File.WriteAllBytesAsync("revenue-report.pdf", exportResult.Data);
```

## Notes

- Thread Safety: Asynchronous methods are designed for concurrent use but require external synchronization when accessing shared chart instances. Synchronous methods block the calling thread and should not be called concurrently on the same chart without proper locking.
- Null Handling: `GetChart` and `GetChartAsync` return null for non-existent charts rather than throwing exceptions, requiring explicit null checks.
- Exception Scenarios: All methods may throw `InvalidOperationException` for invalid chart states, `NotSupportedException` for unsupported formats, or `IOException` for file access failures.
- Cache Behavior: `PrewarmRenderCache` optimizes performance for repeated renders but consumes additional memory resources.
- Service Provider: `GetServiceProvider` exposes internal dependencies; misuse may lead to unstable application states.
