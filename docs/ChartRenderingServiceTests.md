# ChartRenderingServiceTests

Unit tests for `ChartRenderingService`, verifying correct rendering behavior for charts to byte arrays, files, and export formats with caching, cancellation, and time measurement.

## API

### Constructors
#### `public ChartRenderingServiceTests()`
Initializes a new instance of the test class with default test dependencies.

### Async Methods
#### `public async Task RenderToByteArrayAsync_WithNullChart_ThrowsArgumentNullException()`
Verifies that passing a `null` chart to `RenderToByteArrayAsync` throws an `ArgumentNullException`.

#### `public async Task RenderToByteArrayAsync_WithCachedResult_ReturnsCachedData()`
Ensures that when a chart has been previously rendered, subsequent calls return the cached byte array instead of re-rendering.

#### `public async Task RenderToByteArrayAsync_WithoutCachedResult_RendersAndCaches()`
Confirms that a chart not previously rendered is processed, the result is cached, and the correct byte array is returned.

#### `public async Task RenderToByteArrayAsync_RecordsRenderTime()`
Checks that rendering a chart records the time taken in the service’s metrics.

#### `public async Task RenderToByteArrayAsync_WithCancellation_StopsOperation()`
Validates that a cancellation request during rendering halts the operation and throws `OperationCanceledException`.

### Sync Methods
#### `public async Task RenderToFileAsync_WithNullChart_ThrowsArgumentNullException()`
Ensures that passing a `null` chart to `RenderToFileAsync` throws an `ArgumentNullException`.

#### `public async Task RenderToFileAsync_WithNullPath_ThrowsArgumentNullException()`
Confirms that passing a `null` file path to `RenderToFileAsync` throws an `ArgumentNullException`.

#### `public async Task RenderToFileAsync_WithEmptyPath_ThrowsArgumentNullException()`
Validates that passing an empty file path to `RenderToFileAsync` throws an `ArgumentNullException`.

#### `public async Task RenderToFileAsync_CreatesDirectoryIfNotExists()`
Checks that the service creates the target directory if it does not exist before writing the image file.

#### `public async Task RenderToFileAsync_WritesImageDataToFile()`
Ensures that the rendered image data is correctly written to the specified file path.

#### `public async Task RenderWithExportAsync_WithNullChart_ThrowsArgumentNullException()`
Verifies that passing a `null` chart to `RenderWithExportAsync` throws an `ArgumentNullException`.

#### `public async Task RenderWithExportAsync_WithNullOptions_ThrowsArgumentNullException()`
Ensures that passing `null` export options to `RenderWithExportAsync` throws an `ArgumentNullException`.

#### `public async Task RenderWithExportAsync_WithSvgFormat_WritesSvgText()`
Confirms that when exporting to SVG format, the service returns a non-empty SVG text string.

#### `public async Task RenderWithExportAsync_WithPngFormat_WritesPngBytes()`
Validates that when exporting to PNG format, the service returns a non-empty byte array representing the PNG image.

### Synchronous Test Methods
#### `public void RenderToByteArray_WithNullChart_ThrowsArgumentNullException()`
Verifies that passing a `null` chart to the synchronous `RenderToByteArray` method throws an `ArgumentNullException`.

#### `public void RenderToByteArray_WithValidChart_ReturnsNonEmptyByteArray()`
Ensures that a valid chart is rendered and the returned byte array is non-empty.

#### `public void RenderToFile_WithNullChart_ThrowsArgumentNullException()`
Confirms that passing a `null` chart to the synchronous `RenderToFile` method throws an `ArgumentNullException`.

#### `public void RenderToFile_WithNullPath_ThrowsArgumentNullException()`
Validates that passing a `null` file path to the synchronous `RenderToFile` method throws an `ArgumentNullException`.

#### `public void RenderToFile_WritesFileSuccessfully()`
Ensures that the synchronous `RenderToFile` method successfully writes the image file to the specified path.

## Usage

### Example 1: Basic Rendering and Export
