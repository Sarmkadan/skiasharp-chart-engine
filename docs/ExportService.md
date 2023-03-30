# ExportService

The `ExportService` provides functionality to convert rendered chart data into various file formats, leveraging the SkiaSharp engine for high-quality output generation. It serves as the primary interface for exporting visualizations to disk or memory streams in formats such as PNG, JPEG, or PDF, depending on configuration and support.

## API

### ExportService()

Initializes a new instance of the `ExportService` class.

### ExportAsync(object chartData, Stream outputStream, ExportFormat format)

Asynchronously performs an export operation, rendering the provided chart data into the specified format and writing the output to the given stream.

*   **Parameters:**
    *   `chartData`: The data structure or definition representing the chart to be rendered.
    *   `outputStream`: The `Stream` where the exported file data will be written.
    *   `format`: The `ExportFormat` target for the output.
*   **Returns:** A `Task<RenderResult>` indicating the outcome of the asynchronous export process.
*   **Throws:** `NotSupportedException` if the provided `ExportFormat` is not supported. `ArgumentNullException` if any required parameter is null.

### Export(object chartData, Stream outputStream, ExportFormat format)

Synchronously performs an export operation, rendering the provided chart data into the specified format and writing the output to the given stream.

*   **Parameters:**
    *   `chartData`: The data structure or definition representing the chart to be rendered.
    *   `outputStream`: The `Stream` where the exported file data will be written.
    *   `format`: The `ExportFormat` target for the output.
*   **Returns:** A `RenderResult` object representing the outcome of the export.
*   **Throws:** `NotSupportedException` if the provided `ExportFormat` is not supported. `ArgumentNullException` if any required parameter is null.

### SupportsFormat(ExportFormat format)

Determines whether the specified export format is supported by the current instance of the service.

*   **Parameters:**
    *   `format`: The `ExportFormat` to check.
*   **Returns:** `true` if the format is supported; otherwise, `false`.

### GetSupportedFormats()

Retrieves a collection of all export formats currently supported by the service.

*   **Returns:** An `IEnumerable<ExportFormat>` containing the list of supported formats.

## Usage

### Asynchronous Export

```csharp
var exportService = new ExportService();
var chartData = GetMyChartData();
var format = ExportFormat.Png;

using (var fileStream = File.Create("output.png"))
{
    var result = await exportService.ExportAsync(chartData, fileStream, format);
    if (result.Success)
    {
        Console.WriteLine("Chart exported successfully.");
    }
}
```

### Synchronous Check and Export

```csharp
var exportService = new ExportService();
var format = ExportFormat.Pdf;

if (exportService.SupportsFormat(format))
{
    using (var memoryStream = new MemoryStream())
    {
        var result = exportService.Export(myData, memoryStream, format);
        // Process memoryStream content...
    }
}
else
{
    Console.WriteLine($"Format {format} is not supported.");
}
```

## Notes

*   **Thread-Safety**: The `ExportService` is designed to be thread-safe regarding its internal configuration, allowing for shared instances across multiple threads. However, ensure that the provided `Stream` objects are handled appropriately according to standard .NET I/O thread-safety guidelines.
*   **Performance**: `ExportAsync` should be preferred for I/O-bound operations, particularly when exporting large datasets or to slow storage media, to avoid blocking the calling thread. The synchronous `Export` method should be restricted to scenarios where asynchronous execution is not feasible.
*   **Format Availability**: The list of supported formats returned by `GetSupportedFormats` may depend on the underlying SkiaSharp library capabilities and the environment in which the application is running.
