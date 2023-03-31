# CliOptions

The `CliOptions` class serves as the configuration model for the `skiasharp-chart-engine` command-line interface. It encapsulates user-provided command-line arguments, providing a structured representation of application settings, input/output paths, and operational flags, while facilitating argument parsing and standard UI output for the engine.

## API

### Properties

*   **`InputFile`** (`string?`): The file path to the source chart data.
*   **`OutputDirectory`** (`string?`): The directory path where rendered output files will be saved.
*   **`ConfigFile`** (`string?`): The file path to an external configuration file for chart settings.
*   **`ExportFormats`** (`string?`): A comma-separated string representing the desired output formats (e.g., "png,pdf,svg").
*   **`Width`** (`int`): The target width for the rendered charts.
*   **`Height`** (`int`): The target height for the rendered charts.
*   **`Verbosity`** (`string?`): The level of logging verbosity.
*   **`Verbose`** (`bool`): A flag indicating whether verbose logging mode is enabled.
*   **`ShowHelp`** (`bool`): A flag indicating that the user requested the help interface.
*   **`ShowVersion`** (`bool`): A flag indicating that the user requested the application version information.
*   **`ConcurrencyLevel`** (`int`): The maximum number of concurrent operations permitted.
*   **`EnableCache`** (`bool`): A flag indicating if the rendering cache is enabled.
*   **`CacheMaxSizeMb`** (`int`): The maximum allowed size of the rendering cache in megabytes.
*   **`MonitorPerformance`** (`bool`): A flag indicating if performance monitoring is active.
*   **`IsValid`** (`bool`): Indicates if the current configuration meets all requirements for engine execution.

### Methods

*   **`GetExportFormats()`** (`List<string>`): Parses the `ExportFormats` string into a structured list of individual format identifiers.
*   **`GetCacheSizeBytes()`** (`long`): Converts `CacheMaxSizeMb` into bytes for underlying service consumption.
*   **`Parse(string[] args)`** (`static CliOptions`): Parses the provided command-line arguments into a new `CliOptions` instance.
*   **`DisplayHelp()`** (`static void`): Writes the standardized help documentation for the CLI tool to the standard output.

## Usage

### Example 1: Basic Argument Parsing
```csharp
public void RunEngine(string[] args)
{
    var options = CliOptions.Parse(args);
    
    if (options.ShowHelp)
    {
        CliOptions.DisplayHelp();
        return;
    }

    if (!options.IsValid)
    {
        throw new ArgumentException("Invalid configuration provided.");
    }

    // Proceed with engine execution using validated options
    Console.WriteLine($"Processing {options.InputFile} into {options.OutputDirectory}");
}
```

### Example 2: Configuring Engine Services
```csharp
public void ConfigureServices(CliOptions options)
{
    if (options.EnableCache)
    {
        long cacheBytes = options.GetCacheSizeBytes();
        _renderCacheService.Initialize(cacheBytes);
    }
    
    var formats = options.GetExportFormats();
    _exportService.SetAllowedFormats(formats);
}
```

## Notes

*   **Thread Safety**: The `CliOptions` instance itself is a data transfer object and is not inherently thread-safe for modification. It is intended to be populated during the application's initialization phase and accessed as a read-only configuration object thereafter.
*   **Validation**: The `IsValid` property depends on the internal state after `Parse` has been invoked. Ensure that mandatory fields (such as `InputFile` or `OutputDirectory`, depending on specific operational mode) are checked before passing the instance to downstream services.
*   **Edge Cases**: If `ExportFormats` is null or empty, `GetExportFormats` returns an empty list rather than null. `CacheMaxSizeMb` must be a positive integer to be considered valid for `GetCacheSizeBytes`.
