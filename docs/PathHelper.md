# PathHelper

The `PathHelper` class provides a centralized set of static utility methods for robust file system interaction within the `skiasharp-chart-engine`. It simplifies cross-platform path manipulation, filename sanitization, and directory lifecycle management, ensuring that file system operations remain secure, consistent, and predictable when exporting charts or managing temporary workspace assets.

## API

### IsValidPath
Determines whether a provided string represents a valid file system path according to the underlying operating system rules.
* **Parameters**: `string path` (The path string to validate).
* **Returns**: `bool` (True if the path is valid; otherwise, false).

### GetSafeFilename
Sanitizes a string to remove or replace invalid characters, ensuring compatibility with file system naming constraints.
* **Parameters**: `string filename` (The raw filename string).
* **Returns**: `string` (A sanitized version of the filename).

### GetUniqueFilename
Generates a unique file path by appending an incremental numeric suffix if a file already exists at the specified location.
* **Parameters**: `string path` (The target file path).
* **Returns**: `string` (A unique path that does not currently exist).

### EnsureDirectoryExists
Verifies if a directory exists and creates it if it does not.
* **Parameters**: `string path` (The directory path to ensure).
* **Returns**: `bool` (True if the directory exists or was successfully created; false if the operation failed).
* **Throws**: `IOException` or `UnauthorizedAccessException` if directory creation fails due to permissions or I/O errors.

### GetFileExtension
Extracts the file extension from a complete file path.
* **Parameters**: `string path` (The full file path).
* **Returns**: `string` (The file extension, including the leading dot, or an empty string if none is found).

### GetMimeType
Returns the standard MIME type string associated with a file's extension.
* **Parameters**: `string path` (The file path to evaluate).
* **Returns**: `string` (The identified MIME type, or `application/octet-stream` if unrecognized).

### CombinePath
Concatenates multiple path segments using the correct platform-specific directory separator.
* **Parameters**: `params string[] paths` (An array of path segments).
* **Returns**: `string` (The combined path string).

### GetRelativePath
Computes a relative path from one location to another.
* **Parameters**: `string relativeTo` (The base directory), `string path` (The target path).
* **Returns**: `string` (The path to the target relative to the base directory).

### NormalizePath
Standardizes a path string by resolving relative segments (e.g., `..`, `.`) and correcting directory separators.
* **Parameters**: `string path` (The path to normalize).
* **Returns**: `string` (The normalized path).

### CleanupOldFiles
Scans a directory and deletes files that have not been modified since a specified threshold.
* **Parameters**: `string directory` (The directory path), `int daysOld` (The age threshold in days).
* **Returns**: `int` (The number of files successfully deleted).
* **Throws**: `DirectoryNotFoundException` if the specified directory does not exist.

## Usage

### Sanitizing and Creating a Save Path
```csharp
string rawName = "Sales Chart/Report: Q3.png";
string safeName = PathHelper.GetSafeFilename(rawName);
string fullPath = PathHelper.CombinePath("C:\\Exports", safeName);

if (PathHelper.EnsureDirectoryExists("C:\\Exports"))
{
    string uniquePath = PathHelper.GetUniqueFilename(fullPath);
    // Proceed to save file at uniquePath
}
```

### Cleaning Up Exported Reports
```csharp
string reportFolder = "/var/app/reports";
int deletedCount = PathHelper.CleanupOldFiles(reportFolder, 30);
Console.WriteLine($"Removed {deletedCount} expired reports.");
```

## Notes

* **Thread Safety**: All methods in `PathHelper` are static and stateless. They are inherently thread-safe for concurrent calls. However, file system operations (such as `EnsureDirectoryExists` and `CleanupOldFiles`) are subject to race conditions outside the scope of this class if multiple processes attempt to modify the same directory structure simultaneously.
* **Permissions**: Methods that modify the file system (`EnsureDirectoryExists`, `CleanupOldFiles`) require sufficient OS-level permissions to read/write in the target directory.
* **Input Validation**: Methods generally handle null or empty string inputs by returning a default value or throwing an `ArgumentNullException`, depending on the specific implementation requirements. Always validate paths before passing them if unexpected inputs are possible.
* **Path Length**: On some platforms, these methods are still subject to legacy maximum path length limitations (e.g., MAX_PATH on Windows). Long paths may require specific registry or OS configuration to be handled correctly.
