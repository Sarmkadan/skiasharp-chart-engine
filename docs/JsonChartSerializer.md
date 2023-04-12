# JsonChartSerializer

The `JsonChartSerializer` provides functionality for converting `Chart` instances and collections of `Chart` objects to and from JSON format, ensuring data compatibility within the `skiasharp-chart-engine` ecosystem.

## API

| Member | Description |
| :--- | :--- |
| `JsonChartSerializer()` | Initializes a new instance of the `JsonChartSerializer` class. |
| `string Serialize(Chart chart)` | Converts a `Chart` object into its JSON string representation. |
| `Chart? Deserialize(string json)` | Attempts to deserialize a JSON string into a `Chart` object. Returns `null` if deserialization fails. |
| `string SerializeCollection(List<Chart> charts)` | Serializes a list of `Chart` objects into a JSON array string. |
| `List<Chart> DeserializeCollection(string json)` | Deserializes a JSON string into a `List<Chart>`. |
| `bool IsValidJson(string json)` | Validates whether the provided string is properly formatted JSON. |
| `string PrettyPrint(string json)` | Formats a JSON string with indentation for enhanced readability. |
| `SerializationException` | An exception class thrown when serialization or deserialization processes fail due to malformed input or schema mismatches. |
| `SerializationException(string message)` | Constructor for `SerializationException`, initializing the exception with a specific error message. |

## Usage

### Serializing a Single Chart
```csharp
var serializer = new JsonChartSerializer();
var chart = new Chart();
// ... configure chart ...

string json = serializer.Serialize(chart);
Console.WriteLine(serializer.PrettyPrint(json));
```

### Deserializing a Collection of Charts
```csharp
var serializer = new JsonChartSerializer();
string jsonArray = "[{...}, {...}]"; // JSON representing a list of charts

try 
{
    List<Chart> charts = serializer.DeserializeCollection(jsonArray);
    foreach (var chart in charts)
    {
        // ... process charts ...
    }
}
catch (SerializationException ex)
{
    Console.WriteLine($"Failed to deserialize collection: {ex.Message}");
}
```

## Notes

*   **Thread Safety**: Instances of `JsonChartSerializer` are not guaranteed to be thread-safe. It is recommended to instantiate a new serializer per thread or use appropriate synchronization if sharing instances across threads.
*   **Error Handling**: Methods may throw `SerializationException` if the input JSON does not conform to the expected `Chart` structure or if serialization fails due to invalid object state.
*   **Validation**: `IsValidJson` performs a structural check on the JSON syntax but does not guarantee the JSON conforms specifically to the `Chart` object schema.
