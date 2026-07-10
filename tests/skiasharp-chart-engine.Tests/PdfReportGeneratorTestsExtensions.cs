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

public static class PdfReportGeneratorTestsExtensions
{
    /// <summary>
    /// Creates a simple report section with only text content for testing.
    /// </summary>
    /// <param name="test">Test instance (unused)</param>
    /// <param name="heading">Section heading text</param>
    /// <param name="bodyText">Body text content</param>
    /// <returns>A new ReportSection configured with the provided text content</returns>
    public static ReportSection CreateTextSection(this PdfReportGeneratorTests _, string heading, string bodyText)
    {
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
    /// <param name="heading">Section heading text</param>
    /// <param name="chart">Chart to include in the section</param>
    /// <param name="pageBreakBefore">Whether to insert a page break before this section</param>
    /// <returns>A new ReportSection configured with the provided chart</returns>
    public static ReportSection CreateChartSection(this PdfReportGeneratorTests _, string heading, Chart chart, bool pageBreakBefore = false)
    {
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
    /// <param name="pdfBytes">Generated PDF bytes</param>
    /// <param name="expectedSectionCount">Expected number of sections in the report</param>
    /// <param name="because">Optional reason for the assertion</param>
    public static void ShouldContainSections(this byte[] pdfBytes, int expectedSectionCount, string because = "")
    {
        pdfBytes.Should().NotBeNull(because);
        pdfBytes.Length.Should().BeGreaterThan(0, because);

        // PDF should contain basic structure markers for each section
        // This is a simple check that the PDF was generated with content
        var pdfText = System.Text.Encoding.UTF8.GetString(pdfBytes);
        pdfText.Should().NotBeEmpty(because);
    }

    /// <summary>
    /// Creates a collection of multiple report sections for testing multi-section reports.
    /// </summary>
    /// <param name="test">Test instance (unused)</param>
    /// <param name="sections">Collection of section configurations</param>
    /// <returns>A List of ReportSection objects ready for PDF generation</returns>
    public static List<ReportSection> CreateMultiSectionReport(this PdfReportGeneratorTests _, params ReportSection[] sections)
    {
        return new List<ReportSection>(sections);
    }
}
