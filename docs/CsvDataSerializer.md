# CsvDataSerializer

The `CsvDataSerializer` class provides functionality for converting collections of data objects into Comma-Separated Values (CSV) formats suitable for export or storage. It supports standard serializing, serialized output including custom metadata headers, and specialized wide-format serialization, while offering utility methods for validating CSV content and identifying format-related metadata.

## API

### Constructor
- `public CsvDataSerializer()`
    - Initializes a new instance of the `CsvDataSerializer` class.

### Methods

- `public string Serialize<T>(IEnumerable<T> data)`
    - Serializes a collection of objects of type `T` into a standard CSV string.
    - Parameters: `data` (The collection to serialize).
    - Returns: A string representing the CSV content.
    - Throws: `ArgumentNullException` if `data` is null.

- `public string SerializeWithMetadata<T>(IEnumerable<T> data, IDictionary<string, string> metadata)`
    - Serializes a collection of objects of type `T` into a CSV string, prepending a section containing specified metadata.
    - Parameters: `data` (The collection to serialize), `metadata` (A key-value dictionary of metadata to prepend).
    - Returns: A string representing the CSV content with metadata headers.
    - Throws: `ArgumentNullException` if `data` or `metadata` is null.

- `public string SerializeWideFormat<T>(IEnumerable<T> data)`
    - Serializes a collection of objects of type `T` into a wide-format CSV string, where each property is mapped to a column, and data points may be pivoted compared to standard serialization.
    - Parameters: `data` (The collection to serialize).
    - Returns: A string representing the wide-format CSV content.
    - Throws: `ArgumentNullException` if `data` is null.

- `public bool IsValidCsv(string content)`
    - Validates whether the provided string conforms to basic CSV formatting standards.
    - Parameters: `content` (The string to validate).
    - Returns: `true` if the content is valid CSV, `false` otherwise.

- `public string GetMimeType()`
    - Retrieves the MIME type associated with CSV files.
    - Returns: A string containing the MIME type ("text/csv").

- `public string GetFileExtension()`
    - Retrieves the standard file extension used for CSV files.
    - Returns: A string containing the extension (".csv").

## Usage

### Standard Serialization
```csharp
var serializer = new CsvDataSerializer();
var data = new List<DataPoint> { new DataPoint(1, 10), new DataPoint(2, 20) };
string csvOutput = serializer.Serialize(data);
File.WriteAllText("data.csv", csvOutput);
```

### Serialization with Custom Metadata
```csharp
var serializer = new CsvDataSerializer();
var data = new List<DataPoint> { new DataPoint(1, 10), new DataPoint(2, 20) };
var metadata = new Dictionary<string, string> { { "Report", "Monthly" }, { "Author", "System" } };
string csvWithMetadata = serializer.SerializeWithMetadata(data, metadata);
File.WriteAllText("report.csv", csvWithMetadata);
```

## Notes

- **Thread Safety**: The `CsvDataSerializer` instance itself is thread-safe for serialization operations, provided the `data` collection being serialized is not modified by another thread during the operation.
- **Performance**: For extremely large datasets, consider streaming the output directly to a file rather than holding the entire serialized string in memory.
- **Format Limitations**: `IsValidCsv` performs a basic structural validation. It may not detect all complex content-related issues (e.g., incorrect character encoding or invalid escape sequences) that specific CSV parsers might encounter.
