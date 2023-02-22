# QuickStartExample

Utility class providing high-level methods to quickly create, validate, and export SkiaSharp charts without deep configuration. Designed for common charting scenarios such as single-series line charts, multi-series comparisons, and palette generation. All methods are static and thread-safe for concurrent use.

## API

### `public static void CreateSimpleLineChart()`

Creates a basic line chart with default styling and a single data series. The chart is rendered to an in-memory `SKBitmap` and stored internally for later export or display. No parameters are required; default values are applied for chart size, colors, axes, and data points.

Parameters:
- None

Return value:
- None

Throws:
- `InvalidOperationException`: If the internal chart engine fails to initialize or if the default configuration is invalid.
- `ArgumentException`: If the default data series cannot be generated (e.g., empty dataset).

---

### `public static void CreateMultiSeriesChart()`

Constructs a multi-series line chart with automatic legend, axis scaling, and synchronized data ranges. Uses default styling and generates sample data across multiple series. The resulting chart is stored in memory for subsequent export or rendering.

Parameters:
- None

Return value:
- None

Throws:
- `InvalidOperationException`: If the chart engine cannot initialize or if data alignment across series fails.
- `ArgumentException`: If generated sample data is invalid or inconsistent.

---

### `public static void ExportChartToFile()`

Exports the most recently created chart (from `CreateSimpleLineChart` or `CreateMultiSeriesChart`) to a file on disk. Supports common image formats such as PNG, JPEG, and PDF based on the file extension. Uses default export settings unless overridden by internal configuration.

Parameters:
- `string filePath`: Absolute or relative path to the output file. Must include a supported file extension.

Return value:
- None

Throws:
- `ArgumentNullException`: If `filePath` is `null`.
- `ArgumentException`: If `filePath` is empty, contains invalid characters, or uses an unsupported format.
- `InvalidOperationException`: If no chart has been created yet or if file system access is denied.
- `IOException`: If the file cannot be written (e.g., directory does not exist, permissions issue).

---
### `public static void ValidateChartBeforeRendering()`

Performs pre-render validation on the current chart to ensure it is safe to export or display. Checks for null references, invalid data ranges, missing axes, and rendering engine state. Logs warnings to the console if minor issues are detected but does not prevent continuation.

Parameters:
- None

Return value:
- None

Throws:
- None

---
### `public static void GenerateAndVisualizePaletteColors()`

Generates a color palette using the current theme engine and renders it as a horizontal strip of colored rectangles. Useful for visualizing available colors before applying them to charts. Output is displayed in a modal window or saved to a temporary file depending on platform.

Parameters:
- None

Return value:
- None

Throws:
- `InvalidOperationException`: If the palette engine fails to initialize or if color generation is interrupted.

---
### `public static async Task RenderChartAsync()`

Asynchronously renders the current chart to a `SKBitmap` and returns it via a `Task<SKBitmap>`. Designed for responsive UI applications where synchronous rendering may block the main thread. Uses default resolution and quality settings.

Parameters:
- None

Return value:
- `Task<SKBitmap>`: A task that completes with the rendered chart bitmap. The bitmap is owned by the caller and must be disposed.

Throws:
- `InvalidOperationException`: If no chart has been created or if rendering fails.
- `OperationCanceledException`: If the rendering task is canceled via the caller's cancellation token (not exposed in signature; internal cancellation may occur).

## Usage

### Example 1: Create and Export a Simple Line Chart
