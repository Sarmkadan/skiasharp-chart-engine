// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

namespace SkiaSharpChartEngine.Reports;

/// <summary>
/// Default implementation of <see cref="IPdfReportGenerator"/>.
/// Uses SkiaSharp's built-in PDF canvas to produce vectorized page layouts with
/// rasterized chart images embedded at each section.
/// </summary>
public sealed class PdfReportGenerator : IPdfReportGenerator
{
    private readonly IChartRenderingService _renderingService;
    private readonly ILogger<PdfReportGenerator> _logger;

    public PdfReportGenerator(
        IChartRenderingService renderingService,
        ILogger<PdfReportGenerator> logger)
    {
        _renderingService = renderingService ?? throw new ArgumentNullException(nameof(renderingService));
        _logger           = logger           ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<byte[]> GenerateAsync(
        IReadOnlyList<ReportSection> sections,
        PdfReportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (sections == null) throw new ArgumentNullException(nameof(sections));
        options ??= new PdfReportOptions();

        using var stream = new MemoryStream();
        await WriteAsync(stream, sections, options, cancellationToken);
        return stream.ToArray();
    }

    /// <inheritdoc/>
    public async Task GenerateToFileAsync(
        string outputPath,
        IReadOnlyList<ReportSection> sections,
        PdfReportOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(outputPath)) throw new ArgumentNullException(nameof(outputPath));
        if (sections == null) throw new ArgumentNullException(nameof(sections));
        options ??= new PdfReportOptions();

        var directory = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var bytes = await GenerateAsync(sections, options, cancellationToken);
        await File.WriteAllBytesAsync(outputPath, bytes, cancellationToken);

        _logger.LogInformation("PDF report written – path={Path}, size={Size} bytes", outputPath, bytes.Length);
    }

    // -------------------------------------------------------------------------
    private async Task WriteAsync(
        Stream output,
        IReadOnlyList<ReportSection> sections,
        PdfReportOptions opts,
        CancellationToken ct)
    {
        // Pre-render all charts before opening the PDF canvas so any async work completes first.
        var chartImages = await PreRenderChartsAsync(sections, opts, ct);

        using var pdfStream = new SKDynamicMemoryWStream();
        var docMetadata = new SKDocumentPdfMetadata
        {
            Title   = opts.Title,
            Author  = string.Empty,
            Subject = opts.Subtitle ?? string.Empty
        };

        using var document = SKDocument.CreatePdf(pdfStream, docMetadata);

        int pageNumber = 1;

        // Title page
        using (var canvas = document.BeginPage(opts.PageWidth, opts.PageHeight))
        {
            DrawPageBackground(canvas, opts);
            DrawTitlePage(canvas, opts);
            if (opts.ShowPageNumbers)
                DrawPageNumber(canvas, pageNumber, opts);
            document.EndPage();
            pageNumber++;
        }

        // Content pages
        float y            = opts.Margin;
        SKCanvas? canvas2  = null;

        void EnsurePage()
        {
            if (canvas2 == null)
            {
                canvas2 = document.BeginPage(opts.PageWidth, opts.PageHeight);
                DrawPageBackground(canvas2, opts);
                y = opts.Margin;
            }
        }

        void FinishPage()
        {
            if (canvas2 == null) return;
            if (opts.ShowPageNumbers)
                DrawPageNumber(canvas2, pageNumber, opts);
            document.EndPage();
            canvas2 = null;
            pageNumber++;
            y = opts.Margin;
        }

        float usableWidth = opts.PageWidth  - opts.Margin * 2;
        float usableHeight = opts.PageHeight - opts.Margin * 2;

        for (int i = 0; i < sections.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var section = sections[i];

            if (section.PageBreakBefore && canvas2 != null)
                FinishPage();

            EnsurePage();

            // Section heading
            if (!string.IsNullOrWhiteSpace(section.Heading))
            {
                using var paint = MakePaint(opts.AccentColor, opts.HeadingFontSize, bold: true);
                canvas2!.DrawText(section.Heading, opts.Margin, y + opts.HeadingFontSize, paint);
                y += opts.HeadingFontSize + 6f;

                // Decorative rule
                using var rulePaint = MakePaint(opts.AccentColor, 1);
                canvas2.DrawLine(opts.Margin, y, opts.PageWidth - opts.Margin, y, rulePaint);
                y += 8f;
            }

            // Body text (simple single-paragraph, no word-wrap for brevity)
            if (!string.IsNullOrWhiteSpace(section.BodyText))
            {
                using var paint = MakePaint(opts.TextColor, opts.BodyFontSize);
                foreach (var line in WrapText(section.BodyText, paint, usableWidth))
                {
                    if (y + opts.BodyFontSize > opts.PageHeight - opts.Margin)
                    {
                        FinishPage();
                        EnsurePage();
                    }
                    canvas2!.DrawText(line, opts.Margin, y + opts.BodyFontSize, paint);
                    y += opts.BodyFontSize + 3f;
                }
                y += 8f;
            }

            // Chart image
            if (chartImages.TryGetValue(i, out var imgBytes) && imgBytes.Length > 0)
            {
                using var skData   = SKData.CreateCopy(imgBytes);
                using var skBitmap = SKBitmap.Decode(skData);
                if (skBitmap != null)
                {
                    var (imgW, imgH) = ScaleImage(skBitmap.Width, skBitmap.Height, usableWidth, usableHeight, section.ImageFit);

                    if (y + imgH > opts.PageHeight - opts.Margin)
                    {
                        FinishPage();
                        EnsurePage();
                    }

                    var destRect = new SKRect(opts.Margin, y, opts.Margin + imgW, y + imgH);
                    canvas2!.DrawBitmap(skBitmap, destRect);
                    y += imgH + 16f;
                }
            }

            y += 12f; // gap between sections
        }

        FinishPage();
        document.Close();

        // Copy PDF bytes to output stream
        var pdfData = pdfStream.DetachAsData();
        var span    = pdfData.AsSpan();
        output.Write(span);

        _logger.LogInformation("PDF report generated – pages={Pages}, sections={Sections}", pageNumber - 1, sections.Count);
    }

