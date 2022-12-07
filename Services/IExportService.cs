// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Interface for exporting charts to various formats
/// </summary>
public interface IExportService
{
    Task<RenderResult> ExportAsync(Chart chart, ExportOptions options, CancellationToken cancellationToken = default);

    RenderResult Export(Chart chart, ExportOptions options);

    bool SupportsFormat(ExportFormat format);

    IEnumerable<ExportFormat> GetSupportedFormats();
}
