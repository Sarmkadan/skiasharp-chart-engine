// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Rendering;

/// <summary>
/// Computes legend item positions and sizes for different layout modes
/// (horizontal, vertical, wrapped) within the chart bounds.
/// </summary>
public class LegendLayout
{
    private const float SwatchSize = 12f;
    private const float ItemPadding = 8f;
    private const float SwatchTextGap = 4f;
    private const float DefaultFontSize = 11f;

    private readonly ILogger<LegendLayout> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="LegendLayout"/>.
    /// </summary>
    /// <param name="logger">Logger for layout diagnostics.</param>
    public LegendLayout(ILogger<LegendLayout> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Measures the total bounding box needed for all legend items in horizontal layout.
    /// </summary>
    /// <param name="seriesNames">Names of the series to display.</param>
    /// <returns>The required width and height.</returns>
    public SKSize MeasureHorizontal(IReadOnlyList<string> seriesNames)
    {
        ArgumentNullException.ThrowIfNull(seriesNames);

        using var paint = new SKPaint { TextSize = DefaultFontSize, IsAntialias = true };
        float totalWidth = 0;

        foreach (string name in seriesNames)
        {
            totalWidth += SwatchSize + SwatchTextGap + paint.MeasureText(name) + ItemPadding;
        }

        return new SKSize(totalWidth, SwatchSize + ItemPadding);
    }

    /// <summary>
    /// Computes item rectangles for a vertical (stacked) legend layout.
    /// </summary>
    /// <param name="seriesNames">Series names.</param>
    /// <param name="origin">Top-left corner of the legend area.</param>
    /// <returns>List of (label, swatch rect, text position) tuples.</returns>
    public List<(string Label, SKRect SwatchRect, SKPoint TextOrigin)> ComputeVerticalLayout(
        IReadOnlyList<string> seriesNames, SKPoint origin)
    {
        ArgumentNullException.ThrowIfNull(seriesNames);

        var items = new List<(string, SKRect, SKPoint)>();
        float y = origin.Y;

        foreach (string name in seriesNames)
        {
            var swatchRect = new SKRect(origin.X, y, origin.X + SwatchSize, y + SwatchSize);
            var textOrigin = new SKPoint(origin.X + SwatchSize + SwatchTextGap, y + SwatchSize);
            items.Add((name, swatchRect, textOrigin));
            y += SwatchSize + ItemPadding;
        }

        _logger.LogDebug("Computed vertical legend layout for {Count} items", seriesNames.Count);
        return items;
    }

    /// <summary>
    /// Computes a wrapped horizontal layout that fits within a maximum width.
    /// Items flow left to right and wrap to the next row when exceeding maxWidth.
    /// </summary>
    /// <param name="seriesNames">Series names.</param>
    /// <param name="origin">Top-left corner.</param>
    /// <param name="maxWidth">Maximum width before wrapping.</param>
    /// <returns>List of positioned items with row information.</returns>
    public List<(string Label, SKRect SwatchRect, SKPoint TextOrigin, int Row)> ComputeWrappedLayout(
        IReadOnlyList<string> seriesNames, SKPoint origin, float maxWidth)
    {
        ArgumentNullException.ThrowIfNull(seriesNames);
        if (maxWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxWidth));

        using var paint = new SKPaint { TextSize = DefaultFontSize, IsAntialias = true };
        var items = new List<(string, SKRect, SKPoint, int)>();
        float x = origin.X;
        float y = origin.Y;
        int row = 0;

        foreach (string name in seriesNames)
        {
            float itemWidth = SwatchSize + SwatchTextGap + paint.MeasureText(name) + ItemPadding;

            if (x + itemWidth > origin.X + maxWidth && x > origin.X)
            {
                x = origin.X;
                y += SwatchSize + ItemPadding;
                row++;
            }

            var swatchRect = new SKRect(x, y, x + SwatchSize, y + SwatchSize);
            var textOrigin = new SKPoint(x + SwatchSize + SwatchTextGap, y + SwatchSize);
            items.Add((name, swatchRect, textOrigin, row));
            x += itemWidth;
        }

        _logger.LogDebug("Wrapped legend: {Count} items across {Rows} rows",
            seriesNames.Count, row + 1);
        return items;
    }

    /// <summary>
    /// Calculates how much vertical space the chart bounds should shrink
    /// to accommodate the legend at the bottom.
    /// </summary>
    /// <param name="seriesNames">Series names.</param>
    /// <param name="availableWidth">Width available for wrapping.</param>
    /// <returns>The height to reserve.</returns>
    public float CalculateBottomReserve(IReadOnlyList<string> seriesNames, float availableWidth)
    {
        var layout = ComputeWrappedLayout(seriesNames, SKPoint.Empty, availableWidth);
        int rows = layout.Count > 0 ? layout.Max(i => i.Row) + 1 : 0;
        return rows * (SwatchSize + ItemPadding) + ItemPadding;
    }
}
