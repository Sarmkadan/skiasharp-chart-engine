# ExportOptionsExtensions

The `ExportOptionsExtensions` class provides a set of static extension methods and helper utilities designed to configure and manipulate `ExportOptions` instances within the SkiaSharp Chart Engine. It facilitates a fluent API for defining export parameters such as resolution, file format, and output location, while also offering predefined configurations for common scenarios like high-quality PNGs or vector SVGs. Additionally, it includes utility functions for resolving file paths and determining format characteristics (raster vs. vector).

## API

### WithDPI
Configures the dots per inch (DPI) setting for the export operation.
*   **Parameters**: `this ExportOptions options`, `int dpi`
*   **Returns**: `ExportOptions` – The modified options instance to allow method chaining.
*   **Throws**: No specific exceptions documented; invalid DPI values may be validated by the underlying engine during execution.

### WithQuality
Sets the compression quality level, typically used for lossy formats like JPEG.
*   **Parameters**: `this ExportOptions options`, `int quality`
*   **Returns**: `ExportOptions` – The modified options instance.
*   **Throws**: No specific exceptions documented; quality values outside the valid range (e.g., 0-100) may result in runtime errors during rendering.

### WithOutputDirectory
Specifies the target directory path where the exported file will be saved.
*   **Parameters**: `this ExportOptions options`, `string directory`
*   **Returns**: `ExportOptions` – The modified options instance.
*   **Throws**: May throw `ArgumentException` if the directory path is null, empty, or contains invalid characters.

### WithFilename
Defines the base filename for the exported chart, excluding the extension.
*   **Parameters**: `this ExportOptions options`, `string filename`
*   **Returns**: `ExportOptions` – The modified options instance.
*   **Throws**: May throw `ArgumentException` if the filename contains invalid path characters.

### WithFormat
Explicitly sets the output file format (e.g., PNG, JPEG, SVG, PDF).
*   **Parameters**: `this ExportOptions options`, `ExportFormat format` (or equivalent enum/type)
*   **Returns**: `ExportOptions` – The modified options instance.
*   **Throws**: No specific exceptions documented.

### GetFullPathWithSuffix
Constructs a complete file path by combining the configured output directory, filename, and the appropriate file extension suffix based on the selected format.
*   **Parameters**: `this ExportOptions options`
*   **Returns**: `string` – The fully qualified file path.
*   **Throws**: May throw `InvalidOperationException` if the directory or filename has not been configured.

### IsRasterFormat
Determines whether the currently configured export format is a raster image type (e.g., PNG, JPEG).
*   **Parameters**: `this ExportOptions options`
*   **Returns**: `bool` – `true` if the format is raster; otherwise, `false`.
*   **Throws**: No specific exceptions documented.

### IsVectorFormat
Determines whether the currently configured export format is a vector graphic type (e.g., SVG, PDF).
*   **Parameters**: `this ExportOptions options`
*   **Returns**: `bool` – `true` if the format is vector; otherwise, `false`.
*   **Throws**: No specific exceptions documented.

### ForHighQualityPNG
Returns a new `ExportOptions` instance pre-configured for generating high-resolution PNG images.
*   **Parameters**: None (static method).
*   **Returns**: `ExportOptions` – A new instance with DPI and format settings optimized for PNG.
*   **Throws**: No specific exceptions documented.

### ForWebJPEG
Returns a new `ExportOptions` instance pre-configured for generating JPEG images optimized for web usage.
*   **Parameters**: None (static method).
*   **Returns**: `ExportOptions` – A new instance with quality and format settings optimized for web JPEGs.
*   **Throws**: No specific exceptions documented.

### ForVectorSVG
Returns a new `ExportOptions` instance pre-configured for generating scalable vector graphics (SVG).
*   **Parameters**: None (static method).
*   **Returns**: `ExportOptions` – A new instance with the format set to SVG.
*   **Throws**: No specific exceptions documented.

## Usage

The following example demonstrates configuring a custom export operation using the fluent interface to set specific DPI, quality, and output location parameters.

```csharp
using SkiaSharp.ChartEngine;

// Configure custom export options
var options = new ExportOptions()
    .WithFormat(ExportFormat.Jpeg)
    .WithDPI(300)
    .WithQuality(85)
    .WithOutputDirectory("/var/www/charts")
    .WithFilename("sales_report_2023");

// Resolve the final file path
string fullPath = options.GetFullPathWithSuffix();

// Pass 'options' to the rendering pipeline
// chartRenderer.Export(options);
```

The next example utilizes the predefined static helpers to quickly generate options for specific use cases, such as a high-quality PNG for print or an SVG for interactive web display.

```csharp
using SkiaSharp.ChartEngine;

// Generate options for a high-resolution print asset
var printOptions = ExportOptionsExtensions.ForHighQualityPNG()
    .WithOutputDirectory("./exports/print")
    .WithFilename("quarterly_summary");

// Generate options for a web-ready vector graphic
var webOptions = ExportOptionsExtensions.ForVectorSVG()
    .WithOutputDirectory("./exports/web")
    .WithFilename("interactive_dashboard");

// Verify format types before processing
if (webOptions.IsVectorFormat())
{
    // Proceed with vector-specific rendering logic
}
```

## Notes

*   **Immutability and Chaining**: While the extension methods return an `ExportOptions` instance to support fluent chaining, consumers should verify whether the underlying `ExportOptions` type is mutable or if these methods return new instances. Standard practice in this engine suggests the methods modify the state of the provided instance and return it for convenience.
*   **Path Resolution**: The `GetFullPathWithSuffix` method relies on both `WithOutputDirectory` and `WithFilename` being called prior to invocation. Calling this method without setting these properties may result in an `InvalidOperationException` or an invalid path string.
*   **Thread Safety**: As a static class containing only stateless extension methods and factory methods, `ExportOptionsExtensions` is inherently thread-safe. However, the `ExportOptions` instances they manipulate are not guaranteed to be thread-safe; concurrent modifications to a single `ExportOptions` instance from multiple threads should be avoided.
*   **Format Detection**: The `IsRasterFormat` and `IsVectorFormat` methods are mutually exclusive for standard supported formats. If an unknown or custom format is introduced, the behavior of these predicates depends on the internal implementation of the `ExportFormat` enumeration logic.
