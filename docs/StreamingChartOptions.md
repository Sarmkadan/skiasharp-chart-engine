# StreamingChartOptions

The `StreamingChartOptions` class encapsulates the data and metadata required for updating or rendering individual frames within a streaming chart environment in the `skiasharp-chart-engine`. It serves as a container for transmitting data points, associated labels, and the resulting rendered frame information between data producers and the rendering pipeline, ensuring that each frame is correctly associated with a specific chart instance, series, and temporal context.

## API

*   **`ReplaceOnUpdate`** (`bool`)
    Determines whether an existing data point for the specified `SeriesName` and `Timestamp` should be overwritten if it already exists within the chart's data store during an update operation.

*   **`SeriesName`** (`required string`)
    The unique identifier for the data series to which the data point belongs. As a required member, this must be populated to identify the target series correctly.

*   **`X`** (`double`)
    The horizontal coordinate value for the data point.

*   **`Y`** (`double`)
    The vertical coordinate value for the data point.

*   **`Label`** (`string?`)
    An optional text label associated with the data point, which can be displayed as a tooltip or annotation.

*   **`Timestamp`** (`DateTime`)
    The chronological timestamp assigned to this data point.

*   **`ChartId`** (`required string`)
    The unique identifier of the chart to which this data point and its associated frame belong. As a required member, this must be provided to ensure the data is routed to the correct chart instance.

*   **`FrameNumber`** (`long`)
    The sequential index of the frame within the stream, useful for maintaining order and identifying frame drops.

*   **`ImageData`** (`required byte[]`)
    The raw byte array containing the rendered image data for the chart frame. As a required member, this must contain valid image data encoded in a format supported by the rendering engine.

*   **`RenderedAt`** (`DateTime`)
    The system timestamp indicating exactly when this frame was rendered by the processing engine.

*   **`RenderTimeMs`** (`long`)
    The duration, measured in milliseconds, taken to perform the rendering operation for this specific frame.

## Usage

### Creating a new frame update
```csharp
var options = new StreamingChartOptions
{
    ChartId = "temperature-monitor-01",
    SeriesName = "sensor-a",
    X = 10.5,
    Y = 22.3,
    Label = "Current Temp",
    Timestamp = DateTime.UtcNow,
    FrameNumber = 125,
    ImageData = renderedBytes, // Assuming byte[] from SkiaSharp surface
    RenderedAt = DateTime.UtcNow,
    RenderTimeMs = 15,
    ReplaceOnUpdate = false
};
```

### Passing options to a processing service
```csharp
public void HandleFrame(StreamingChartOptions frameData)
{
    // Validate required fields are non-null/empty if not guaranteed by caller
    if (string.IsNullOrEmpty(frameData.ChartId) || frameData.ImageData == null)
    {
        throw new ArgumentException("Invalid streaming options.");
    }
    
    // Logic to push frame data to the rendering queue
    _renderQueue.Enqueue(frameData);
}
```

## Notes

*   **Thread Safety:** Instances of `StreamingChartOptions` are not thread-safe. If an instance is shared between a producer thread and a consumer thread (e.g., a background worker), external synchronization must be implemented to prevent race conditions during modification or read operations.
*   **Data Validation:** While the `required` keyword enforces initialization, the `ImageData` byte array should be validated for integrity and format compatibility before being passed to the `skiasharp-chart-engine` drawing surfaces.
*   **Temporal Precision:** The `Timestamp` and `RenderedAt` properties use `DateTime`. Ensure the system clock is synchronized via NTP if high-precision temporal alignment across distributed components is required.
*   **Performance:** Given that `ImageData` is passed by reference as a `byte[]`, care should be taken to avoid unintended mutations of this array once it is handed off to the processing pipeline.
