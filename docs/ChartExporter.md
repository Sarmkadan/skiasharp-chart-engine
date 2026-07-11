# ChartExporter

The `ChartExporter` class provides functionality to export processed chart data into structured formats, specifically JSON and CSV, while also facilitating the generation of analytical summaries for the chart data. It serves as a bridge between the internal chart engine data structures and external requirements for reporting or data interchange.

## API

### Constructors

*   `public ChartExporter()`: Initializes a new instance of the `ChartExporter` class.

### Methods

*   `public async Task<string> ExportToJsonAsync()`: Asynchronously exports the current chart data to a JSON-formatted string. Returns a string representing the serialized JSON data.
*   `public async Task<string> ExportToCsvAsync()`: Asynchronously exports the current chart data to a CSV-formatted string. Returns a string representing the formatted CSV data.
*   `public ExportSummary GenerateExportSummary()`: Generates an `ExportSummary` object, which contains comprehensive statistics and metadata derived from the chart's data points.
*   `public bool IsValidExportFormat()`: Evaluates a provided format identifier to determine if it is a supported export format. Returns `true` if the format is supported, `false` otherwise.

### Properties

*   `public string ChartId { get; }`: Gets the unique identifier assigned to the chart.
*   `public string Title { get; }`: Gets the descriptive title of the chart.
*   `public string ChartType { get; }`: Gets the type identifier representing the chart visualization (e.g., "line", "bar").
*   `public int SeriesCount { get; }`: Gets the total number of data series contained within the chart.
*   `public int TotalDataPoints { get; }`: Gets the total number of data points aggregated across all series.
*   `public double MinValue { get; }`: Gets the minimum numerical value found across all data points in the chart.
*   `public double MaxValue { get; }`: Gets the maximum numerical value found across all data points in the chart.
*   `public DateTime CreatedAt { get; }`: Gets the timestamp indicating when the chart was initially created.
*   `public DateTime ExportedAt { get; }`: Gets the timestamp indicating the most recent export operation performed by this instance.

## Usage

### Exporting Chart Data to JSON

```csharp
var exporter = new ChartExporter();
// ... assume exporter is populated with chart data ...

try
{
    string jsonOutput = await exporter.ExportToJsonAsync();
    Console.WriteLine("Chart data exported to JSON successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Export failed: {ex.Message}");
}
```

### Validating Formats and Generating Summaries

```csharp
var exporter = new ChartExporter();

if (exporter.IsValidExportFormat("csv"))
{
    var summary = exporter.GenerateExportSummary();
    Console.WriteLine($"Chart: {exporter.Title}");
    Console.WriteLine($"Total Points: {summary.DataPointCount}");
    
    string csvData = await exporter.ExportToCsvAsync();
}
```

## Notes

*   **Thread Safety**: The asynchronous methods `ExportToJsonAsync` and `ExportToCsvAsync` are thread-safe, assuming the underlying chart data is not modified during the execution of the export operation. Property access is thread-safe as these properties typically represent a snapshot of the chart state at the time of `ChartExporter` initialization or modification.
*   **Edge Cases**: If `TotalDataPoints` is zero, `ExportToJsonAsync` and `ExportToCsvAsync` may return empty strings or serialized empty structures depending on the specific implementation of the serializer. `MinValue` and `MaxValue` will default to `double.NaN` or similar sentinel values if no data points are present.
*   **Performance**: For charts with a very large `TotalDataPoints` count, `ExportToJsonAsync` and `ExportToCsvAsync` should be awaited to avoid blocking the main thread during serialization.
