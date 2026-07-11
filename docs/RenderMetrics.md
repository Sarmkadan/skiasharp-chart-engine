# RenderMetrics

`RenderMetrics` is a metrics tracking class used by the SkiaSharp Chart Engine to record and analyze rendering performance characteristics of chart visualizations. It captures key performance indicators such as render time, image size, data density, and cache efficiency, enabling performance profiling and optimization.

## API

### `public string? ChartId`
Identifies the chart associated with the metrics. May be `null` if the chart ID is not available at the time of recording.

### `public DateTime RenderedAt`
Timestamp indicating when the chart rendering was completed. Useful for correlating metrics with external logs or events.

### `public long RenderTimeMs`
Total time taken to render the chart, measured in milliseconds. Represents the duration from the start of rendering to completion.

### `public long ImageSizeBytes`
Size of the rendered image in bytes. Used to assess memory usage and bandwidth requirements for chart distribution.

### `public int SeriesCount`
Number of data series included in the chart. Helps evaluate rendering complexity and performance impact of multi-series visualizations.

### `public int DataPointCount`
Total number of data points across all series in the chart. Indicates the data density and potential rendering load.

### `public ExportFormat ExportFormat`
The format in which the chart was exported (e.g., PNG, JPEG, SVG). Determines compression and quality characteristics of the rendered image.

### `public int CacheSizeAtRenderTime`
Number of cached entries available at the time of rendering. Reflects cache utilization during the rendering process.

### `public bool WasCached`
Indicates whether the rendered chart was served from cache (`true`) or newly generated (`false`). Useful for measuring cache effectiveness.

### `public Dictionary<string, object>? AdditionalMetrics`
Optional dictionary for storing arbitrary, implementation-specific metrics related to the rendering process. May be `null` if no additional metrics are recorded.

### `public double GetMegabytesPerSecond()`
Computes the rendering throughput in megabytes per second. Calculated as `(ImageSizeBytes / 1024.0 / 1024.0) / (RenderTimeMs / 1000.0)`. Returns `0.0` if `RenderTimeMs` is zero to avoid division by zero.

### `public double GetDataPointsPerSecond()`
Computes the data processing rate in data points per second. Calculated as `DataPointCount / (RenderTimeMs / 1000.0)`. Returns `0.0` if `RenderTimeMs` is zero.

### `public override string ToString()`
Returns a human-readable summary of the metrics, including `ChartId`, `RenderedAt`, `RenderTimeMs`, `ImageSizeBytes`, `SeriesCount`, `DataPointCount`, and `ExportFormat`. Useful for logging and debugging.

### `public void RecordMetrics()`
Records the current metrics into a persistent collection for later retrieval and analysis. No parameters or return value. May throw if the internal storage mechanism fails (e.g., due to thread contention or corruption).

### `public List<RenderMetrics> GetAllMetrics()`
Retrieves all recorded metrics in chronological order. Returns an empty list if no metrics have been recorded. Thread-safe for concurrent reads.

### `public RenderMetrics? GetLastMetrics()`
Retrieves the most recently recorded metrics. Returns `null` if no metrics have been recorded. Thread-safe for concurrent reads.

### `public double GetAverageRenderTimeMs()`
Computes the average render time across all recorded metrics in milliseconds. Returns `0.0` if no metrics are available. Thread-safe for concurrent reads.

### `public double GetAverageImageSizeBytes()`
Computes the average image size across all recorded metrics in bytes. Returns `0.0` if no metrics are available. Thread-safe for concurrent reads.

### `public int GetCacheHitCount()`
Returns the total number of cache hits recorded across all metrics. Useful for evaluating cache performance. Returns `0` if no metrics are available. Thread-safe for concurrent reads.

### `public double GetCacheHitPercentage()`
Computes the percentage of cache hits relative to total render operations. Calculated as `(GetCacheHitCount() / (double)GetAllMetrics().Count) * 100.0`. Returns `0.0` if no metrics are available. Thread-safe for concurrent reads.

## Usage

### Example 1: Recording and Retrieving Metrics
