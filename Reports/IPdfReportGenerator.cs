// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SkiaSharpChartEngine.Reports;

/// <summary>
/// Generates PDF reports that embed one or more rendered chart images along with
/// descriptive text and report metadata.
/// </summary>
public interface IPdfReportGenerator
{
    /// <summary>
    /// Generates a PDF report and returns it as an in-memory byte array.
    /// </summary>
    /// <param name="sections">Ordered list of report sections to include.</param>
    /// <param name="options">Layout and appearance options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The PDF file contents as a byte array.</returns>
    Task<byte[]> GenerateAsync(
        IReadOnlyList<ReportSection> sections,
        PdfReportOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a PDF report and writes it to the specified file path.
    /// </summary>
    /// <param name="outputPath">Absolute or relative path of the output PDF file.</param>
    /// <param name="sections">Ordered list of report sections to include.</param>
    /// <param name="options">Layout and appearance options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task GenerateToFileAsync(
        string outputPath,
        IReadOnlyList<ReportSection> sections,
        PdfReportOptions? options = null,
        CancellationToken cancellationToken = default);
}
