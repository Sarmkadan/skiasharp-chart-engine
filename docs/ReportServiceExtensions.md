# ReportServiceExtensions

Provides a dependency injection helper for PDF report generation in the `SkiaSharpChartEngine.Reports` namespace. The single extension method registers `IPdfReportGenerator` with its default implementation, `PdfReportGenerator`, as a singleton.

## Registration

`AddPdfReportGenerator` only registers the generator. It does not register the services that `PdfReportGenerator` needs. Before resolving `IPdfReportGenerator`, the container must also provide:

- `IChartRenderingService`: registered by `AddSkiaSharpChartEngine()` in `Configuration/ServiceCollectionExtensions.cs`.
- `ILogger<PdfReportGenerator>`: provided by the logging infrastructure, for example through `AddLogging()`.

If either service is missing, resolution fails with the container's own exception (for example `InvalidOperationException` from `Microsoft.Extensions.DependencyInjection`). The extension method itself does not throw in that case.

Calling `AddPdfReportGenerator` more than once adds another singleton registration for the same service type each time. The container resolves the last registration.

## API

### `public static IServiceCollection AddPdfReportGenerator`

Registers `IPdfReportGenerator` as a singleton, implemented by `PdfReportGenerator`. The registration uses `AddSingleton<IPdfReportGenerator, PdfReportGenerator>()`, so the container creates one instance and shares it for its lifetime.

**Parameters**
- `services` (`IServiceCollection`, `this`): The service collection to configure.

**Return Value**
- `IServiceCollection`: The same `services` instance, so calls can be chained.

**Exceptions**
- `ArgumentNullException`: Thrown when `services` is `null`. The check uses `ArgumentNullException.ThrowIfNull`.

---

## Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using SkiaSharpChartEngine.Configuration;
using SkiaSharpChartEngine.Reports;

var services = new ServiceCollection();

services.AddLogging();
services.AddSkiaSharpChartEngine();
services.AddPdfReportGenerator();

using var provider = services.BuildServiceProvider();

IPdfReportGenerator generator = provider.GetRequiredService<IPdfReportGenerator>();
```

The chained form works the same way:

```csharp
var provider = new ServiceCollection()
    .AddLogging()
    .AddSkiaSharpChartEngine()
    .AddPdfReportGenerator()
    .BuildServiceProvider();
```

## Related

- [IPdfReportGenerator](IPdfReportGenerator.md): the interface this method registers.
- [PdfReportGenerator](PdfReportGenerator.md): the default implementation, including its constructor requirements.
- [PdfReportOptions](PdfReportOptions.md): layout and appearance options passed to the generator.
- [ReportSection](ReportSection.md): the section model the generator consumes.
