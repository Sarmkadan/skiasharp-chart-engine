# RenderingBenchmarks

A utility class for measuring the performance of chart rendering operations in the SkiaSharp Chart Engine. It provides controlled benchmarking scenarios for cold and warm rendering paths, allowing comparison of initial and subsequent render times under identical conditions.

## API

### `public int DataPoints`

Gets or sets the number of data points to generate for benchmarking. This value determines the size of the dataset used during rendering operations. Increasing this value will generally increase rendering time and memory usage.

### `public void Setup()`

Initializes the benchmark environment, including chart configuration and data generation. This method must be called before any rendering operations (`Render_Cold` or `Render_Warm`) to ensure valid test conditions. It does not return a value and throws no exceptions under normal operation.

### `public RenderResult Render_Cold()`

Executes a full chart rendering pipeline from scratch, simulating a first-time render with no cached resources. This includes data generation, layout calculation, and final image rasterization. Returns a `RenderResult` containing timing metrics and memory usage. Does not throw exceptions under normal operation.

### `public RenderResult Render_Warm()`

Executes a chart rendering pipeline using pre-warmed resources (e.g., cached layout, pre-generated geometry), simulating a repeat render after initial setup. Returns a `RenderResult` with performance metrics. Does not throw exceptions under normal operation.

## Usage
