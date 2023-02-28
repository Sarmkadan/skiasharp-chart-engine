# AsyncDataLoaderExtensions

Provides asynchronous helper methods for loading `Chart` objects from files and directories, with built‑in fallback support and utility functions for identifying valid chart definition files.

## API

### LoadChartFromFileWithFallbackAsync

**Purpose**  
Attempts to load a single `Chart` from a primary file; if that fails, tries a fallback file. Returns `null` if both attempts fail.

**Parameters**  
- `primaryFilePath` – Path to the primary chart definition file.  
- `fallbackFilePath` – Optional path to a fallback chart definition file; if `null` or empty, no fallback is attempted.  
- `cancellationToken` – Optional token to cancel the operation.

**Return value**  
A `Task<Chart?>` that completes with the loaded `Chart` instance, or `null` when neither file could be loaded.

**Exceptions**  
- `ArgumentNullException` – If `primaryFilePath` is `null`.  
- `IOException` – If an I/O error occurs while reading either file.  
- `InvalidOperationException` – If the file contents cannot be deserialized into a `Chart`.  
- `OperationCanceledException` – If the operation is cancelled via `cancellationToken`.

### LoadChartsFromDirectoryAsync

**Purpose**  
Enumerates all valid chart definition files in a directory and loads them into a list of `Chart` objects.

**Parameters**  
- `directoryPath` – Path to the directory to search.  
- `searchOption` – Specifies whether to search subdirectories (`SearchOption.AllDirectories`) or only the top level (`SearchOption.TopDirectoryOnly`). Defaults to `SearchOption.TopDirectoryOnly`.  
- `cancellationToken` – Optional token to cancel the operation.

**Return value**  
A `Task<List<Chart>>` that completes with a list containing all successfully loaded charts. Files that fail to load are skipped and do not appear in the result list may be empty** if no valid files are found or all files fail to load.

**Exceptions**  
- `ArgumentNullException` – If `directoryPath` is `null`.  
- `DirectoryNotFoundException` – If the directory does not exist.  
- `IOException` – If an I/O error occurs while enumerating files or reading a file.  
- `OperationCanceledException` – If the operation is cancelled via `cancellationToken`.

### LoadChartsFromDirectoriesAsync

**Purpose**  
Loads charts from multiple directories, combining the results into a single list.

**Parameters**  
- `directoryPaths` – Enumeration of directory paths to search.  
- `searchOption` – Specifies whether to search subdirectories for each directory; applies uniformly to all directories. Defaults to `SearchOption.TopDirectoryOnly`.  
- `cancellationToken` – Optional token to cancel the operation.

**Return value**  
A `Task<List<Chart>>` that completes with a concatenated list of all charts successfully loaded from the supplied directories.

**Exceptions**  
- `ArgumentNullException` – If `directoryPaths` is `null` or contains a `null` element.  
- `DirectoryNotFoundException` – If any directory in `directoryPaths` does not exist.  
- `IOException` – If an I/O error occurs while processing any directory.  
- `OperationCanceledException` – If the operation is cancelled via `cancellationToken`.

### GetValidFiles

**Purpose**  
Returns a list of file names that match the supported chart definition extensions within a given directory.

**Parameters**  
- `directoryPath` – Path to the directory to scan.  
- `searchOption` – Specifies whether to include subdirectories; defaults to `SearchOption.TopDirectoryOnly`.

**Return value**  
A `List<string>` containing the full paths of files whose extensions are recognized as valid chart definitions (e.g., `.json`, `.csv`). The list is ordered according to the underlying file system enumeration.

**Exceptions**  
- `ArgumentNullException` – If `directoryPath` is `null`.  
- `DirectoryNotFoundException` – If the directory does not exist.  
- `IOException` – If an I/O error occurs while enumerating the directory.

## Usage

```csharp
using SkiasharpChartEngine.IO;
using System.Threading.Tasks;

// Example 1: Load a chart with a fallback file.
async Task<Chart?> LoadSampleChartAsync()
{
    string primary = @"charts\sample.json";
    string fallback = @"charts\sample.json";

    return await AsyncDataLoaderExtensions.LoadChartFromFileWithFallbackAsync(
        primaryFilePath: primary,
        fallbackFilePath: fallback);
}

// Example 2: Load all charts from a set of directories.
async Task<List<Chart>> LoadAllChartsAsync()
{
    string[] dirs = { @"charts\set1", @"charts\set2", @"charts\set3" };

    return await AsyncDataLoaderExtensions.LoadChartsFromDirectoriesAsync(
        directoryPaths: dirs,
        searchOption: SearchOption.AllDirectories);
}
```

## Notes

- The extension methods are stateless; they perform only I/O operations and do not retain any internal data between calls. Consequently, they are safe to invoke concurrently from multiple threads, although concurrent access to the same file system resources may produce race conditions that callers must manage externally (e.g., by ensuring exclusive access when writing to the same files while reading).
- `GetValidFiles` returns a newly allocated list on each invocation; modifications to the returned list do not affect subsequent calls.
- If a file fails to parse, the loading methods skip that file rather than aborting the entire operation, which permits partial success when loading batches of charts.
- Supplying `null` for required path arguments results in an `ArgumentNullException`; callers should validate inputs prior to invoking the methods.
- Cancellation is cooperative: passing a `CancellationToken` that is already triggered will cause the method to throw `OperationCanceledException` before performing any work.
