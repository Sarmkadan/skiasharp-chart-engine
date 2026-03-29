// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Interface for chart rendering operations
/// </summary>
public interface IChartRenderingService
{
    Task<RenderResult> RenderToByteArrayAsync(Chart chart, CancellationToken cancellationToken = default);

    Task<RenderResult> RenderToFileAsync(Chart chart, string outputPath, CancellationToken cancellationToken = default);

    Task<RenderResult> RenderWithExportAsync(Chart chart, ExportOptions exportOptions, CancellationToken cancellationToken = default);

    RenderResult RenderToByteArray(Chart chart);

    RenderResult RenderToFile(Chart chart, string outputPath);

    void PrewarmCache(Chart chart);
}
