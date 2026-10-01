// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Orchestrates chart rendering by delegating to specialized renderers
/// based on chart type, and composing axis, legend, and tooltip layers.
/// </summary>
public class ChartRenderer : IDisposable
{
    private readonly ILogger<ChartRenderer> _logger;
    private readonly Dictionary<string, IChartRenderer> _renderers = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="ChartRenderer"/>.
    /// </summary>
    /// <param name="logger">Logger for render diagnostics.</param>
    public ChartRenderer(ILogger<ChartRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a renderer for a specific chart type name (e.g. "bar", "line").
    /// </summary>
    /// <param name="chartType">Case-insensitive chart type key.</param>
    /// <param name="renderer">The renderer implementation.</param>
    public void RegisterRenderer(string chartType, IChartRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(chartType);
        ArgumentNullException.ThrowIfNull(renderer);
        _renderers[chartType] = renderer;
        _logger.LogDebug("Registered renderer for chart type '{ChartType}'", chartType);
    }

    /// <summary>
    /// Renders a chart onto a new bitmap surface of the given dimensions.
    /// </summary>
    /// <param name="chart">The chart model to render.</param>
    /// <param name="width">Bitmap width in pixels.</param>
    /// <param name="height">Bitmap height in pixels.</param>
    /// <returns>The rendered <see cref="SKBitmap"/>, caller owns disposal.</returns>
    public SKBitmap RenderToBitmap(Chart chart, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(chart);
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException("Width and height must be positive");

        var sw = Stopwatch.StartNew();
        var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var bounds = new SKRect(0, 0, width, height);
        RenderToCanvas(canvas, chart, bounds);

        sw.Stop();
        _logger.LogInformation("Rendered chart '{Title}' in {Elapsed}ms",
            chart.Title, sw.ElapsedMilliseconds);
        return bitmap;
    }

    /// <summary>
    /// Renders a chart onto an existing canvas within the specified bounds.
    /// Selects the appropriate renderer based on <see cref="Chart.ChartType"/>.
    /// </summary>
    /// <param name="canvas">Target SkiaSharp canvas.</param>
    /// <param name="chart">The chart model.</param>
    /// <param name="bounds">Drawing region.</param>
    public void RenderToCanvas(SKCanvas canvas, Chart chart, SKRect bounds)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ArgumentNullException.ThrowIfNull(chart);

        string type = chart.ChartType?.ToString() ?? "line";

        if (!_renderers.TryGetValue(type, out var renderer))
        {
            _logger.LogWarning("No renderer registered for chart type '{Type}', skipping", type);
            return;
        }

        renderer.Render(canvas, chart, bounds);
    }

    /// <summary>
    /// Exports the chart as a PNG byte array.
    /// </summary>
    /// <param name="chart">The chart to export.</param>
    /// <param name="width">Image width.</param>
    /// <param name="height">Image height.</param>
    /// <param name="quality">PNG quality 0-100.</param>
    /// <returns>PNG-encoded byte array.</returns>
    public byte[] ExportPng(Chart chart, int width, int height, int quality = 95)
    {
        using var bitmap = RenderToBitmap(chart, width, height);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, quality);
        return data.ToArray();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _renderers.Clear();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
