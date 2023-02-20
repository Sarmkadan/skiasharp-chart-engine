# ExportController

The `ExportController` provides endpoints for exporting charts in various formats from the SkiaSharp Chart Engine. It handles both single-chart and batch rendering operations, returning structured responses that include success status, messages, and binary data or metadata about the exported charts.

## API

### `ExportController`
Public controller class that exposes chart export functionality via HTTP endpoints.

### `async Task<ApiResponse<byte[]>> RenderChartAsync(string chartId, string format, int width, int height, string? theme = null)`
Renders a single chart to the specified format and dimensions.

- **Parameters**:
  - `chartId`: Identifier of the chart to render.
  - `format`: Target export format (e.g., "png", "jpeg", "svg").
  - `width`: Output image width in pixels.
  - `height`: Output image height in pixels.
  - `theme` (optional): Theme identifier to apply during rendering.
- **Returns**: `ApiResponse<byte[]>` containing the rendered chart as a byte array on success, or error details on failure.
- **Throws**: May throw if `chartId` or `format` is invalid, or if rendering fails.

### `async Task<ApiResponse<BatchExportResult>> BatchRenderAsync(List<BatchRenderRequest> requests)`
Renders multiple charts in a single request and returns grouped results.

- **Parameters**:
  - `requests`: List of individual render requests, each specifying `ChartId`, `Format`, `Width`, `Height`, and optional `Theme`.
- **Returns**: `ApiResponse<BatchExportResult>` containing aggregated results, including success/failure counts and per-chart results.
- **Throws**: May throw if any request is malformed or if batch processing fails.

### `ApiResponse<List<ExportFormatInfo>> GetSupportedFormats()`
Retrieves metadata about supported export formats.

- **Returns**: `ApiResponse<List<ExportFormatInfo>>` listing available formats with their MIME types, file extensions, and capabilities.
- **Throws**: Never throws; returns empty list if no formats are supported.

### `int TotalCharts`
Gets the total number of charts processed in the current batch operation.

- **Value**: Non-negative integer representing the total charts in the batch.
- **Thread Safety**: Safe to read concurrently.

### `int SuccessfulRenders`
Gets the number of successfully rendered charts in the current batch operation.

- **Value**: Non-negative integer ≤ `TotalCharts`.
- **Thread Safety**: Safe to read concurrently.

### `int FailedRenders`
Gets the number of failed renders in the current batch operation.

- **Value**: Non-negative integer ≤ `TotalCharts`.
- **Thread Safety**: Safe to read concurrently.

### `List<IndividualRenderResult> RenderResults`
Gets detailed results for each chart in the current batch operation.

- **Value**: List of `IndividualRenderResult` objects, one per chart. May be empty.
- **Thread Safety**: Safe to read concurrently; modifications are not thread-safe.

### `string? ChartId`
Gets the identifier of the chart associated with the current operation (single or batch).

- **Value**: Chart ID string, or `null` if not applicable.
- **Thread Safety**: Safe to read concurrently.

### `bool Success`
Indicates whether the current operation (single or batch) completed successfully.

- **Value**: `true` if all operations succeeded; `false` otherwise.
- **Thread Safety**: Safe to read concurrently.

### `string? Message`
Provides a human-readable status or error message for the current operation.

- **Value**: Status message, or `null` if no message is available.
- **Thread Safety**: Safe to read concurrently.

### `long FileSize`
Gets the size of the exported file in bytes (single-chart rendering only).

- **Value**: Non-negative file size, or `0` if not applicable.
- **Thread Safety**: Safe to read concurrently.

### `string? Format`
Gets the export format used for the current operation.

- **Value**: Format identifier (e.g., "png"), or `null` if not applicable.
- **Thread Safety**: Safe to read concurrently.

### `string? MimeType`
Gets the MIME type of the exported file.

- **Value**: MIME type string (e.g., "image/png"), or `null` if not applicable.
- **Thread Safety**: Safe to read concurrently.

### `string? FileExtension`
Gets the recommended file extension for the exported file.

- **Value**: File extension string (e.g., ".png"), or `null` if not applicable.
- **Thread Safety**: Safe to read concurrently.

## Usage

### Single-Chart Rendering
