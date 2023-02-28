# AsyncDataLoader

A utility class designed to asynchronously load chart data from files or directories, primarily targeting CSV-formatted input. It provides methods to parse chart configurations and data points, enabling deferred loading scenarios common in UI-driven applications.

## API

### `AsyncDataLoader`

Initializes a new instance of the `AsyncDataLoader` class. This constructor has no parameters and prepares the loader for subsequent file or directory operations.

### `async Task<Chart?> LoadChartFromFileAsync`

Asynchronously loads a single chart from a specified file path.

- **Parameters**
  - `filePath` (string): The absolute or relative path to the file containing chart data.
- **Return value**
  - A `Task<Chart?>` that resolves to the deserialized `Chart` instance if successful, or `null` if the file is invalid or inaccessible.
- **Exceptions**
  - Throws `ArgumentNullException` if `filePath` is `null`.
  - Throws `FileNotFoundException` if the file does not exist.
  - Throws `UnauthorizedAccessException` if the caller lacks permissions.
  - Throws `InvalidDataException` if the file content is malformed or incompatible.

### `async Task<List<Chart>> LoadChartsFromDirectoryAsync`

Asynchronously loads all valid chart files from a specified directory.

- **Parameters**
  - `directoryPath` (string): The path to the directory containing chart files.
- **Return value**
  - A `Task<List<Chart>>` containing all successfully parsed charts; the list may be empty if no valid files are found.
- **Exceptions**
  - Throws `ArgumentNullException` if `directoryPath` is `null`.
  - Throws `DirectoryNotFoundException` if the directory does not exist.
  - Throws `UnauthorizedAccessException` if the caller lacks permissions.

### `List<DataPoint> ParseCsvData`

Synchronously parses a CSV-formatted string into a list of data points.

- **Parameters**
  - `csvContent` (string): The CSV content to parse, typically representing chart data points.
- **Return value**
  - A `List<DataPoint>` containing the parsed data points. Returns an empty list if `csvContent` is empty or contains no valid data rows.
- **Exceptions**
  - Throws `ArgumentNullException` if `csvContent` is `null`.

### `bool CanLoadFile`

Determines whether the loader can process a file based on its extension.

- **Parameters**
  - `filePath` (string): The path to the file to evaluate.
- **Return value**
  - `true` if the file has a supported extension (e.g., `.csv`, `.json`); otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `filePath` is `null`.

## Usage

### Example 1: Loading a single chart from a file
