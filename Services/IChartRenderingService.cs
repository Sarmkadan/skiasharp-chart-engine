// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Defines the contract for rendering chart definitions into rasterized images
/// using SkiaSharp. Supports both synchronous and asynchronous rendering with
/// optional file output and export format conversion.
/// </summary>
public interface IChartRenderingService
{
    /// <summary>
    /// Renders a chart to an in-memory byte array asynchronously.
    /// </summary>
    /// <param name="chart">The chart definition containing data series, axes, and styling configuration.</param>
    /// <param name="cancellationToken">Token to cancel the rendering operation.</param>
    /// <returns>A <see cref="RenderResult"/> containing the rendered image bytes and metadata.</returns>
    Task<RenderResult> RenderToByteArrayAsync(Chart chart, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a chart and writes the output directly to a file on disk.
    /// </summary>
    /// <param name="chart">The chart definition to render.</param>
    /// <param name="outputPath">Absolute or relative path where the rendered image file will be saved.</param>
    /// <param name="cancellationToken">Token to cancel the rendering operation.</param>
    /// <returns>A <see cref="RenderResult"/> with the file path and render metadata.</returns>
    Task<RenderResult> RenderToFileAsync(Chart chart, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a chart and applies export options (format, DPI, compression quality).
    /// </summary>
    /// <param name="chart">The chart definition to render.</param>
    /// <param name="exportOptions">Export settings controlling output format (PNG/JPEG/SVG/PDF), resolution, and quality.</param>
    /// <param name="cancellationToken">Token to cancel the rendering operation.</param>
    /// <returns>A <see cref="RenderResult"/> in the requested export format.</returns>
    Task<RenderResult> RenderWithExportAsync(Chart chart, ExportOptions exportOptions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a chart to an in-memory byte array synchronously.
    /// Prefer the async overload for server-side scenarios to avoid thread pool starvation.
    /// </summary>
    /// <param name="chart">The chart definition to render.</param>
    /// <returns>A <see cref="RenderResult"/> containing the rendered image bytes and metadata.</returns>
    RenderResult RenderToByteArray(Chart chart);

    /// <summary>
    /// Renders a chart and writes the output to a file synchronously.
    /// </summary>
    /// <param name="chart">The chart definition to render.</param>
    /// <param name="outputPath">File path for the rendered image output.</param>
    /// <returns>A <see cref="RenderResult"/> with the file path and render metadata.</returns>
    RenderResult RenderToFile(Chart chart, string outputPath);

    /// <summary>
    /// Pre-populates internal rendering caches (font metrics, gradient shaders) for the
    /// given chart configuration. Call before rendering in latency-sensitive paths to
    /// avoid first-render overhead.
    /// </summary>
    /// <param name="chart">The chart definition whose resources should be pre-cached.</param>
    void PrewarmCache(Chart chart);
}
