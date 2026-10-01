// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Renders tooltip overlays on a chart canvas, showing data point details
/// at a specified screen position. Handles background, border, and text layout.
/// </summary>
public class TooltipOverlay : IDisposable
{
    private const float DefaultFontSize = 11f;
    private const float Padding = 8f;
    private const float CornerRadius = 4f;
    private const float BorderWidth = 1f;
    private const float ShadowOffsetX = 2f;
    private const float ShadowOffsetY = 2f;
    private const float LineHeight = 16f;

    private readonly ILogger<TooltipOverlay> _logger;
    private SKPaint? _backgroundPaint;
    private SKPaint? _borderPaint;
    private SKPaint? _textPaint;
    private SKPaint? _shadowPaint;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="TooltipOverlay"/>.
    /// </summary>
    /// <param name="logger">Logger for tooltip diagnostics.</param>
    public TooltipOverlay(ILogger<TooltipOverlay> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializePaints();
    }

    private void InitializePaints()
    {
        _backgroundPaint = new SKPaint
        {
            Color = new SKColor(0xFF, 0xFF, 0xFF, 0xF0),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        _borderPaint = new SKPaint
        {
            Color = new SKColor(0xCC, 0xCC, 0xCC),
            Style = SKPaintStyle.Stroke,
            StrokeWidth = BorderWidth,
            IsAntialias = true
        };
        _textPaint = new SKPaint
        {
            Color = new SKColor(0x33, 0x33, 0x33),
            TextSize = DefaultFontSize,
            IsAntialias = true
        };
        _shadowPaint = new SKPaint
        {
            Color = new SKColor(0x00, 0x00, 0x00, 0x20),
            Style = SKPaintStyle.Fill,
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3f)
        };
    }

    /// <summary>
    /// Renders a tooltip at the specified position showing the data point value.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="position">Screen position for the tooltip anchor.</param>
    /// <param name="seriesName">Name of the series.</param>
    /// <param name="x">X value of the data point.</param>
    /// <param name="y">Y value of the data point.</param>
    /// <param name="valueFormat">Format string for the Y value.</param>
    public void Render(SKCanvas canvas, SKPoint position, string seriesName,
        double x, double y, string valueFormat = "N2")
    {
        ArgumentNullException.ThrowIfNull(canvas);
        ObjectDisposedException.ThrowIf(_disposed, this);

        string line1 = seriesName;
        string line2 = $"X: {x.ToString("G", CultureInfo.InvariantCulture)}";
        string line3 = $"Y: {y.ToString(valueFormat, CultureInfo.InvariantCulture)}";

        float maxTextWidth = Math.Max(_textPaint!.MeasureText(line1),
            Math.Max(_textPaint.MeasureText(line2), _textPaint.MeasureText(line3)));

        float boxWidth = maxTextWidth + Padding * 2;
        float boxHeight = LineHeight * 3 + Padding * 2;

        var tooltipRect = ClampToCanvas(position, boxWidth, boxHeight, canvas);

        // Shadow
        var shadowRect = SKRect.Create(
            tooltipRect.Left + ShadowOffsetX, tooltipRect.Top + ShadowOffsetY,
            tooltipRect.Width, tooltipRect.Height);
        canvas.DrawRoundRect(shadowRect, CornerRadius, CornerRadius, _shadowPaint!);

        // Background + border
        canvas.DrawRoundRect(tooltipRect, CornerRadius, CornerRadius, _backgroundPaint!);
        canvas.DrawRoundRect(tooltipRect, CornerRadius, CornerRadius, _borderPaint!);

        // Text
        float textX = tooltipRect.Left + Padding;
        float textY = tooltipRect.Top + Padding + DefaultFontSize;
        canvas.DrawText(line1, textX, textY, _textPaint);
        canvas.DrawText(line2, textX, textY + LineHeight, _textPaint);
        canvas.DrawText(line3, textX, textY + LineHeight * 2, _textPaint);

        _logger.LogDebug("Rendered tooltip for '{Series}' at ({X}, {Y})",
            seriesName, position.X, position.Y);
    }

    /// <summary>
    /// Adjusts the tooltip rectangle so it does not overflow the visible canvas area.
    /// </summary>
    /// <param name="anchor">Desired anchor point.</param>
    /// <param name="width">Tooltip width.</param>
    /// <param name="height">Tooltip height.</param>
    /// <param name="canvas">The canvas to check bounds against.</param>
    /// <returns>A clamped SKRect.</returns>
    public SKRect ClampToCanvas(SKPoint anchor, float width, float height, SKCanvas canvas)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        var deviceClip = canvas.DeviceClipBounds;

        float x = anchor.X;
        float y = anchor.Y - height - 4f;

        if (x + width > deviceClip.Right)
            x = deviceClip.Right - width;
        if (x < deviceClip.Left)
            x = deviceClip.Left;
        if (y < deviceClip.Top)
            y = anchor.Y + 4f;
        if (y + height > deviceClip.Bottom)
            y = deviceClip.Bottom - height;

        return SKRect.Create(x, y, width, height);
    }

    /// <summary>
    /// Checks whether a screen coordinate falls within a data point's hit area.
    /// </summary>
    /// <param name="screenPoint">The screen coordinate to test.</param>
    /// <param name="dataPointCenter">The center of the data point marker.</param>
    /// <param name="hitRadius">The hit test radius in pixels.</param>
    /// <returns>True if the point is within the hit area.</returns>
    public bool HitTest(SKPoint screenPoint, SKPoint dataPointCenter, float hitRadius = 8f)
    {
        float dx = screenPoint.X - dataPointCenter.X;
        float dy = screenPoint.Y - dataPointCenter.Y;
        return dx * dx + dy * dy <= hitRadius * hitRadius;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _backgroundPaint?.Dispose();
            _borderPaint?.Dispose();
            _textPaint?.Dispose();
            _shadowPaint?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
