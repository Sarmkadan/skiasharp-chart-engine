# IChartStreamingService

`IChartStreamingService` defines the contract for feeding live data into charts and turning that data into rendered frames. A chart is first registered for streaming. Data points are then published into a per-chart buffer, applied to the chart's series when flushed or snapshotted, and rendered on demand as a sequence of PNG frames.

The engine registers `ChartStreamingService` as a singleton through `AddChartStreaming()` in `Streaming/StreamingServiceExtensions.cs`. The implementation depends on `IChartRenderingService` (for frame rendering) and `ILogger<ChartStreamingService>`. To change how streamed data is buffered or how frames are produced, implement this interface and register your implementation in its place.

**Namespace:** `SkiaSharpChartEngine.Streaming`
**Declaration:** `public interface IChartStreamingService`
**Source:** `Streaming/IChartStreamingService.cs`

## Related Types

The contract uses three types declared in `Streaming/StreamingChartOptions.cs`:

- **`StreamingChartOptions`** - per-chart settings passed to `Register`:
  - `MaxBufferSize` (default `1000`, minimum `1`) - capacity of the chart's buffer.
  - `WindowSize` (default `0`) - when greater than zero, each series keeps only its most recent `WindowSize` points.
  - `FlushIntervalMs` (default `100`, minimum `10`) - delay between frames produced by `RenderFramesAsync`.
  - `ReplaceOnUpdate` (default `false`) - when `true`, a series is cleared before each incoming point is added, so it only ever holds the latest point.
- **`StreamDataPoint`** - a single item to publish. It requires `SeriesName` and has `X`, `Y`, an optional `Label`, and a read-only `Timestamp` set to the UTC time of creation.
- **`StreamFrame`** - a rendered frame with `ChartId`, a 1-based `FrameNumber`, PNG `ImageData`, `RenderedAt` (UTC), and `RenderTimeMs`.

## API

### Publishing Data

### `bool Publish(string chartId, StreamDataPoint point)`
Writes a single data point to the buffer of the given chart.
- **Parameters:** `chartId` - The identifier of a chart previously passed to `Register`. `point` - The data point to publish.
- **Returns:** `true` when the point was written to the buffer. `false` when the point could not be written.

### `int PublishBatch(string chartId, IEnumerable<StreamDataPoint> points)`
Writes several data points to the buffer of the given chart, in enumeration order.
- **Parameters:** `chartId` - The identifier of a registered chart. `points` - The data points to publish.
- **Returns:** The number of points that were accepted, as reported by `Publish`.

### Registration

### `void Register(Chart chart, StreamingChartOptions? options = null)`
Registers a chart to receive streamed data and creates its buffer. The chart is keyed by `chart.Id`.
- **Parameters:** `chart` - The chart instance that streamed points will be applied to. `options` - Buffer and frame settings. When `null`, default `StreamingChartOptions` are used.

### `void Unregister(string chartId)`
Removes a chart from streaming and completes its buffer. New calls for the chart fail; see Behavior Notes for running frame sequences.
- **Parameters:** `chartId` - The identifier of the chart to remove.

### Reading the Chart

### `Chart GetSnapshot(string chartId)`
Applies all currently buffered points to the chart and returns it.
- **Parameters:** `chartId` - The identifier of a registered chart.
- **Returns:** The chart instance held by the service, updated with all buffered points.

### `Task<int> FlushAsync(string chartId, CancellationToken cancellationToken = default)`
Applies all currently buffered points to the chart's series without rendering anything.
- **Parameters:** `chartId` - The identifier of a registered chart. `cancellationToken` - Cancels the asynchronous operation.
- **Returns:** The number of points that were applied.

### Rendering Frames

### `IAsyncEnumerable<StreamFrame> RenderFramesAsync(string chartId, CancellationToken cancellationToken = default)`
Produces an unbounded asynchronous sequence of rendered PNG frames for the given chart. On each tick, the sequence waits `FlushIntervalMs`, applies buffered points, and yields a frame only if at least one point was applied.
- **Parameters:** `chartId` - The identifier of a registered chart. `cancellationToken` - Ends the sequence when cancelled.
- **Returns:** An `IAsyncEnumerable<StreamFrame>` that yields frames numbered from `1`.

