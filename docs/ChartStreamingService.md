# ChartStreamingService

The `ChartStreamingService` facilitates real-time data ingestion and frame rendering for charts within the SkiaSharp chart engine. It acts as a central coordinator, managing the registration of data streams, the publication of high-frequency data points or batches, and the generation of rendered frames asynchronously, ensuring that chart visualizations remain accurately synchronized with incoming data updates.

## API

### Constructor
`public ChartStreamingService()`
Initializes a new instance of the `ChartStreamingService`.

### Register
`public void Register(string streamId, IDataStream stream)`
Registers a data stream identified by `streamId`. 
*   **Parameters:** `streamId` (string identifier for the stream), `stream` (the `IDataStream` implementation to register).
*   **Throws:** `ArgumentException` if the `streamId` is already registered.

### Unregister
`public void Unregister(string streamId)`
Removes the registered data stream associated with the provided `streamId`.
*   **Parameters:** `streamId` (string identifier for the stream).

### Publish
`public bool Publish(string streamId, DataPoint point)`
Publishes a single data point to the specified stream.
*   **Parameters:** `streamId` (string identifier), `point` (the data point to publish).
*   **Returns:** `true` if the point was accepted; `false` otherwise.

### PublishBatch
`public int PublishBatch(string streamId, IEnumerable<DataPoint> points)`
Publishes a collection of data points to the specified stream.
*   **Parameters:** `streamId` (string identifier), `points` (an enumeration of data points).
*   **Returns:** The number of data points successfully published.

### GetSnapshot
`public Chart GetSnapshot(string streamId)`
Retrieves a static snapshot of the chart based on the current state of the specified stream.
*   **Parameters:** `streamId` (string identifier).
*   **Returns:** The current `Chart` snapshot.

### FlushAsync
`public async Task<int> FlushAsync(CancellationToken ct = default)`
Processes and flushes all pending data operations to ensure the internal state is synchronized.
*   **Parameters:** `ct` (optional cancellation token).
*   **Returns:** A task representing the asynchronous operation, returning the number of operations processed.

### RenderFramesAsync
`public async IAsyncEnumerable<StreamFrame> RenderFramesAsync(CancellationToken ct = default)`
Provides an asynchronous stream of rendered frames based on the currently processed data.
*   **Parameters:** `ct` (optional cancellation token).
*   **Returns:** An `IAsyncEnumerable` of `StreamFrame` objects.

### Dispose
`public void Dispose()`
Releases all resources used by the `ChartStreamingService`.

## Usage

### Registering and Publishing Data
```csharp
var service = new ChartStreamingService();
service.Register("sensor_stream", myDataStream);

// Publish a single point
service.Publish("sensor_stream", new DataPoint(DateTime.Now, 25.5));

// Publish a batch of points
var points = new List<DataPoint> { 
    new DataPoint(DateTime.Now.AddSeconds(-1), 24.0),
    new DataPoint(DateTime.Now, 26.0)
};
service.PublishBatch("sensor_stream", points);
```

### Rendering Frames Asynchronously
```csharp
var service = new ChartStreamingService();
// ... assume streams are registered and data is published ...

await foreach (var frame in service.RenderFramesAsync(cancellationToken))
{
    // Apply frame.ImageData to the UI component
    UpdateUI(frame.ImageData);
}
```

## Notes

*   **Thread Safety:** The `Publish` and `PublishBatch` methods are thread-safe and can be called from multiple background threads simultaneously to ingest data.
*   **Async Operations:** `FlushAsync` and `RenderFramesAsync` are designed to be non-blocking. If `RenderFramesAsync` is not consumed, internal buffers may fill up, potentially impacting performance or leading to dropped frames depending on the configured overflow strategy.
*   **Resource Cleanup:** It is critical to call `Dispose()` when the service is no longer needed to release underlying SkiaSharp resources and stop any active rendering loops.
*   **Stream Identification:** All `streamId` values are case-sensitive. Attempting to publish to a non-existent `streamId` will result in a no-op or a warning, depending on the service configuration.
