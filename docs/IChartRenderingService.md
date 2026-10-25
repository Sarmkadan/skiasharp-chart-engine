# IChartRenderingService

`IChartRenderingService` defines the contract for turning a `Chart` definition into a rasterized image with SkiaSharp. It covers rendering to memory, rendering to a file, rendering with explicit export options, and pre-populating render caches. Each rendering operation is offered in both asynchronous and synchronous forms, so callers can choose the model that fits their context.

The engine registers `ChartRenderingService` as a singleton in `Configuration/ServiceCollectionExtensions.cs`. `ChartEngine` resolves `IChartRenderingService` from the service provider in its constructor. To change how charts are rasterized, implement this interface and register your implementation in its place.

**Namespace:** `SkiaSharpChartEngine.Services`
**Declaration:** `public interface IChartRenderingService`
**Source:** `Services/IChartRenderingService.cs`

## API

### Rendering to Memory

### `Task<RenderResult> RenderToByteArrayAsync(Chart chart, CancellationToken cancellationToken = default)`
### `RenderResult RenderToByteArray(Chart chart)`
Renders a chart into an in-memory image.
- **Parameters:** `chart` - The chart definition containing data series, axes, and styling configuration. `cancellationToken` - Cancels the asynchronous operation.
- **Returns:** A `RenderResult` whose `ImageData` holds the encoded image bytes and whose metadata describes the render.

The synchronous overload blocks the calling thread. The documentation for the asynchronous overload recommends it for server-side scenarios.

### Rendering to a File

### `Task<RenderResult> RenderToFileAsync(Chart chart, string outputPath, CancellationToken cancellationToken = default)`
### `RenderResult RenderToFile(Chart chart, string outputPath)`
Renders a chart and writes the image to disk.
- **Parameters:** `chart` - The chart definition to render. `outputPath` - Absolute or relative path where the image file will be saved. `cancellationToken` - Cancels the asynchronous operation.
- **Returns:** A `RenderResult` whose `OutputPath` is the file that was written, together with render metadata.

### Rendering with Export Options

### `Task<RenderResult> RenderWithExportAsync(Chart chart, ExportOptions exportOptions, CancellationToken cancellationToken = default)`
Renders a chart using the format, resolution, and quality settings in `ExportOptions`.
- **Parameters:** `chart` - The chart definition to render. `exportOptions` - Export settings controlling the output format, DPI, and quality. `cancellationToken` - Cancels the asynchronous operation.
- **Returns:** A `RenderResult` describing the export, including the `ExportFormat` that was used.

This is the only rendering operation that accepts `ExportOptions`. It has no synchronous counterpart.

### Cache Warming

### `void PrewarmCache(Chart chart)`
Pre-populates internal rendering caches, such as font metrics and gradient shaders, for the given chart configuration.
- **Parameters:** `chart` - The chart definition whose resources should be pre-cached.

Call this before rendering in latency-sensitive paths, so that the first real render does not pay the setup cost.

## Behavior Notes

- Methods with an `Async` suffix accept a `CancellationToken` and return a `Task<RenderResult>`. The synchronous counterparts take no token.
- Sync and async pairs are expected to perform the same operation and return the same result.
- A null `chart` (or a null `exportOptions` or `outputPath`) is a programming error and throws `ArgumentNullException`.
- Failures during rendering, such as an invalid chart or an I/O error, are reported through the returned `RenderResult` (`Success` is `false`, with `ErrorMessage` and `Exception` populated) rather than thrown. The bundled implementation applies this to cancellation as well, so a canceled `Async` call returns a failed result instead of throwing `OperationCanceledException`. Custom implementations should document their own behavior.
- The bundled implementation caches rendered bytes keyed by the chart. A cache hit returns the stored image without re-rendering.
- `PrewarmCache` does not throw for rendering failures. It logs a warning and returns.

## Usage

### Rendering to Memory
```csharp
IChartRenderingService rendering = serviceProvider.GetRequiredService<IChartRenderingService>();

var chart = new Chart { /* ... configuration ... */ };
RenderResult result = await rendering.RenderToByteArrayAsync(chart);

if (result.Success)
{
    byte[] png = result.ImageData!;
}
```

### Rendering to a File
```csharp
RenderResult result = await rendering.RenderToFileAsync(chart, "output/chart.png");

if (result.Success)
{
    Console.WriteLine($"Saved to {result.OutputPath}");
}
```

### Exporting with Options
```csharp
var options = ExportOptionsExtensions.ForHighQualityPNG("chart");

RenderResult result = await rendering.RenderWithExportAsync(chart, options);
```

### Warming Caches at Startup
```csharp
rendering.PrewarmCache(chart);
```

### Implementing a Custom Rendering Service
```csharp
public class CustomChartRenderingService : IChartRenderingService
{
    // Implement every member of IChartRenderingService against your rendering backend.
}

services.AddSingleton<IChartRenderingService, CustomChartRenderingService>();
```

## See Also

- `ChartRenderingService` - The default SkiaSharp-based implementation.
- `RenderResult` - The result type returned by every rendering operation.
- `ExportOptions` - Export settings used by `RenderWithExportAsync`.
- `ExportFormat` - The output formats that can be requested.
- `Chart` - The chart definition that is rendered.
- `IChartRepository` - Persistence contract for `Chart` objects.
