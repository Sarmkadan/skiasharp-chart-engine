# IPdfReportGenerator

`IPdfReportGenerator` is the contract for producing PDF reports that combine ordered text sections and rendered chart images. It lives in the `SkiaSharpChartEngine.Reports` namespace, in `Reports/IPdfReportGenerator.cs`. The default implementation is [`PdfReportGenerator`](PdfReportGenerator.md).

The interface offers two output modes: returning the PDF as a `byte[]` and writing it directly to a file. Both accept the same section list and optional layout options.

## Interface

```csharp
public interface IPdfReportGenerator
{
    Task<byte[]> GenerateAsync(
        IReadOnlyList<ReportSection> sections,
        PdfReportOptions? options = null,
        CancellationToken cancellationToken = default);

    Task GenerateToFileAsync(
        string outputPath,
        IReadOnlyList<ReportSection> sections,
        PdfReportOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

## Methods

### GenerateAsync

Generates a PDF report and returns its contents as an in-memory byte array. Use this when the caller needs to stream the PDF to an HTTP response, store it through another abstraction, or post-process the bytes.

| Parameter | Description |
| --- | --- |
| `sections` | Ordered list of `ReportSection` values. Sections are rendered in list order. |
| `options` | Layout and appearance settings as `PdfReportOptions`. `null` uses a new instance with default values. |
| `cancellationToken` | Cancels chart rendering and section processing. |

**Returns:** `Task<byte[]>` containing the complete PDF file.

### GenerateToFileAsync

Generates a PDF report and writes it to `outputPath`. Use this when the report should be saved on disk.

| Parameter | Description |
| --- | --- |
| `outputPath` | Absolute or relative path of the PDF file to write. |
| `sections` | Ordered list of `ReportSection` values. |
| `options` | Layout and appearance settings. `null` uses default values. |
| `cancellationToken` | Cancels generation and the asynchronous file write. |

**Returns:** `Task`. The task completes after the file has been written.

The method is expected to create missing parent directories of `outputPath` and to overwrite an existing file at that path.

## Related types

These types are defined in `Reports/PdfReportOptions.cs` and are used by both methods:

- **`ReportSection`** – one block of report content with `Heading`, `BodyText`, optional `Chart`, `ImageFit` (`PdfImageFit`, default `FitWidth`) and `PageBreakBefore`.
- **`PdfReportOptions`** – document title and subtitle, page size, margin, font sizes, chart DPI, page numbers, and colors.
- **`PdfImageFit`** – controls how chart images are scaled: `Original`, `FitWidth`, `FitHeight`, or `FitPage`.

## Implementations

| Class | Description |
| --- | --- |
| [`PdfReportGenerator`](PdfReportGenerator.md) | Default implementation. Renders charts through `IChartRenderingService` and draws the PDF with SkiaSharp. |

## Registration

`ReportServiceExtensions.AddPdfReportGenerator()` registers `IPdfReportGenerator` with `PdfReportGenerator` as a singleton:

```csharp
services.AddPdfReportGenerator();
```

## Usage

Resolve the interface from the service provider and generate a report:

```csharp
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Reports;

IPdfReportGenerator generator = serviceProvider.GetRequiredService<IPdfReportGenerator>();

var sections = new List<ReportSection>
{
    new() { Heading = "Summary", BodyText = "Throughput increased in the third quarter." },
    new() { Heading = "Trend", Chart = trendChart, ImageFit = PdfImageFit.FitPage }
};

// In-memory PDF
byte[] pdf = await generator.GenerateAsync(sections, new PdfReportOptions { Title = "Q3 Report" }, cancellationToken);

// Written to disk
await generator.GenerateToFileAsync("reports/q3.pdf", sections, cancellationToken: cancellationToken);
```

## Notes

- Callers typically depend on `IPdfReportGenerator` rather than `PdfReportGenerator`, so the implementation can be replaced in tests or by a custom registration.
- Arguments follow the behavior of the default implementation: a `null` `sections` list throws `ArgumentNullException`, and a null, empty, or whitespace-only `outputPath` in `GenerateToFileAsync` throws `ArgumentNullException`.
- Cancellation is observed during processing and may surface as `OperationCanceledException`.
