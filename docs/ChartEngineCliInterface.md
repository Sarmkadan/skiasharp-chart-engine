# ChartEngineCliInterface

A lightweight command-line interface for the SkiaSharp Chart Engine that parses user-provided arguments and coordinates chart generation and export operations.

## API

### `public ChartEngineCliInterface`

Initializes a new instance of the CLI interface with default values for all properties. The object is intended to be configured via property assignments before calling `ExecuteAsync`.

### `public async Task<int> ExecuteAsync()`

Executes the chart generation pipeline using the current property values. The method validates required inputs, processes configuration and data files, renders the chart, and exports it to the specified formats. Returns an integer exit code suitable for console applications (0 for success, non-zero for failure). Throws `InvalidOperationException` if required properties are missing or invalid. Throws `FileNotFoundException` if referenced files do not exist. Throws `IOException` on file access errors.

### `public string? ChartType`

Gets or sets the type of chart to generate (e.g., "line", "bar", "pie"). The value is case-insensitive and must match a supported chart type in the underlying engine. Defaults to `null`, which must be set before calling `ExecuteAsync`.

### `public string? OutputPath`

Gets or sets the directory where generated chart files will be saved. If not specified, defaults to the current working directory. The path must be writable; otherwise, `ExecuteAsync` will throw `UnauthorizedAccessException`.

### `public string? DataFile`

Gets or sets the path to the data file (e.g., CSV, JSON) used to populate the chart. Required for most chart types. If not set or the file is missing, `ExecuteAsync` throws `FileNotFoundException`.

### `public string? ConfigFile`

Gets or sets the path to an optional configuration file (e.g., JSON) that customizes chart appearance and behavior. If not specified, default settings are used. If the file exists but is malformed, `ExecuteAsync` throws `JsonException`.

### `public string? ChartId`

Gets or sets a unique identifier for the chart instance. Used to disambiguate multiple charts in a single run or when referencing charts in configuration. Defaults to `null`; if not set, a default identifier is generated internally.

### `public List<string> ExportFormats`

Gets the list of output formats to generate (e.g., "png", "svg", "pdf"). Defaults to an empty list. At least one format must be specified before calling `ExecuteAsync`, otherwise the method throws `InvalidOperationException`.

## Usage
