# RenderResult

Represents the outcome of a chart rendering operation. It encapsulates both successful results—including the rendered image data, output path, and performance metrics—and failure information such as error messages and exceptions. The type is designed as a discriminated result object that avoids the need for out parameters or exception-driven control flow in rendering pipelines.

## API

### Properties

| Member | Type | Description |
|---|---|---|
| `ChartId` | `string` | Identifier of the chart that was rendered. Set by the factory methods; empty string for the parameterless constructor. |
| `Success` | `bool` | Indicates whether the rendering operation completed without errors. `true` for results created via `CreateSuccess`, `false` for those created via `CreateFailure`. |
| `ImageData` | `byte[]?` | The raw bytes of the rendered image. Populated on success; `null` on failure. |
| `FileSizeBytes` | `long?` | Size of the output file in bytes, if written to disk. `null` when no file was produced or rendering failed. |
| `OutputPath` | `string?` | Absolute or relative path to the output file. `null` when rendering was in-memory only or failed. |
| `ExportFormat` | `ExportFormat?` | The format used for export (e.g., PNG, SVG, PDF). `null` on failure. |
| `RenderedAt` | `DateTime` | UTC timestamp of when the rendering was finalized. |
| `RenderTimeMs` | `long` | Total rendering duration in milliseconds. |
| `ErrorMessage` | `string?` | Human-readable description of the failure. `null` on success. |
| `Exception` | `Exception?` | The exception that caused the failure, if any. `null` on success. |
| `Metadata` | `Dictionary<string, object>?` | Arbitrary key-value pairs attached during rendering (e.g., chart version, data hash). `null` if no metadata was provided. |

### Constructors

#### `RenderResult()`

Parameterless constructor. Initializes `ChartId` to `string.Empty`, `Success` to `false`, `RenderedAt` to `DateTime.MinValue`, and `RenderTimeMs` to `0`. All nullable members remain `null`. Intended primarily for serialization scenarios; prefer the static factory methods for application code.

#### `RenderResult(...)` (internal overloads)

Additional constructor overloads exist for initializing the result with specific values. These are used internally by the factory methods and are not intended for direct consumption.

### Static Factory Methods

#### `CreateSuccess(string chartId, byte[] imageData, long renderTimeMs, ExportFormat exportFormat, string? outputPath = null, long? fileSizeBytes = null, Dictionary<string, object>? metadata = null)`

Creates a `RenderResult` representing a successful rendering.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `chartId` | `string` | Yes | Identifier of the rendered chart. |
| `imageData` | `byte[]` | Yes | Raw bytes of the rendered image. |
| `renderTimeMs` | `long` | Yes | Rendering duration in milliseconds. |
| `exportFormat` | `ExportFormat` | Yes | Format of the exported image. |
| `outputPath` | `string?` | No | File path if written to disk; `null` for in-memory renders. |
| `fileSizeBytes` | `long?` | No | File size on disk; `null` if not applicable. |
| `metadata` | `Dictionary<string, object>?` | No | Optional metadata dictionary. |

**Returns:** A `RenderResult` with `Success = true`, `RenderedAt` set to `DateTime.UtcNow`, and all failure-related members set to `null`.

**Throws:** `ArgumentNullException` if `chartId` or `imageData` is `null`. `ArgumentException` if `chartId` is empty or whitespace.

#### `CreateSuccess(string chartId, string outputPath, long fileSizeBytes, long renderTimeMs, ExportFormat exportFormat, Dictionary<string, object>? metadata = null)`

Overload for file-based results where the raw image bytes are not retained in memory.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `chartId` | `string` | Yes | Identifier of the rendered chart. |
| `outputPath` | `string` | Yes | Path to the output file. |
| `fileSizeBytes` | `long` | Yes | Size of the output file in bytes. |
| `renderTimeMs` | `long` | Yes | Rendering duration in milliseconds. |
| `exportFormat` | `ExportFormat` | Yes | Format of the exported file. |
| `metadata` | `Dictionary<string, object>?` | No | Optional metadata dictionary. |

**Returns:** A `RenderResult` with `Success = true`, `ImageData = null`, and `RenderedAt` set to `DateTime.UtcNow`.

**Throws:** `ArgumentNullException` if `chartId` or `outputPath` is `null`. `ArgumentException` if `chartId` or `outputPath` is empty or whitespace.

