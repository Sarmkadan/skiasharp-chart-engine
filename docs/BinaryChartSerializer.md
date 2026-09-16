# BinaryChartSerializer

## Overview
The `BinaryChartSerializer` class provides efficient binary serialization and deserialization of `Chart` objects for compact storage and transmission. It implements an asynchronous API using `System.IO.BinaryWriter` and `System.IO.BinaryReader` with UTF-8 encoding.

## Serialization Format
The binary format consists of the following sequential components:

1. **Format Version** (1 byte)
   - Current version: `1`
   - Used for backward compatibility; deserialization will fail if versions don't match

2. **Chart Metadata**
   - Id (string, length-prefixed UTF8)
   - Title (string, length-prefixed UTF8)
   - ChartType (4-byte integer representing the `ChartType` enum)
   - CreatedAt (8-byte Int64 representing DateTime.Ticks)
   - UpdatedAt (8-byte Int64 representing DateTime.Ticks)

3. **Series Data**
   - SeriesCount (4-byte integer)
   - For each series:
     - Name (string, length-prefixed UTF8)
     - Color (string, length-prefixed UTF8)
     - DataPointCount (4-byte integer)
     - For each data point:
       - Label (string, length-prefixed UTF8)
       - Value (8-byte double)

All strings are written as length-prefixed UTF8 encoded bytes (7-bit encoded integer length followed by the bytes).

## Public Methods

### SerializeAsync
```csharp
public async Task<byte[]> SerializeAsync(Chart chart)
```
Serializes a `Chart` object to a byte array in the binary format.

**Parameters:**
- `chart`: The chart to serialize (cannot be null)

**Returns:**
- A byte array containing the serialized chart data

**Exceptions:**
- `ArgumentNullException`: If `chart` is null
- `IOException`: If an I/O error occurs during serialization

### DeserializeAsync
```csharp
public async Task<Chart> DeserializeAsync(byte[] data)
```
Deserializes a byte array back into a `Chart` object.

**Parameters:**
- `data`: The byte array containing serialized chart data (cannot be null or empty)

**Returns:**
- A `Chart` object reconstructed from the binary data

**Exceptions:**
- `ArgumentException`: If `data` is null or empty
- `InvalidOperationException`: If the format version in the data is unsupported
- `IOException`: If an I/O error occurs during deserialization

## Example Round-Trip
The following example demonstrates serializing a chart to binary format and then deserializing it back:

```csharp
// Create a logger (required for constructor)
var loggerFactory = LoggerFactory.Create(builder => 
    builder.AddConsole());
var logger = loggerFactory.CreateLogger<BinaryChartSerializer>();

// Create serializer instance
var serializer = new BinaryChartSerializer(logger);

// Create a sample chart
var chart = new Chart
{
    Id = "chart-123",
    Title = "Sample Sales Chart",
    ChartType = ChartType.Bar,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow,
    Series = new List<ChartSeries>
    {
        new ChartSeries
        {
            Name = "Q1 Sales",
            Color = "#4CAF50",
            DataPoints = new List<DataPoint>
            {
                new DataPoint { Label = "Jan", Value = 1000 },
                new DataPoint { Label = "Feb", Value = 1200 },
                new DataPoint { Label = "Mar", Value = 950 }
            }
        },
        new ChartSeries
        {
            Name = "Q2 Sales",
            Color = "#2196F3",
            DataPoints = new List<DataPoint>
            {
                new DataPoint { Label = "Apr", Value = 1100 },
                new DataPoint { Label = "May", Value = 1300 },
                new DataPoint { Label = "Jun", Value = 1050 }
            }
        }
    }
};

// Serialize the chart to binary format
byte[] binaryData = await serializer.SerializeAsync(chart);
Console.WriteLine($"Serialized chart size: {binaryData.Length} bytes");

// Deserialize the binary data back to a chart
Chart deserializedChart = await serializer.DeserializeAsync(binaryData);

// Verify the round-trip succeeded
Console.WriteLine($"Deserialized chart ID: {deserializedChart.Id}");
Console.WriteLine($"Deserialized chart title: {deserializedChart.Title}");
Console.WriteLine($"Number of series: {deserializedChart.Series.Count}");
```

**Notes:**
- The serializer uses simulated async delays (`Task.Delay`) in private methods to demonstrate asynchronous behavior, but these are minimal and don't affect the actual serialization logic.
- The binary format is designed for compactness rather than human readability.
- All string data is encoded as UTF8 to support international characters.
- Null values in string properties are serialized as empty strings.