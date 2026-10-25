# IExportService

`IExportService` defines the contract for exporting a `Chart` to an output file or byte payload in the `skiasharp-chart-engine`. It covers rendering and writing in one call (async and sync), checking which `ExportFormat` values are supported, and exporting series data as delimited text (CSV and TSV).

The engine registers `ExportService` as a singleton in `Configuration/ServiceCollectionExtensions.cs`. `ChartEngine` resolves `IExportService` from the service provider in its constructor and forwards its export operations to it. To change how charts are exported, implement this interface and register your implementation in place of `ExportService`.

**Namespace:** `SkiaSharpChartEngine.Services`
**Declaration:** `public interface IExportService`
**Source:** `Services/IExportService.cs`

## API

### Export

### `Task<RenderResult> ExportAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default)`
Renders the chart in the format given by `options.Format` and writes the result to the path described by `options`.
- **Parameters:**
  - `chart` - The chart definition to render and export.
  - `options` - Export configuration, including format, DPI, quality, and output path.
  - `cancellationToken` - Token to cancel the operation.
- **Returns:** A `RenderResult` with the exported data and metadata.

### `RenderResult Export(Chart chart, ExportOptions options)`
Synchronous counterpart of `ExportAsync`, with the same inputs and result.
- **Parameters:**
  - `chart` - The chart definition to render and export.
  - `options` - Export configuration.
- **Returns:** A `RenderResult` with the exported data and metadata.

### Format support

### `bool SupportsFormat(ExportFormat format)`
Checks whether this implementation can export the given format.
- **Parameters:** `format` - The export format to check.
- **Returns:** `true` if the format is supported; otherwise `false`.

### `IEnumerable<ExportFormat> GetSupportedFormats()`
Returns every format this implementation supports.
- **Returns:** An enumerable of supported `ExportFormat` values.

### Delimited series data

### `Task<RenderResult> ExportToCsvAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default)`
Exports the series data of the chart to CSV.
- **Parameters:**
  - `chart` - The chart whose series are exported.
  - `options` - Export configuration, including the output path.
  - `cancellationToken` - Token to cancel the operation.
- **Returns:** A `RenderResult` with the CSV data and metadata.

### `Task<RenderResult> ExportToTsvAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default)`
Exports the series data of the chart to TSV (tab-separated values).
- **Parameters:**
  - `chart` - The chart whose series are exported.
  - `options` - Export configuration, including the output path.
  - `cancellationToken` - Token to cancel the operation.
- **Returns:** A `RenderResult` with the TSV data and metadata.

### `RenderResult ExportToCsv(Chart chart, ExportOptions options)`
Synchronous counterpart of `ExportToCsvAsync`.
- **Parameters:**
  - `chart` - The chart whose series are exported.
  - `options` - Export configuration.
- **Returns:** A `RenderResult` with the CSV data.

### `RenderResult ExportToTsv(Chart chart, ExportOptions options)`
Synchronous counterpart of `ExportToTsvAsync`.
- **Parameters:**
  - `chart` - The chart whose series are exported.
  - `options` - Export configuration.
- **Returns:** A `RenderResult` with the TSV data.

## Implementation notes

`ExportService` (`Services/ExportService.cs`) is the built-in implementation. Its behavior, as read from the source:

- **Supported formats:** `PNG`, `SVG`, `JPEG`, `WEBP`, `CSV`, and `TSV`. The interface summary mentions PDF, but `ExportService` does not list PDF as supported, so `SupportsFormat(ExportFormat.PDF)` returns `false` for it.
- **Unsupported formats:** `ExportAsync` and `Export` throw `UnsupportedExportFormatException` when `SupportsFormat(options.Format)` is `false`.
- **Null arguments:** `Export` throws `ArgumentNullException` for a `null` `chart` or `options`.
- **Delimiters:** the CSV and TSV methods use an internal delimiter helper and throw `ArgumentException` when the delimiter is `null` or whitespace.
- **Async errors:** `ExportAsync` lets `ArgumentException` and `UnsupportedExportFormatException` propagate. Other exceptions raised during export are logged and returned as a failed `RenderResult` (via `RenderResult.CreateFailure`) instead of being thrown.
- **Logging:** each export is logged through `ILogger<ExportService>`, including the chart ID, format, and output path.