    // -------------------------------------------------------------------------

    private async Task<Dictionary<int, byte[]>> PreRenderChartsAsync(
        IReadOnlyList<ReportSection> sections,
        PdfReportOptions opts,
        CancellationToken ct)
    {
        var result = new Dictionary<int, byte[]>();

        for (int i = 0; i < sections.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var section = sections[i];
            if (section.Chart == null) continue;

            try
            {
                var exportOptions = new ExportOptions("report-chart", ExportFormat.PNG, opts.ChartDpi, 0.95f);
                var render = await _renderingService.RenderWithExportAsync(section.Chart, exportOptions, ct);
                if (render.Success && render.ImageData != null)
                    result[i] = render.ImageData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render chart for section {Index}", i);
            }
        }

        return result;
    }

    private static void DrawPageBackground(SKCanvas canvas, PdfReportOptions opts)
    {
        using var paint = new SKPaint { Color = ParseColor(opts.PageBackgroundColor), IsAntialias = true };
        canvas.DrawRect(0, 0, opts.PageWidth, opts.PageHeight, paint);
    }

    private static void DrawTitlePage(SKCanvas canvas, PdfReportOptions opts)
    {
        float centerX = opts.PageWidth / 2f;
        float centerY = opts.PageHeight / 2f;

        using var titlePaint = MakePaint(opts.AccentColor, opts.TitleFontSize, bold: true);
        titlePaint.TextAlign = SKTextAlign.Center;
        canvas.DrawText(opts.Title, centerX, centerY - 20f, titlePaint);

        if (!string.IsNullOrWhiteSpace(opts.Subtitle))
        {
            using var subPaint = MakePaint(opts.TextColor, opts.HeadingFontSize);
            subPaint.TextAlign = SKTextAlign.Center;
            canvas.DrawText(opts.Subtitle, centerX, centerY + 10f, subPaint);
        }

        // Date
        using var datePaint = MakePaint(opts.TextColor, opts.BodyFontSize);
        datePaint.TextAlign = SKTextAlign.Center;
        canvas.DrawText(DateTime.UtcNow.ToString("MMMM dd, yyyy"), centerX, centerY + 40f, datePaint);
    }

    private static void DrawPageNumber(SKCanvas canvas, int pageNumber, PdfReportOptions opts)
    {
        using var paint = MakePaint(opts.TextColor, opts.BodyFontSize - 1f);
        paint.TextAlign = SKTextAlign.Center;
        canvas.DrawText(pageNumber.ToString(), opts.PageWidth / 2f, opts.PageHeight - opts.Margin / 2f, paint);
    }

    private static SKPaint MakePaint(string hexColor, float fontSize, bool bold = false)
    {
        return new SKPaint
        {
            Color       = ParseColor(hexColor),
            TextSize    = fontSize,
            IsAntialias = true,
            Typeface    = bold
                ? SKTypeface.FromFamilyName(null, SKFontStyle.Bold)
                : SKTypeface.FromFamilyName(null, SKFontStyle.Normal)
        };
    }

    private static SKColor ParseColor(string hex)
    {
        if (SKColor.TryParse(hex, out var color))
            return color;
        return SKColors.Black;
    }

    private static IEnumerable<string> WrapText(string text, SKPaint paint, float maxWidth)
    {
        var words  = text.Split(' ');
        var line   = new System.Text.StringBuilder();

        foreach (var word in words)
        {
            var candidate = line.Length == 0 ? word : $"{line} {word}";
            if (paint.MeasureText(candidate) <= maxWidth)
            {
                line.Clear();
                line.Append(candidate);
            }
            else
            {
                if (line.Length > 0)
                {
                    yield return line.ToString();
                    line.Clear();
                }
                line.Append(word);
            }
        }

        if (line.Length > 0)
            yield return line.ToString();
    }

    private static (float width, float height) ScaleImage(
        float srcW, float srcH, float maxW, float maxH, PdfImageFit fit)
    {
        return fit switch
        {
            PdfImageFit.Original  => (srcW, srcH),
            PdfImageFit.FitWidth  => ScaleProportional(srcW, srcH, maxW, float.MaxValue),
            PdfImageFit.FitHeight => ScaleProportional(srcW, srcH, float.MaxValue, maxH),
            PdfImageFit.FitPage   => ScaleProportional(srcW, srcH, maxW, maxH),
            _                     => (srcW, srcH)
        };
    }

    private static (float, float) ScaleProportional(float srcW, float srcH, float maxW, float maxH)
    {
        if (srcW <= 0 || srcH <= 0) return (0, 0);
        var scaleW = maxW / srcW;
        var scaleH = maxH / srcH;
        var scale  = Math.Min(scaleW, scaleH);
        scale      = Math.Min(scale, 1f); // never upscale
        return (srcW * scale, srcH * scale);
    }
}
