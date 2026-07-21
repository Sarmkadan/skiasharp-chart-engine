// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================
// Common interface for all chart renderers in the SkiaSharpChartEngine library.
// Provides a unified contract for rendering different chart types to a SkiaSharp canvas.
// =============================================================================

using SkiaSharp;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Common interface for chart renderers.
/// All chart renderers must implement this interface to ensure consistent rendering API.
/// </summary>
public interface IChartRenderer
{
    /// <summary>
    /// Renders the chart onto the supplied canvas.
    /// </summary>
    /// <param name="canvas">The SkiaSharp canvas to draw on.</param>
    /// <param name="chart">The chart model containing series and data points.</param>
    /// <param name="bounds">The drawing bounds within the canvas.</param>
    void Render(SKCanvas canvas, Chart chart, SKRect bounds);
}
