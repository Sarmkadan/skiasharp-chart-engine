// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Extension methods for <see cref="ChartSeries"/> providing additional functionality
/// for data analysis and transformation.
/// </summary>
public static class ChartSeriesExtensions
{
    /// <summary>
    /// Gets the minimum Y value in the series.
    /// </summary>
    public static double MinY(this ChartSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);
        if (series.GetDataPointCount() == 0) return 0;
        return series.DataPoints.Min(p => p.Y);
    }

    /// <summary>
    /// Gets the maximum Y value in the series.
    /// </summary>
    public static double MaxY(this ChartSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);
        if (series.GetDataPointCount() == 0) return 0;
        return series.DataPoints.Max(p => p.Y);
    }

    /// <summary>
    /// Gets the range (MaxY - MinY) of the series.
    /// </summary>
    public static double Range(this ChartSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);
        if (series.GetDataPointCount() == 0) return 0;
        var (min, max) = series.GetYAxisRange();
        return max - min;
    }

    /// <summary>
    /// Checks if the series contains no data points.
    /// </summary>
    public static bool IsEmpty(this ChartSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);
        return series.GetDataPointCount() == 0;
    }

    /// <summary>
    /// Normalizes the Y values of all data points in the series to a new range.
    /// </summary>
    public static void NormalizeY(this ChartSeries series, double newMin, double newMax)
    {
        ArgumentNullException.ThrowIfNull(series);
        if (series.IsEmpty()) return;

        var (currentMin, currentMax) = series.GetYAxisRange();
        var currentRange = currentMax - currentMin;

        if (currentRange == 0) return;

        foreach (var point in series.DataPoints)
        {
            point.Y = newMin + (point.Y - currentMin) * (newMax - newMin) / currentRange;
        }
    }
}
