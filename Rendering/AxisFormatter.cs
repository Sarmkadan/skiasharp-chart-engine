// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Formats and renders axis labels, tick marks, and grid lines for chart axes.
/// Supports numeric, date/time, and percentage formatting modes.
/// </summary>
public class AxisFormatter
{
    private const float DefaultFontSize = 10f;
    private const float TickLength = 5f;
    private const float LabelOffset = 8f;

    private readonly ILogger<AxisFormatter> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="AxisFormatter"/>.
    /// </summary>
    /// <param name="logger">Logger for axis formatting diagnostics.</param>
    public AxisFormatter(ILogger<AxisFormatter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Computes evenly spaced tick positions between min and max values.
    /// </summary>
    /// <param name="min">Axis minimum value.</param>
    /// <param name="max">Axis maximum value.</param>
    /// <param name="desiredTicks">Approximate number of tick marks.</param>
    /// <returns>List of tick values snapped to nice intervals.</returns>
    public List<double> ComputeTickPositions(double min, double max, int desiredTicks = 5)
    {
        if (desiredTicks <= 0)
            throw new ArgumentOutOfRangeException(nameof(desiredTicks));

        double range = max - min;
        if (range <= 0)
        {
            _logger.LogWarning("Axis range is zero or negative: min={Min}, max={Max}", min, max);
            return new List<double> { min };
        }

        double roughStep = range / desiredTicks;
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(roughStep)));
        double residual = roughStep / magnitude;

        double niceStep = residual switch
        {
            <= 1.5 => magnitude,
            <= 3.0 => 2 * magnitude,
            <= 7.0 => 5 * magnitude,
            _ => 10 * magnitude
        };

        double start = Math.Floor(min / niceStep) * niceStep;
        var ticks = new List<double>();
        for (double v = start; v <= max + niceStep * 0.01; v += niceStep)
        {
            ticks.Add(Math.Round(v, 10));
        }

        return ticks;
    }

    /// <summary>
    /// Formats a numeric value as a label string using the specified format.
    /// </summary>
    /// <param name="value">The tick value.</param>
    /// <param name="format">A .NET numeric format string (e.g. "N0", "P1", "C2").</param>
    /// <returns>The formatted label.</returns>
    public string FormatLabel(double value, string format = "G")
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Draws Y-axis labels and horizontal grid lines on the canvas.
    /// </summary>
    /// <param name="canvas">The canvas to draw on.</param>
    /// <param name="bounds">The chart drawing area.</param>
    /// <param name="ticks">Pre-computed tick values.</param>
    /// <param name="minValue">Data minimum.</param>
    /// <param name="maxValue">Data maximum.</param>
    /// <param name="format">Label format string.</param>
    public void DrawYAxis(SKCanvas canvas, SKRect bounds, List<double> ticks,
        double minValue, double maxValue, string format = "N0")
    {
        ArgumentNullException.ThrowIfNull(canvas);

        using var paint = new SKPaint
        {
            Color = new SKColor(0x66, 0x66, 0x66),
            TextSize = DefaultFontSize,
            IsAntialias = true
        };

        using var gridPaint = new SKPaint
        {
            Color = new SKColor(0xE0, 0xE0, 0xE0),
            StrokeWidth = 0.5f,
            IsAntialias = true
        };

        double range = maxValue - minValue;
        if (range <= 0) return;

        foreach (double tick in ticks)
        {
            float ratio = (float)((tick - minValue) / range);
            float y = bounds.Bottom - ratio * bounds.Height;

            if (y < bounds.Top || y > bounds.Bottom)
                continue;

            string label = FormatLabel(tick, format);
            canvas.DrawText(label, bounds.Left - LabelOffset - paint.MeasureText(label), y + DefaultFontSize / 3, paint);
            canvas.DrawLine(bounds.Left - TickLength, y, bounds.Left, y, paint);
            canvas.DrawLine(bounds.Left, y, bounds.Right, y, gridPaint);
        }

        _logger.LogDebug("Drew {Count} Y-axis labels", ticks.Count);
    }
}
