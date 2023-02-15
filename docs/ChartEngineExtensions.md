# ChartEngineExtensions

Provides extension methods for generating, rendering, and saving charts using SkiaSharp. These methods simplify common charting workflows by combining chart creation, rendering, and file output into single calls.

## API

### `public static async Task<RenderResult> RenderAndSaveChartAsync`

Renders a chart to a file asynchronously and returns a `RenderResult` indicating success or failure. The chart is created using the provided chart configuration delegate, rendered to a stream, and saved to the specified file path. Throws `ArgumentNullException` if `chartConfig` or `filePath` is `null`. Throws `IOException` if the file cannot be written.

**Parameters**
- `chartConfig` (`Action<Chart>`): A delegate that configures the chart.
- `filePath` (`string`): The path where the chart image will be saved.
- `imageFormat` (`SKEncodedImageFormat`, optional): The format of the output image (default: `SKEncodedImageFormat.Png`).
- `quality` (`int`, optional): The image quality (0–100), relevant for JPEG (default: 75).

**Return Value**
- `Task<RenderResult>`: A task that resolves to a `RenderResult` indicating success or failure.

---

### `public static RenderResult RenderAndSaveChart`

Synchronous version of `RenderAndSaveChartAsync`. Renders a chart to a file and returns a `RenderResult`. Throws `ArgumentNullException` if `chartConfig` or `filePath` is `null`. Throws `IOException` if the file cannot be written.

**Parameters**
- `chartConfig` (`Action<Chart>`): A delegate that configures the chart.
- `filePath` (`string`): The path where the chart image will be saved.
- `imageFormat` (`SKEncodedImageFormat`, optional): The format of the output image (default: `SKEncodedImageFormat.Png`).
- `quality` (`int`, optional): The image quality (0–100), relevant for JPEG (default: 75).

**Return Value**
- `RenderResult`: A result indicating success or failure.

---

### `public static async Task<Chart> CreateAndSaveChartAsync`

Creates a chart using the provided configuration delegate, renders it to a stream, saves it to the specified file path, and returns the configured `Chart` instance. Throws `ArgumentNullException` if `chartConfig` or `filePath` is `null`. Throws `IOException` if the file cannot be written.

**Parameters**
- `chartConfig` (`Action<Chart>`): A delegate that configures the chart.
- `filePath` (`string`): The path where the chart image will be saved.
- `imageFormat` (`SKEncodedImageFormat`, optional): The format of the output image (default: `SKEncodedImageFormat.Png`).
- `quality` (`int`, optional): The image quality (0–100), relevant for JPEG (default: 75).

**Return Value**
- `Task<Chart>`: A task that resolves to the configured `Chart` instance.

---
### `public static Chart CreateAndSaveChart`

Synchronous version of `CreateAndSaveChartAsync`. Creates a chart, renders it to a file, and returns the configured `Chart` instance. Throws `ArgumentNullException` if `chartConfig` or `filePath` is `null`. Throws `IOException` if the file cannot be written.

**Parameters**
- `chartConfig` (`Action<Chart>`): A delegate that configures the chart.
- `filePath` (`string`): The path where the chart image will be saved.
- `imageFormat` (`SKEncodedImageFormat`, optional): The format of the output image (default: `SKEncodedImageFormat.Png`).
- `quality` (`int`, optional): The image quality (0–100), relevant for JPEG (default: 75).

**Return Value**
- `Chart`: The configured `Chart` instance.

---
### `public static async Task<MemoryStream> RenderToStreamAsync`

Renders a chart to a `MemoryStream` asynchronously. The chart is created using the provided chart configuration delegate. Throws `ArgumentNullException` if `chartConfig` is `null`.

**Parameters**
- `chartConfig` (`Action<Chart>`): A delegate that configures the chart.
- `imageFormat` (`SKEncodedImageFormat`, optional): The format of the output image (default: `SKEncodedImageFormat.Png`).
- `quality` (`int`, optional): The image quality (0–100), relevant for JPEG (default: 75).

**Return Value**
- `Task<MemoryStream>`: A task that resolves to a `MemoryStream` containing the rendered chart image.

---
### `public static MemoryStream RenderToStream`

Synchronous version of `RenderToStreamAsync`. Renders a chart to a `MemoryStream`. Throws `ArgumentNullException` if `chartConfig` is `null`.

**Parameters**
- `chartConfig` (`Action<Chart>`): A delegate that configures the chart.
- `imageFormat` (`SKEncodedImageFormat`, optional): The format of the output image (default: `SKEncodedImageFormat.Png`).
- `quality` (`int`, optional): The image quality (0–100), relevant for JPEG (default: 75).

**Return Value**
- `MemoryStream`: A stream containing the rendered chart image.

---
### `public static async Task<RenderResult> QuickRenderAsync`

Renders a chart to a `MemoryStream` asynchronously and returns a `RenderResult` indicating success or failure. The chart is created using the provided chart configuration delegate. Throws `ArgumentNullException` if `chartConfig` is `null`.

**Parameters**
- `chartConfig` (`Action<Chart>`): A delegate that configures the chart.
- `imageFormat` (`SKEncodedImageFormat`, optional): The format of the output image (default: `SKEncodedImageFormat.Png`).
- `quality` (`int`, optional): The image quality (0–100), relevant for JPEG (default: 75).

**Return Value**
- `Task<RenderResult>`: A task that resolves to a `RenderResult` indicating success or failure.

## Usage
