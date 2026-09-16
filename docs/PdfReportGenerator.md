# PdfReportGenerator

`PdfReportGenerator` is the default `IPdfReportGenerator` implementation. It creates a PDF with a title page followed by ordered report sections containing an optional heading, body text, and chart. PDF pages and text are drawn with SkiaSharp; charts are first rendered as PNG images by `IChartRenderingService` and then embedded in the PDF.

## Construction and registration

The constructor requires an `IChartRenderingService` and an `ILogger<PdfReportGenerator>`. Both arguments must be non-null. Applications using dependency injection can register the generator with `AddPdfReportGenerator()` and request `IPdfReportGenerator` from the service provider.

## Public methods

### `GenerateAsync`

```csharp
Task<byte[]> GenerateAsync(
    IReadOnlyList<ReportSection> sections,
    PdfReportOptions? options = null,
    CancellationToken cancellationToken = default)
```

Generates the complete PDF in memory and returns its bytes. Sections are rendered in list order. Passing `null` for `options` uses a new `PdfReportOptions` instance with the defaults described below.

- `sections` must not be `null`; otherwise, `ArgumentNullException` is thrown.
- An empty section list still produces a PDF containing the title page.
- Cancellation is checked while charts and sections are processed and can result in `OperationCanceledException`.

### `GenerateToFileAsync`

```csharp
Task GenerateToFileAsync(
    string outputPath,
    IReadOnlyList<ReportSection> sections,
    PdfReportOptions? options = null,
    CancellationToken cancellationToken = default)
```

Generates the report and writes it to `outputPath`. A missing parent directory is created automatically. Relative and absolute paths are supported.

- A null, empty, or whitespace-only `outputPath` causes `ArgumentNullException`.
- A null `sections` value causes `ArgumentNullException`.
- File-system and permission failures are propagated to the caller.
- The cancellation token is used during generation and the asynchronous file write.

## Report sections

Each `ReportSection` controls one logical block of report content:

| Property | Purpose | Default |
| --- | --- | --- |
| `Heading` | Optional heading followed by an accent-colored rule. | `null` |
| `BodyText` | Optional text placed below the heading and wrapped to the usable page width. | `null` |
| `Chart` | Optional chart rendered through `IChartRenderingService`. | `null` |
| `ImageFit` | Controls how the rendered chart image is scaled. | `PdfImageFit.FitWidth` |
| `PageBreakBefore` | Starts the section on a new page when a content page is already open. | `false` |

The generator inserts new pages when body text or a chart does not fit vertically. Chart rendering uses PNG output, `ChartDpi`, and an export quality of `0.95`. If rendering a chart throws or returns no successful image, the failure is logged and the report continues without that chart.

`PdfImageFit` supports the following modes:

- `Original`: preserves the raster image dimensions and can extend beyond the page.
- `FitWidth`: scales proportionally to the usable page width.
- `FitHeight`: scales proportionally to the usable page height.
- `FitPage`: scales proportionally to fit within both usable dimensions.

Images are never enlarged above their original dimensions.

## PdfReportOptions

`PdfReportOptions` controls document metadata, page layout, typography, and colors:

| Property | Default | How it is used |
| --- | --- | --- |
| `Title` | `"Chart Report"` | Drawn on the title page and stored as the PDF title metadata. |
| `Subtitle` | `null` | Drawn below the title when present and stored as the PDF subject metadata. |
| `PageWidth` | `595f` | Page width in PDF points; values below 100 throw `ArgumentOutOfRangeException`. |
| `PageHeight` | `842f` | Page height in PDF points; values below 100 throw `ArgumentOutOfRangeException`. |
| `Margin` | `40f` | Uniform content margin in points; negative values throw `ArgumentOutOfRangeException`. |
| `TitleFontSize` | `24f` | Title-page title size. |
| `HeadingFontSize` | `16f` | Section heading and title-page subtitle size. |
| `BodyFontSize` | `11f` | Body text and title-page date size; page numbers use one point less. |
| `ChartDpi` | `150` | DPI passed to chart PNG export. |
| `ShowPageNumbers` | `true` | Draws centered page numbers on the title and content pages. |
| `PageBackgroundColor` | `"#FFFFFF"` | Background color painted across every page. |
| `TextColor` | `"#1A1A1A"` | Body, subtitle, date, and page-number color. |
| `AccentColor` | `"#2E7D99"` | Title, section-heading, and decorative-rule color. |

Page dimensions and margins are expressed in PDF points, where 72 points equal one inch. Color properties accept values understood by `SKColor.TryParse`; an unparseable color falls back to black.

Every report begins with a title page containing the title, optional subtitle, and the current UTC date. Content starts on subsequent pages. The PDF author metadata is left empty.

## Example usage

The following example resolves the registered generator, creates text and chart sections, and writes the report to disk:

```csharp
using Microsoft.Extensions.DependencyInjection;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Reports;

IServiceProvider serviceProvider = /* configured application service provider */;
var reportGenerator = serviceProvider.GetRequiredService<IPdfReportGenerator>();

var salesChart = new Chart(ChartType.LineChart)
{
    Title = "Monthly sales"
};

var sections = new List<ReportSection>
{
    new()
    {
        Heading = "Executive summary",
        BodyText = "Sales remained strong throughout the reporting period."
    },
    new()
    {
        Heading = "Sales trend",
        BodyText = "The chart below shows the monthly trend.",
        Chart = salesChart,
        ImageFit = PdfImageFit.FitPage,
        PageBreakBefore = true
    }
};

var options = new PdfReportOptions
{
    Title = "Quarterly Sales Report",
    Subtitle = "Q3 2026",
    AccentColor = "#1565C0",
    ChartDpi = 200,
    ShowPageNumbers = true
};

await reportGenerator.GenerateToFileAsync(
    "reports/q3-sales.pdf",
    sections,
    options,
    cancellationToken);
```

To return a PDF from an API or save it through another storage abstraction, call `GenerateAsync` instead:

```csharp
byte[] pdf = await reportGenerator.GenerateAsync(
    sections,
    options,
    cancellationToken);
```
