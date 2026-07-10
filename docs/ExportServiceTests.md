# ExportServiceTests

Unit tests for the `ExportService` class, verifying correct behavior for chart export operations including format support, argument validation, rendering delegation, error handling, and cancellation.

## API

### Public Constructors

#### `ExportServiceTests`
Initializes a new instance of the `ExportServiceTests` class with required test dependencies.

### Public Methods

#### `ExportAsync_WithNullChart_ThrowsArgumentNullException`
Ensures that passing a `null` chart to `ExportAsync` results in an `ArgumentNullException`.

- **Parameters**: None
- **Returns**: `Task`
- **Throws**: `ArgumentNullException` when the chart parameter is `null`.

#### `ExportAsync_WithNullOptions_ThrowsArgumentNullException`
Ensures that passing `null` export options to `ExportAsync` results in an `ArgumentNullException`.

- **Parameters**: None
- **Returns**: `Task`
- **Throws**: `ArgumentNullException` when the options parameter is `null`.

#### `ExportAsync_WithUnsupportedFormat_ThrowsUnsupportedExportFormatException`
Ensures that attempting to export with an unsupported format throws `UnsupportedExportFormatException`.

- **Parameters**: None
- **Returns**: `Task`
- **Throws**: `UnsupportedExportFormatException` when the format is not supported.

#### `ExportAsync_WithSupportedPngFormat_DelegatesRenderingAndReturnsSuccess`
Verifies that exporting a valid chart to PNG format delegates rendering and returns a successful result.

- **Parameters**: None
- **Returns**: `Task`
- **Throws**: None under normal operation.

#### `ExportAsync_WithSupportedSvgFormat_DelegatesRenderingAndReturnsSuccess`
Verifies that exporting a valid chart to SVG format delegates rendering and returns a successful result.

- **Parameters**: None
- **Returns**: `Task`
- **Throws**: None under normal operation.

#### `ExportAsync_WhenRenderingServiceReturnsFailure_ReturnsFailureResult`
Ensures that when the rendering service returns a failure, the export operation reflects this in the result.

- **Parameters**: None
- **Returns**: `Task`
- **Throws**: None.

#### `ExportAsync_WhenInfrastructureErrorOccurs_ReturnsFailureWithErrorMessage`
Ensures that infrastructure-level errors during export are captured and returned with a descriptive error message.

- **Parameters**: None
- **Returns**: `Task`
- **Throws**: None.

#### `ExportAsync_WithCancellation_RespectsCancellationToken`
Verifies that the export operation respects the provided `CancellationToken` and cancels appropriately.

- **Parameters**: None
- **Returns**: `Task`
- **Throws**: `OperationCanceledException` when cancellation is requested.

#### `Export_WithNullChart_ThrowsArgumentNullException`
Ensures that passing a `null` chart to the synchronous `Export` method results in an `ArgumentNullException`.

- **Parameters**: None
- **Returns**: `void`
- **Throws**: `ArgumentNullException` when the chart parameter is `null`.

#### `Export_WithNullOptions_ThrowsArgumentNullException`
Ensures that passing `null` export options to the synchronous `Export` method results in an `ArgumentNullException`.

- **Parameters**: None
- **Returns**: `void`
- **Throws**: `ArgumentNullException` when the options parameter is `null`.

#### `Export_WithUnsupportedFormat_ThrowsUnsupportedExportFormatException`
Ensures that attempting to export with an unsupported format using the synchronous `Export` method throws `UnsupportedExportFormatException`.

- **Parameters**: None
- **Returns**: `void`
- **Throws**: `UnsupportedExportFormatException` when the format is not supported.

#### `Export_WithValidChartAndFormat_DelegatesAndReturnsSuccess`
Verifies that exporting a valid chart to a supported format using the synchronous `Export` method delegates rendering and returns a successful result.

- **Parameters**: None
- **Returns**: `void`
- **Throws**: None under normal operation.

#### `Export_WhenRenderingServiceThrows_ReturnsFailureResult`
Ensures that when the rendering service throws during the synchronous `Export` operation, the result reflects the failure.

- **Parameters**: None
- **Returns**: `void`
- **Throws**: None.

#### `SupportsFormat_WithPng_ReturnsTrue`
Verifies that the `SupportsFormat` method returns `true` for the PNG format.

- **Parameters**: None
- **Returns**: `void`
- **Throws**: None.

#### `SupportsFormat_WithSvg_ReturnsTrue`
Verifies that the `SupportsFormat` method returns `true` for the SVG format.

- **Parameters**: None
- **Returns**: `void`
- **Throws**: None.

#### `SupportsFormat_WithJpeg_ReturnsTrue`
Verifies that the `SupportsFormat` method returns `true` for the JPEG format.

- **Parameters**: None
- **Returns**: `void`
- **Throws**: None.

#### `SupportsFormat_WithWebp_ReturnsTrue`
Verifies that the `SupportsFormat` method returns `true` for the WebP format.

- **Parameters**: None
- **Returns**: `void`
- **Throws**: None.

#### `SupportsFormat_WithUnsupportedFormat_ReturnsFalse`
Verifies that the `SupportsFormat` method returns `false` for an unsupported format.

- **Parameters**: None
- **Returns**: `void`
- **Throws**: None.

#### `GetSupportedFormats_ReturnsAllFourFormats`
Verifies that the `GetSupportedFormats` method returns a collection containing all four supported formats: PNG, SVG, JPEG, and WebP.

- **Parameters**: None
- **Returns**: `void`
- **Throws**: None.

## Usage

### Example 1: Validating Export with PNG Format