## Behavior Notes

- **Unregistered charts.** `Publish`, `PublishBatch`, `GetSnapshot`, `FlushAsync`, and `RenderFramesAsync` throw `InvalidOperationException` when `chartId` is not registered. `Unregister` does nothing for unknown identifiers.
- **Argument validation.** `Register` throws `ArgumentNullException` for a null `chart`. `Publish` throws for a null `point`, and `PublishBatch` throws for a null `points`.
- **Bounded buffer.** The bundled implementation uses a bounded channel with the drop-oldest policy. When the buffer is full, the oldest buffered point is discarded to make room for the new one, so `Publish` reports success. Dropping is only logged as a warning when the write itself is rejected. Custom implementations that drop the newest point should report `false` as this interface describes.
- **Re-registration.** Calling `Register` again with an existing `chartId` replaces the previous registration. This includes the chart instance and the buffer, so points that were buffered under the old registration are lost. The "idempotent" wording refers to the call succeeding, not to keeping the old state.
- **Deferred exceptions.** `RenderFramesAsync` is an iterator, so the unregistered-chart exception is thrown on the first `MoveNextAsync` call, not when the method is called.
- **Series creation.** Points naming a series that does not exist yet create it on the chart automatically. The series is added with the point's `SeriesName` and no other styling.
- **Windowing.** With `WindowSize` greater than zero, the oldest points are removed from a series after each point is added, so the series never exceeds `WindowSize` points.
- **Mutation of the chart.** `GetSnapshot` and `FlushAsync` change the chart instance passed to `Register` in place. `GetSnapshot` returns that same instance, not a copy.
- **Render failures.** In `RenderFramesAsync`, an exception from the rendering service is logged and that tick is skipped. A result without image data is logged as a warning and also skipped. Neither ends the sequence.
- **Cancellation.** Cancelling the token passed to `RenderFramesAsync` ends the sequence normally. The bundled implementation does not surface `OperationCanceledException` from the enumerator itself. Exceptions thrown by the consumer's loop body are not affected.
- **Threading.** `Publish` and `PublishBatch` may be called from several threads. Applying buffered points to the chart is not synchronized across calls, so calling `FlushAsync`, `GetSnapshot`, and a running `RenderFramesAsync` for the same chart at the same time can modify the same series concurrently. Serialize these calls when concurrent consumers are used.
- **Unregistering and disposal.** After `Unregister` (or disposing the bundled implementation, which implements `IDisposable`), new calls for that chart throw `InvalidOperationException`. A `RenderFramesAsync` sequence that is already running keeps its own reference to the registration. It continues to apply any remaining buffered points and to yield frames until its cancellation token is cancelled, so cancel it when you unregister. Unregistering does not stop it by itself.

## Usage

### Registering a Chart and Publishing Points
```csharp
IChartStreamingService streaming = serviceProvider.GetRequiredService<IChartStreamingService>();

var chart = new Chart { Id = "sensor-chart" /* ... configuration ... */ };
streaming.Register(chart, new StreamingChartOptions
{
    MaxBufferSize = 500,
    WindowSize = 200,
    FlushIntervalMs = 250
});

streaming.Publish("sensor-chart", new StreamDataPoint
{
    SeriesName = "temperature",
    X = 1,
    Y = 25.5
});

int accepted = streaming.PublishBatch("sensor-chart", new[]
{
    new StreamDataPoint { SeriesName = "temperature", X = 2, Y = 24.0 },
    new StreamDataPoint { SeriesName = "temperature", X = 3, Y = 26.0 }
});
```

### Reading a Snapshot
```csharp
Chart current = streaming.GetSnapshot("sensor-chart");
```

### Rendering Frames
```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));

await foreach (StreamFrame frame in streaming.RenderFramesAsync("sensor-chart", cts.Token))
{
    Console.WriteLine($"Frame {frame.FrameNumber} rendered in {frame.RenderTimeMs} ms");
    await File.WriteAllBytesAsync($"frames/{frame.FrameNumber}.png", frame.ImageData);
}

// The sequence has ended because the token was cancelled; now stop streaming.
streaming.Unregister("sensor-chart");
```
