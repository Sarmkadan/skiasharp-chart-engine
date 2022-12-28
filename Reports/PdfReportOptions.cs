// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Reports;

/// <summary>
/// Defines how chart images are scaled to fit a PDF page.
/// </summary>
public enum PdfImageFit
{
    /// <summary>Preserve original dimensions (may clip if too large).</summary>
    Original = 0,
    /// <summary>Scale proportionally to fit within the available width.</summary>
    FitWidth = 1,
    /// <summary>Scale proportionally to fit within the available height.</summary>
    FitHeight = 2,
    /// <summary>Scale proportionally so the image fits entirely within the page margins.</summary>
    FitPage = 3
}

/// <summary>
/// A single section within a PDF report containing an optional heading, text, and chart.
/// </summary>
public sealed class ReportSection
{
    /// <summary>Gets or sets the optional section heading.</summary>
    public string? Heading { get; set; }

    /// <summary>Gets or sets descriptive body text that appears below the heading.</summary>
    public string? BodyText { get; set; }

    /// <summary>Gets or sets the chart to render in this section, or <c>null</c> to render text only.</summary>
    public Chart? Chart { get; set; }

    /// <summary>Gets or sets how the chart image should be scaled on the page.</summary>
    public PdfImageFit ImageFit { get; set; } = PdfImageFit.FitWidth;

    /// <summary>Gets or sets whether to insert a page break before this section.</summary>
    public bool PageBreakBefore { get; set; } = false;
}

/// <summary>
/// Options controlling layout and appearance of a generated PDF report.
/// </summary>
public sealed class PdfReportOptions
{
    private float _pageWidth  = 595f;  // A4 in points
    private float _pageHeight = 842f;  // A4 in points
    private float _margin     = 40f;

    /// <summary>Gets or sets the report title shown on the first page.</summary>
    public string Title { get; set; } = "Chart Report";

    /// <summary>Gets or sets an optional subtitle shown below the title.</summary>
    public string? Subtitle { get; set; }

    /// <summary>Gets or sets the page width in PDF points (1 pt = 1/72 inch). Default is A4 (595).</summary>
    public float PageWidth
    {
        get => _pageWidth;
        set
        {
            if (value < 100) throw new ArgumentOutOfRangeException(nameof(value), "PageWidth must be at least 100 pt.");
            _pageWidth = value;
        }
    }

    /// <summary>Gets or sets the page height in PDF points. Default is A4 (842).</summary>
    public float PageHeight
    {
        get => _pageHeight;
        set
        {
            if (value < 100) throw new ArgumentOutOfRangeException(nameof(value), "PageHeight must be at least 100 pt.");
            _pageHeight = value;
        }
    }

    /// <summary>Gets or sets the uniform page margin in PDF points. Defaults to 40.</summary>
    public float Margin
    {
        get => _margin;
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Margin cannot be negative.");
            _margin = value;
        }
    }

    /// <summary>Gets or sets the title font size in points. Defaults to 24.</summary>
    public float TitleFontSize { get; set; } = 24f;

    /// <summary>Gets or sets the section heading font size in points. Defaults to 16.</summary>
    public float HeadingFontSize { get; set; } = 16f;

    /// <summary>Gets or sets the body text font size in points. Defaults to 11.</summary>
    public float BodyFontSize { get; set; } = 11f;

    /// <summary>Gets or sets the chart render DPI used when rasterizing chart images. Defaults to 150.</summary>
    public int ChartDpi { get; set; } = 150;

    /// <summary>Gets or sets whether to add a page number footer to each page. Defaults to <c>true</c>.</summary>
    public bool ShowPageNumbers { get; set; } = true;

    /// <summary>Gets or sets the background color of each page in hex format. Defaults to white.</summary>
    public string PageBackgroundColor { get; set; } = "#FFFFFF";

    /// <summary>Gets or sets the primary text color in hex format. Defaults to near-black.</summary>
    public string TextColor { get; set; } = "#1A1A1A";

    /// <summary>Gets or sets the accent color used for headings and rules in hex format.</summary>
    public string AccentColor { get; set; } = "#2E7D99";
}
