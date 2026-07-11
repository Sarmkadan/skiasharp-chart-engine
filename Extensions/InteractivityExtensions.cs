// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

namespace SkiaSharpChartEngine.Extensions;

/// <summary>
/// Provides extension methods for registering interactive chart services with the DI container
/// and for performing tooltip hit-testing, zooming, panning, and viewport resetting on <see cref="Chart"/> instances.
/// </summary>
public static class InteractivityExtensions
{
    /// <summary>
    /// Registers <see cref="IInteractivityService"/> (and its default implementation
    /// <see cref="InteractivityService"/>) as a singleton in the service collection.
    /// Call this from your composition root or alongside
    /// <c>AddSkiaSharpChartEngine()</c>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddChartInteractivity(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IInteractivityService, InteractivityService>();
        return services;
    }

    /// <summary>
    /// Convenience overload of <see cref="GetTooltipAt"/> that accepts the pointer as an
    /// <see cref="SKPoint"/>, matching the coordinate type used by SkiaSharp touch/mouse events
    /// in MAUI and Blazor WASM hosts.
    /// </summary>
    public static TooltipHitResult GetTooltipAt(
        this Chart chart,
        SKPoint point,
        float canvasWidth, float canvasHeight,
        TooltipOptions? options = null,
        ViewportState? viewport = null)
    {
        ArgumentNullException.ThrowIfNull(chart);
        return _defaultService.HitTest(chart, point, canvasWidth, canvasHeight, options, viewport);
    }

    /// <summary>
    /// Performs a synchronous tooltip hit-test against all visible series in the chart,
    /// returning the nearest data point within the configured <see cref="TooltipOptions.HitRadius"/>.
    /// </summary>
    /// <param name="chart">The chart to test against.</param>
    /// <param name="pointerX">Canvas X coordinate of the pointer in pixels.</param>
    /// <param name="pointerY">Canvas Y coordinate of the pointer in pixels.</param>
    /// <param name="canvasWidth">Total canvas width in pixels.</param>
    /// <param name="canvasHeight">Total canvas height in pixels.</param>
    /// <param name="options">Optional tooltip configuration; defaults are used when <c>null</c>.</param>
    /// <param name="viewport">Optional active viewport; full data range is assumed when <c>null</c>.</param>
    /// <returns>
    /// A <see cref="TooltipHitResult"/> describing the nearest point, or
    /// <see cref="TooltipHitResult.Miss"/> when nothing is within range.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> is <c>null</c>.</exception>
    public static TooltipHitResult GetTooltipAt(
        this Chart chart,
        float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        TooltipOptions? options = null,
        ViewportState? viewport = null)
    {
        ArgumentNullException.ThrowIfNull(chart);
        return _defaultService.HitTest(chart, pointerX, pointerY, canvasWidth, canvasHeight, options, viewport);
    }

    /// <summary>
    /// Asynchronous overload of <see cref="GetTooltipAt"/>.
    /// Respects <paramref name="cancellationToken"/> before performing the hit-test.
    /// </summary>
    /// <param name="chart">The chart to test against.</param>
    /// <param name="pointerX">Canvas X coordinate of the pointer in pixels.</param>
    /// <param name="pointerY">Canvas Y coordinate of the pointer in pixels.</param>
    /// <param name="canvasWidth">Total canvas width in pixels.</param>
    /// <param name="canvasHeight">Total canvas height in pixels.</param>
    /// <param name="options">Optional tooltip configuration.</param>
    /// <param name="viewport">Optional active viewport.</param>
    /// <param name="cancellationToken">Token to observe for cancellation.</param>
    /// <returns>A task that resolves to the nearest <see cref="TooltipHitResult"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> is <c>null</c>.</exception>
    public static Task<TooltipHitResult> GetTooltipAtAsync(
        this Chart chart,
        float pointerX, float pointerY,
        float canvasWidth, float canvasHeight,
        TooltipOptions? options = null,
        ViewportState? viewport = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(chart);
        return _defaultService.HitTestAsync(
            chart, pointerX, pointerY, canvasWidth, canvasHeight, options, viewport, cancellationToken);
    }

    /// <summary>
    /// Returns a new <see cref="ViewportState"/> zoomed around the specified canvas anchor point.
    /// A factor above 1.0 zooms in; below 1.0 zooms out.
    /// </summary>
    /// <param name="chart">The chart whose data bounds constrain the zoom.</param>
    /// <param name="current">The current viewport state.</param>
    /// <param name="anchorX">Canvas X coordinate of the zoom anchor in pixels.</param>
    /// <param name="anchorY">Canvas Y coordinate of the zoom anchor in pixels.</param>
    /// <param name="canvasWidth">Total canvas width in pixels.</param>
    /// <param name="canvasHeight">Total canvas height in pixels.</param>
    /// <param name="factor">Multiplicative zoom factor (must be &gt; 0).</param>
    /// <returns>A new <see cref="ViewportState"/> reflecting the zoom.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> or <paramref name="current"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="factor"/> is &lt;= 0.</exception>
    public static ViewportState ZoomAt(
        this Chart chart,
        ViewportState current,
        float anchorX, float anchorY,
        float canvasWidth, float canvasHeight,
        double factor)
    {
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(current);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(factor, 0);

        return _defaultService.Zoom(chart, current, anchorX, anchorY, canvasWidth, canvasHeight, factor);
    }

    /// <summary>
    /// Returns a new <see cref="ViewportState"/> translated by a pixel delta.
    /// Positive <paramref name="deltaX"/> pans right; positive <paramref name="deltaY"/> pans down.
    /// </summary>
    /// <param name="chart">The chart whose data bounds constrain the pan.</param>
    /// <param name="current">The current viewport state.</param>
    /// <param name="deltaX">Horizontal drag delta in pixels.</param>
    /// <param name="deltaY">Vertical drag delta in pixels.</param>
    /// <param name="canvasWidth">Total canvas width in pixels.</param>
    /// <param name="canvasHeight">Total canvas height in pixels.</param>
    /// <returns>A new <see cref="ViewportState"/> reflecting the pan.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> or <paramref name="current"/> is <c>null</c>.</exception>
    public static ViewportState PanBy(
        this Chart chart,
        ViewportState current,
        float deltaX, float deltaY,
        float canvasWidth, float canvasHeight)
    {
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(current);
        return _defaultService.Pan(chart, current, deltaX, deltaY, canvasWidth, canvasHeight);
    }

    /// <summary>
    /// Returns a <see cref="ViewportState"/> that shows the full extent of the chart's data,
    /// effectively resetting any active zoom or pan.
    /// </summary>
    /// <param name="chart">The chart whose data bounds define the full viewport.</param>
    /// <returns>A new default <see cref="ViewportState"/> covering all data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> is <c>null</c>.</exception>
    public static ViewportState ResetViewport(this Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);
        return _defaultService.ResetViewport(chart);
    }

    // Shared service instance used by extension methods that operate without a DI container.
    // NullLogger suppresses output; callers that need structured logs should resolve
    // IInteractivityService from the container instead.
    private static readonly IInteractivityService _defaultService =
        new InteractivityService(NullLogger<InteractivityService>.Instance);
}
