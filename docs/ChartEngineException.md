# ChartEngineException

The `ChartEngineException` class is the base exception type for all errors originating from the skiasharp-chart-engine library. It provides a common hierarchy for chart-related failures, allowing callers to catch a single base type or handle specific derived exceptions. Each derived exception represents a distinct category of error, such as invalid input data, rendering failures, unsupported export formats, invalid configuration, or missing resources. The base class exposes an `ErrorCode` property for programmatic error identification.

## API

### ChartEngineException

The root exception class.

| Constructor | Description |
|---|---|
| `ChartEngineException()` | Initializes a new instance with no message. |
| `ChartEngineException(string message)` | Initializes a new instance with a specified error message. |
| `ChartEngineException(string message, Exception innerException)` | Initializes a new instance with a message and a reference to the inner exception that caused this error. |

| Property | Type | Description |
|---|---|---|
| `ErrorCode` | `int` | Gets or sets a numeric error code that can be used for programmatic handling of the exception. |

### InvalidChartDataException

Thrown when chart input data is malformed, out of range, or otherwise invalid.

| Constructor | Description |
|---|---|
| `InvalidChartDataException(string message)` | Initializes a new instance with a specified error message. |
| `InvalidChartDataException(string message, Exception innerException)` | Initializes a new instance with a message and a reference to the inner exception. |

This class does not introduce additional public members beyond those inherited from `ChartEngineException`.

### ChartRenderingException

Thrown when the chart rendering pipeline encounters an unrecoverable error (e.g., GPU resource failure, invalid shader state).

| Constructor | Description |
|---|---|
| `ChartRenderingException(string message)` | Initializes a new instance with a specified error message. |
| `ChartRenderingException(string message, Exception innerException)` | Initializes a new instance with a message and a reference to the inner exception. |

This class does not introduce additional public members beyond those inherited from `ChartEngineException`.

### UnsupportedExportFormatException

Thrown when an attempt is made to export a chart to a format that is not supported by the engine.

| Constructor | Description |
|---|---|
| `UnsupportedExportFormatException(string format)` | Initializes a new instance with the unsupported format string. The message is automatically generated to include the format value. |

| Property | Type | Description |
|---|---|---|
| `Format` | `string` | Gets the unsupported export format that caused the exception. |

### InvalidConfigurationException

Thrown when the chart engine configuration (e.g., options, settings) contains invalid or conflicting values.

| Constructor | Description |
|---|---|
| `InvalidConfigurationException(string message)` | Initializes a new instance with a specified error message. |

This class does not introduce additional public members beyond those inherited from `ChartEngineException`.

### ResourceNotFoundException

Thrown when a required resource (e.g., font, image, data file) cannot be located or loaded.

| Property | Type | Description |
|---|---|---|
| `ResourceName` | `string` | Gets the name of the resource that was not found. |
| `Identifier` | `string` | Gets an identifier that further describes the missing resource (e.g., a key or path). |

No constructors are publicly documented for this class; instances are typically created internally by the library.

## Usage

The following examples demonstrate catching and inspecting chart engine exceptions.

### Example 1: Catching a specific derived exception

```csharp
using SkiaSharp.ChartEngine;

try
{
    var chart = new Chart();
    chart.Export("output.png", ExportFormat.Pdf); // unsupported format
}
catch (UnsupportedExportFormatException ex)
{
    Console.WriteLine($"Export failed: format '{ex.Format}' is not supported.");
    // Log or fall back to a supported format
}
```

### Example 2: Handling multiple exception types with the base class

```csharp
using SkiaSharp.ChartEngine;

try
{
    var data = LoadChartData();
    var chart = new Chart(data);
    chart.Render();
}
catch (ChartEngineException ex) when (ex.ErrorCode != 0)
{
    Console.WriteLine($"Chart error (code {ex.ErrorCode}): {ex.Message}");
    // Perform recovery based on error code
}
catch (ChartEngineException ex)
{
    Console.WriteLine($"Chart error: {ex.Message}");
}
```

## Notes

- **Edge Cases**  
  - The `ErrorCode` property on the base `ChartEngineException` is not set by default; derived exceptions may or may not assign a meaningful value. Always check for a non‑zero code before relying on it.  
  - `UnsupportedExportFormatException` automatically generates its message from the provided format string. If the format string is `null` or empty, the message may be ambiguous.  
  - `ResourceNotFoundException` exposes both `ResourceName` and `Identifier`. In some cases one of these may be `null` if the corresponding information is unavailable.

- **Thread Safety**  
  Exception types are inherently not thread‑safe for mutation. Once thrown, an exception instance should be treated as immutable. Reading properties like `ErrorCode`, `Format`, `ResourceName`, or `Identifier` from multiple threads concurrently is safe as long as the instance is not being modified (which should never occur after construction). The library does not reuse or cache exception instances; each throw creates a new object.
