# ChartRenderingService

The `ChartRenderingService` is a service class responsible for rendering chart visualizations to various output formats, including byte arrays and files. It handles both synchronous and asynchronous rendering operations, with support for caching and pre-warming to improve performance.

## API

### `ChartRenderingService`

The default constructor for the `ChartRenderingService` class. Initializes a new instance of the service with default dependencies.

### `async Task<RenderResult> RenderToByteArrayAsync(Chart chart, ChartImageFormat format = ChartImageFormat.Png, int? width = null, int? height = null, CancellationToken cancellationToken = default)`

Asynchronously renders the provided chart to a byte array in the specified image format.

- **Parameters**:
  - `chart`: The chart instance to render.
  - `format`: The target image format (defaults to PNG).
  - `width`: Optional target width for the rendered image.
  - `height`: Optional target height for the rendered image.
  - `cancellationToken`: Optional cancellation token for the async operation.
- **Returns**: A `Task<RenderResult>` that resolves to a `RenderResult` containing the rendered image data and metadata.
- **Throws**:
  - `ArgumentNullException` if `chart` is null.
  - `OperationCanceledException` if the operation is canceled via `cancellationToken`.

### `async Task<RenderResult> RenderToFileAsync(Chart chart, string filePath, ChartImageFormat format = ChartImageFormat.Png, int? width = null, int? height = null, CancellationToken cancellationToken = default)`

Asynchronously renders the provided chart to a file at the specified path in the given image format.

- **Parameters**:
  - `chart`: The chart instance to render.
  - `filePath`: The destination file path for the rendered image.
  - `format`: The target image format (defaults to PNG).
  - `width`: Optional target width for the rendered image.
  - `height`: Optional target height for the rendered image.
  - `cancellationToken`: Optional cancellation token for the async operation.
- **Returns**: A `Task<RenderResult>` that resolves to a `RenderResult` containing metadata about the rendered file.
- **Throws**:
  - `ArgumentNullException` if `chart` or `filePath` is null.
  - `ArgumentException` if `filePath` is an empty string or contains invalid characters.
  - `OperationCanceledException` if the operation is canceled via `cancellationToken`.
  - `IOException` if the file cannot be written to the specified path.

### `async Task<RenderResult> RenderWithExportAsync(Chart chart, ChartImageFormat format = ChartImageFormat.Png, int? width = null, int? height = null, CancellationToken cancellationToken = default)`

Asynchronously renders the provided chart and returns a result that includes both the rendered image data and export metadata.

- **Parameters**:
  - `chart`: The chart instance to render.
  - `format`: The target image format (defaults to PNG).
  - `width`: Optional target width for the rendered image.
  - `height`: Optional target height for the rendered image.
  - `cancellationToken`: Optional cancellation token for the async operation.
- **Returns**: A `Task<RenderResult>` that resolves to a `RenderResult` containing the rendered image data, metadata, and export information.
- **Throws**:
  - `ArgumentNullException` if `chart` is null.
  - `OperationCanceledException` if the operation is canceled via `cancellationToken`.

### `RenderResult RenderToByteArray(Chart chart, ChartImageFormat format = ChartImageFormat.Png, int? width = null, int? height = null)`

Synchronously renders the provided chart to a byte array in the specified image format.

- **Parameters**:
  - `chart`: The chart instance to render.
  - `format`: The target image format (defaults to PNG).
  - `width`: Optional target width for the rendered image.
  - `height`: Optional target height for the rendered image.
- **Returns**: A `RenderResult` containing the rendered image data and metadata.
- **Throws**:
  - `ArgumentNullException` if `chart` is null.
  - `InvalidOperationException` if the rendering operation fails due to internal errors.

### `RenderResult RenderToFile(Chart chart, string filePath, ChartImageFormat format = ChartImageFormat.Png, int? width = null, int? height = null)`

Synchronously renders the provided chart to a file at the specified path in the given image format.

- **Parameters**:
  - `chart`: The chart instance to render.
  - `filePath`: The destination file path for the rendered image.
  - `format`: The target image format (defaults to PNG).
  - `width`: Optional target width for the rendered image.
  - `height`: Optional target height for the rendered image.
- **Returns**: A `RenderResult` containing metadata about the rendered file.
- **Throws**:
  - `ArgumentNullException` if `chart` or `filePath` is null.
  - `ArgumentException` if `filePath` is an empty string or contains invalid characters.
  - `InvalidOperationException` if the rendering operation fails due to internal errors.
  - `IOException` if the file cannot be written to the specified path.

### `void PrewarmCache()`

Pre-warms the internal rendering cache by initializing and caching common rendering configurations. This can improve performance for subsequent rendering operations by reducing cold-start latency.

- **Throws**: No exceptions are documented as thrown by this method.

## Usage

### Example 1: Asynchronous Rendering to Byte Array
