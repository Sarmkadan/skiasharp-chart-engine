# ExportOptions

`ExportOptions` encapsulates all configurable parameters for rasterizing and exporting chart surfaces to image files. It controls resolution, compression quality, font handling, output paths, and extensible format-specific settings. Instances are validated before use and support cloning for safe reuse across multiple export operations.

## API

### Constructors

- **`ExportOptions()`**  
  Initializes a new instance with default values: `DPI = 96`, `Quality = 1.0f`, `EmbedFonts = false`, `PreserveAspectRatio = true`, `OutputDirectory = null`, `CustomFormatOptions = null`.

- **`ExportOptions(ExportOptions other)`**  
  Copy constructor. Creates a deep clone of the provided `ExportOptions` instance, including a shallow copy of `CustomFormatOptions` if present.  
  *Parameters:* `other` — the source options to copy.  
  *Throws:* `ArgumentNullException` if `other` is `null`.

### Properties

- **`int DPI`**  
  The output resolution in dots per inch. Must be a positive integer. Defaults to `96`.

- **`float Quality`**  
  Compression quality for lossy formats (e.g., JPEG). Valid range is `0.0f` (minimum quality, maximum compression) through `1.0f` (maximum quality, minimum compression). Defaults to `1.0f`.

- **`bool EmbedFonts`**  
  When `true`, embeds the necessary font glyphs into the output file where the target format supports it. Defaults to `false`.

- **`bool PreserveAspectRatio`**  
  When `true`, the exported image maintains the original chart aspect ratio; when `false`, the output dimensions may stretch independently. Defaults to `true`.

- **`string? OutputDirectory`**  
  An optional directory path where exported files are written. If `null`, the current working directory is used. The path is not validated for existence until `Validate()` or `GetFullPath()` is called.

- **`Dictionary<string, object>? CustomFormatOptions`**  
  An extensible dictionary of format-specific key-value pairs (e.g., PNG compression level, TIFF byte order). Keys and value types depend on the target export format. Defaults to `null`.

### Methods

- **`void Validate()`**  
  Validates all property values against their constraints.  
  *Throws:* `ArgumentOutOfRangeException` if `DPI` is less than or equal to zero, or if `Quality` is outside `[0.0f, 1.0f]`. `ArgumentException` if `OutputDirectory` is a non-null, non-empty string that does not represent a valid, existing directory path.

- **`string GetFullPath(string fileName)`**  
  Combines `OutputDirectory` (or the current working directory if `OutputDirectory` is `null`) with the given `fileName` and returns the absolute path.  
  *Parameters:* `fileName` — the file name, including extension, to combine with the output directory.  
  *Returns:* The fully qualified absolute file path.  
  *Throws:* `ArgumentException` if `fileName` is `null` or empty. `InvalidOperationException` if `OutputDirectory` is set but does not exist.

- **`static string GetFileExtension(ExportFormat format)`**  
  Returns the standard file extension (including the leading dot) for the specified `ExportFormat` enum value.  
  *Parameters:* `format` — a member of the `ExportFormat` enum.  
  *Returns:* A string like `".png"`, `".jpg"`, or `".pdf"`.  
  *Throws:* `ArgumentOutOfRangeException` if `format` is not a defined enum value.

- **`ExportOptions Clone()`**  
  Creates and returns a new `ExportOptions` instance that is a memberwise deep copy of the current instance. Equivalent to calling the copy constructor with `this`.  
  *Returns:* A new `ExportOptions` with identical property values.

- **`override string ToString()`**  
  Returns a string representation summarizing key properties (DPI, Quality, output directory, and whether fonts are embedded). Intended for diagnostics and logging.

## Usage

### Example 1: Basic PNG Export with High DPI

```csharp
var options = new ExportOptions
{
    DPI = 300,
    Quality = 1.0f,
    EmbedFonts = true,
    OutputDirectory = @"C:\Charts\Exports"
};

options.Validate();

string extension = ExportOptions.GetFileExtension(ExportFormat.Png);
string fullPath = options.GetFullPath($"sales_report{extension}");

// Pass 'options' to the chart engine's export method
chart.Export(fullPath, ExportFormat.Png, options);
```

### Example 2: Cloning and Overriding for Multiple Outputs

```csharp
var baseOptions = new ExportOptions
{
    DPI = 150,
    Quality = 0.9f,
    PreserveAspectRatio = true,
    OutputDirectory = @".\output"
};

// Export a high-quality JPEG
var jpegOptions = baseOptions.Clone();
jpegOptions.Quality = 0.85f;
string jpegPath = jpegOptions.GetFullPath("chart.jpg");
chart.Export(jpegPath, ExportFormat.Jpeg, jpegOptions);

// Export a lossless PNG with embedded fonts for archival
var pngOptions = baseOptions.Clone();
pngOptions.EmbedFonts = true;
pngOptions.Quality = 1.0f;
string pngPath = pngOptions.GetFullPath("chart.png");
chart.Export(pngPath, ExportFormat.Png, pngOptions);
```

## Notes

- **Validation timing:** `Validate()` is not called automatically by the constructor or property setters. Callers must invoke it explicitly before passing the options to an export operation. The export engine itself may call `Validate()` internally, but relying on this without prior explicit validation can lead to exceptions deep in the export pipeline.
- **Directory existence:** `OutputDirectory` is checked for existence only during `Validate()` or `GetFullPath()`. If the directory is created after validation but before `GetFullPath()`, the call succeeds. Conversely, if the directory is deleted between validation and path resolution, `GetFullPath()` throws `InvalidOperationException`.
- **`CustomFormatOptions` cloning:** The `Clone()` method and copy constructor perform a shallow copy of the `CustomFormatOptions` dictionary. The dictionary reference is duplicated, but the contained `object` values are shared between the original and the clone. Modifying a mutable value object inside the dictionary will affect both instances.
- **Thread safety:** `ExportOptions` is not thread-safe. Mutating properties or calling `Validate()`/`GetFullPath()` concurrently from multiple threads without external synchronization leads to undefined behavior. Cloning an instance and using the clone on a separate thread is safe provided the source instance is not concurrently modified during the clone operation.
- **`GetFileExtension` independence:** The static `GetFileExtension` method does not depend on instance state and is safe to call from any thread at any time.
