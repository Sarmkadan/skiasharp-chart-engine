// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Defines the contract for exporting rendered charts to various output formats
/// (PNG, JPEG, SVG, PDF) with configurable resolution and compression settings.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports a chart in the format specified by <paramref name="options"/> asynchronously.
    /// </summary>
    /// <param name="chart">The chart definition to render and export.</param>
    /// <param name="options">Export configuration including format, DPI, quality, and output path.</param>
    /// <param name="cancellationToken">Token to cancel the export operation.</param>
    /// <returns>A <see cref="RenderResult"/> containing the exported data and metadata.</returns>
    Task<RenderResult> ExportAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports a chart synchronously using the specified options.
    /// </summary>
    /// <param name="chart">The chart definition to render and export.</param>
    /// <param name="options">Export configuration.</param>
    /// <returns>A <see cref="RenderResult"/> containing the exported data.</returns>
    RenderResult Export(Chart chart, ExportOptions options);

    /// <summary>
    /// Checks whether the given export format is supported by this service implementation.
    /// </summary>
    /// <param name="format">The export format to check.</param>
    /// <returns><c>true</c> if the format is supported; otherwise <c>false</c>.</returns>
    bool SupportsFormat(ExportFormat format);

    /// <summary>
    /// Returns all export formats supported by this service implementation.
    /// </summary>
    /// <returns>An enumerable of supported <see cref="ExportFormat"/> values.</returns>
    IEnumerable<ExportFormat> GetSupportedFormats();

    /// <summary>
    /// Exports chart series data to CSV format.
    /// </summary>
    /// <param name="chart">The chart containing series to export.</param>
    /// <param name="options">Export configuration including output path.</param>
    /// <returns>A <see cref="RenderResult"/> containing the CSV data and metadata.</returns>
    Task<RenderResult> ExportToCsvAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports chart series data to TSV format.
    /// </summary>
    /// <param name="chart">The chart containing series to export.</param>
    /// <param name="options">Export configuration including output path.</param>
    /// <returns>A <see cref="RenderResult"/> containing the TSV data and metadata.</returns>
    Task<RenderResult> ExportToTsvAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports chart series data to CSV format synchronously.
    /// </summary>
    /// <param name="chart">The chart containing series to export.</param>
    /// <param name="options">Export configuration.</param>
    /// <returns>A <see cref="RenderResult"/> containing the CSV data.</returns>
    RenderResult ExportToCsv(Chart chart, ExportOptions options);

    /// <summary>
    /// Exports chart series data to TSV format synchronously.
    /// </summary>
    /// <param name="chart">The chart containing series to export.</param>
    /// <param name="options">Export configuration.</param>
    /// <returns>A <see cref="RenderResult"/> containing the TSV data.</returns>
    RenderResult ExportToTsv(Chart chart, ExportOptions options);
}
