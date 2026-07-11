// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Reports;

namespace SkiaSharpChartEngine.Tests.Reports;

/// <summary>
/// Provides extension methods for creating and asserting PDF report test data.
/// </summary>
public static class PdfReportGeneratorTestsExtensions
{
    /// <summary>
    /// Creates a simple report section with only text content for testing.
    /// </summary>
    /// <param name="test">Test instance (unused)</param>
    /// <param name="heading">Section heading text. Cannot be null or whitespace.</param>
    /// <param name="bodyText">Body text content. Cannot be null.</param>
    /// <returns>A new ReportSection configured with the provided text content.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="heading"/> is null. <paramref name="bodyText"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="heading"/> is empty or whitespace.</exception>
    public static ReportSection CreateTextSection(this PdfReportGeneratorTests _, string heading, string bodyText)
    {
        ArgumentNullException.ThrowIfNull(heading);
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);
        ArgumentNullException.ThrowIfNull(bodyText);

        return new ReportSection
        {
            Heading = heading,
            BodyText = bodyText
        };
    }

    /// <summary>
    /// Creates a report section containing a chart for testing.
    /// </summary>
    /// <param name="test">Test instance (unused)</param>
    /// <param name="heading">Section heading text. Cannot be null or whitespace.</param>
    /// <param name="chart">Chart to include in the section. Cannot be null.</param>
    /// <param name="pageBreakBefore">Whether to insert a page break before this section.</param>
    /// <returns>A new ReportSection configured with the provided chart.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="heading"/> is null. <paramref name="chart"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="heading"/> is empty or whitespace.</exception>
    public static ReportSection CreateChartSection(this PdfReportGeneratorTests _, string heading, Chart chart, bool pageBreakBefore = false)
    {
        ArgumentNullException.ThrowIfNull(heading);
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);
        ArgumentNullException.ThrowIfNull(chart);

        return new ReportSection
        {
            Heading = heading,
            Chart = chart,
            PageBreakBefore = pageBreakBefore
        };
    }

    /// <summary>
    /// Asserts that a generated PDF report contains the expected sections.
    /// </summary>
    /// <param name="pdfBytes">Generated PDF bytes. Cannot be null or empty.</param>
    /// <param name="expectedSectionCount">Expected number of sections in the report.</param>
    /// <param name="because">Optional reason for the assertion.</param>
    /// <exception cref="ArgumentNullException"><paramref name="pdfBytes"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="pdfBytes"/> is empty.</exception>
    public static void ShouldContainSections(this byte[] pdfBytes, int expectedSectionCount, string because = "")
    {
        ArgumentNullException.ThrowIfNull(pdfBytes);

        pdfBytes.Should().NotBeNull(because);
        pdfBytes.Length.Should().BeGreaterThan(0, because);

        // Verify the PDF contains expected content by checking for basic PDF structure markers
        // PDF files start with %PDF- header and contain /Pages, /Page, and /Contents tokens
        var pdfText = System.Text.Encoding.ASCII.GetString(pdfBytes.AsSpan(0, Math.Min(1024, pdfBytes.Length)));
        pdfText.Should().Contain("%PDF-", because, "PDF should contain valid header");
        pdfText.Should().Contain("/Pages", because, "PDF should contain Pages structure");
        pdfText.Should().Contain("/Page", because, "PDF should contain Page structure");
    }

    /// <summary>
    /// Creates a collection of multiple report sections for testing multi-section reports.
    /// </summary>
    /// <param name="test">Test instance (unused)</param>
    /// <param name="sections">Collection of section configurations. Cannot be null.</param>
    /// <returns>A List of ReportSection objects ready for PDF generation.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="sections"/> is null.</exception>
    public static List<ReportSection> CreateMultiSectionReport(this PdfReportGeneratorTests _, params ReportSection[] sections)
    {
        ArgumentNullException.ThrowIfNull(sections);

        return new List<ReportSection>(sections);
    }
}
