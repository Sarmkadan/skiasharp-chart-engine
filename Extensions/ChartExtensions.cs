// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

namespace SkiaSharpChartEngine.Extensions;

/// <summary>
/// Extension methods for Chart operations
/// </summary>
public static class ChartExtensions
{
    /// <summary>
    /// Applies default configuration to the chart, optionally overriding the chart type.
    /// </summary>
    /// <param name="chart">The chart to configure. Cannot be null.</param>
    /// <param name="overrideType">Optional chart type override. If null, uses the chart's existing type.</param>
    /// <returns>The configured chart for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when chart validation fails.</exception>
    public static Chart WithDefaultConfiguration(this Chart chart, ChartType? overrideType = null)
    {
        ArgumentNullException.ThrowIfNull(chart);

        var type = overrideType ?? chart.Type;
        var validator = ChartValidator.ValidateChart(chart);

        if (!validator.IsValid)
            throw new InvalidOperationException($"Chart validation failed:\n{validator}");

        return chart;
    }

    /// <summary>
    /// Calculates the axis bounds (min/max for X and Y axes) from the chart's data.
    /// </summary>
    /// <param name="chart">The chart containing data to calculate bounds from. Cannot be null.</param>
    /// <returns>A tuple containing (minX, maxX, minY, maxY) representing the axis bounds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> is null.</exception>
    public static (double minX, double maxX, double minY, double maxY) GetAxisBounds(this Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        return chart.GetDataBounds();
    }

    /// <summary>
    /// Counts the total number of data points across all series in the chart.
    /// </summary>
    /// <param name="chart">The chart containing data points. Cannot be null.</param>
    /// <returns>The total count of data points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> is null.</exception>
    public static int GetTotalDataPoints(this Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        return chart.Series.Sum(series => series.DataPoints.Count);
    }

    /// <summary>
    /// Determines whether the chart is empty (no series or no data points).
    /// </summary>
    /// <param name="chart">The chart to check. Cannot be null.</param>
    /// <returns>True if the chart has no series or no data points; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> is null.</exception>
    public static bool IsEmpty(this Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        return chart.Series.Count == 0 || chart.GetTotalDataPoints() == 0;
    }

    /// <summary>
    /// Applies a color palette to all series in the chart.
    /// </summary>
    /// <param name="chart">The chart to apply palette to. Cannot be null.</param>
    /// <param name="palette">The color palette to apply. Cannot be null.</param>
    /// <returns>A new chart with the palette applied (original chart is not modified).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> or <paramref name="palette"/> is null.</exception>
    public static Chart ApplyPalette(this Chart chart, ColorPalette palette)
    {
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(palette);

        var cloned = chart.Clone();
        for (int i = 0; i < cloned.Series.Count; i++)
        {
            cloned.Series[i].Color = palette.GetColorAtIndex(i);
        }

        return cloned;
    }

    /// <summary>
    /// Normalizes the data in all series to a common scale (0-1 range).
    /// </summary>
    /// <param name="chart">The chart containing data to normalize. Cannot be null.</param>
    /// <param name="clone">If true, returns a new chart; otherwise modifies the original.</param>
    /// <returns>The chart with normalized data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> is null.</exception>
    public static Chart NormalizeData(this Chart chart, bool clone = true)
    {
        ArgumentNullException.ThrowIfNull(chart);

        var targetChart = clone ? chart.Clone() : chart;

        // Find min and max values across all series
        double minValue = double.MaxValue;
        double maxValue = double.MinValue;

        foreach (var series in targetChart.Series)
        {
            foreach (var point in series.DataPoints)
            {
                minValue = Math.Min(minValue, point.Y);
                maxValue = Math.Max(maxValue, point.Y);
            }
        }

        // Handle case where all values are the same (avoid division by zero)
        if (maxValue == minValue)
        {
            maxValue = minValue + 1;
        }

        // Normalize each data point to 0-1 range
        foreach (var series in targetChart.Series)
        {
            for (int i = 0; i < series.DataPoints.Count; i++)
            {
                var point = series.DataPoints[i];
                series.DataPoints[i] = new DataPoint(point.X, (point.Y - minValue) / (maxValue - minValue));
            }
        }

        ChartValidator.ValidateChart(targetChart);
        return targetChart;
    }

    /// <summary>
    /// Filters series in the chart based on a predicate.
    /// </summary>
    /// <param name="chart">The chart containing series to filter. Cannot be null.</param>
    /// <param name="predicate">The filter condition. Cannot be null.</param>
    /// <param name="clone">If true, returns a new chart; otherwise modifies the original.</param>
    /// <returns>The chart with filtered series.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> or <paramref name="predicate"/> is null.</exception>
    public static Chart FilterSeries(this Chart chart, Func<ChartSeries, bool> predicate, bool clone = true)
    {
        ArgumentNullException.ThrowIfNull(chart);
        ArgumentNullException.ThrowIfNull(predicate);

        var targetChart = clone ? chart.Clone() : chart;
        var seriesToRemove = targetChart.Series.Where(predicate).ToList();

        foreach (var series in seriesToRemove)
        {
            targetChart.Series.Remove(series);
        }

        return targetChart;
    }

    /// <summary>
    /// Creates a new ChartBuilder initialized with the chart's type and ID.
    /// </summary>
    /// <param name="chart">The chart to create a builder from. Cannot be null.</param>
    /// <returns>A new ChartBuilder instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chart"/> is null.</exception>
    public static ChartBuilder ToBuilder(this Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        return new ChartBuilder(chart.Type, chart.Id);
    }
}