#### `CreateFailure(string chartId, string errorMessage, Exception? exception = null, long renderTimeMs = 0, Dictionary<string, object>? metadata = null)`

Creates a `RenderResult` representing a failed rendering.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `chartId` | `string` | Yes | Identifier of the chart that failed to render. |
| `errorMessage` | `string` | Yes | Description of the failure. |
| `exception` | `Exception?` | No | The exception that caused the failure, if available. |
| `renderTimeMs` | `long` | No | Time spent before failure occurred; defaults to `0`. |
| `metadata` | `Dictionary<string, object>?` | No | Optional metadata dictionary. |

**Returns:** A `RenderResult` with `Success = false`, all success-related members set to `null`, and `RenderedAt` set to `DateTime.UtcNow`.

**Throws:** `ArgumentNullException` if `chartId` or `errorMessage` is `null`. `ArgumentException` if `chartId` or `errorMessage` is empty or whitespace.

### Methods

#### `override string ToString()`

Returns a string representation summarizing the result. For successes, includes the `ChartId`, `ExportFormat`, and `RenderTimeMs`. For failures, includes the `ChartId` and `ErrorMessage`. The format is intended for logging and debugging, not for machine parsing.

## Usage

### Example 1: Rendering to a file and handling the result

```csharp
var engine = new ChartRenderEngine();
RenderResult result;

try
{
    result = engine.RenderToFile(
        chartId: "sales-q4-2025",
        chartDefinition: definition,
        outputPath: "/exports/sales-q4.png",
        format: ExportFormat.Png
    );
}
catch (Exception ex)
{
    result = RenderResult.CreateFailure(
        chartId: "sales-q4-2025",
        errorMessage: "Unhandled exception during rendering pipeline.",
        exception: ex
    );
}

if (result.Success)
{
    Console.WriteLine($"Chart rendered to {result.OutputPath} in {result.RenderTimeMs}ms.");
    UploadToCdn(result.OutputPath, result.FileSizeBytes);
}
else
{
    logger.Error("Chart rendering failed for {ChartId}: {ErrorMessage}",
        result.ChartId, result.ErrorMessage, result.Exception);
    NotifyMonitoring(result);
}
```

### Example 2: In-memory rendering with metadata

```csharp
var metadata = new Dictionary<string, object>
{
    ["dataVersion"] = "v3.2",
    ["rowCount"] = 14_500
};

var result = RenderResult.CreateSuccess(
    chartId: "dashboard-sparkline-42",
    imageData: rawPngBytes,
    renderTimeMs: stopwatch.ElapsedMilliseconds,
    exportFormat: ExportFormat.Png,
    metadata: metadata
);

if (result.Success)
{
    var base64 = Convert.ToBase64String(result.ImageData);
    response.SetBody(new { chartId = result.ChartId, image = base64, meta = result.Metadata });
}
```

## Notes

- **Nullability:** Consumers must check `Success` before accessing `ImageData`, `OutputPath`, `FileSizeBytes`, or `ExportFormat`. These members are guaranteed non-null only when `Success` is `true`. Conversely, `ErrorMessage` and `Exception` are non-null only when `Success` is `false`.
- **Immutability:** `RenderResult` instances are effectively immutable after construction. No public setters or mutation methods are exposed. This makes them safe to share across threads without synchronization.
- **Thread safety:** All factory methods are static and produce fully initialized instances. Reading properties from a `RenderResult` on multiple threads is safe; the underlying `Dictionary<string, object>?` in `Metadata` is not cloned, so if callers mutate that dictionary after passing it to a factory method, those mutations will be visible to all holders of the result. Do not modify the metadata dictionary post-creation.
- **Serialization:** The parameterless constructor exists to support deserializers. When deserializing, validate `Success` before interpreting other fields, as a deserialized object may not respect the success/failure field invariants if constructed outside the factory methods.
- **`RenderTimeMs` on failure:** When using `CreateFailure`, the `renderTimeMs` parameter reflects time spent before the failure occurred. A value of `0` is acceptable and indicates either an immediate failure or that timing information was not captured.
- **`DateTime` precision:** `RenderedAt` uses `DateTime.UtcNow`, which has system-dependent precision (typically 10–15 ms on Windows, higher on some Linux configurations). Do not rely on it for sub-millisecond ordering.
