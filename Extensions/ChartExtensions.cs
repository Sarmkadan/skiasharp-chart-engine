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
    public static Chart WithDefaultConfiguration(this Chart chart, ChartType? overrideType = null)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var type = overrideType ?? chart.Type;
        var validator = ChartValidator.ValidateChart(chart);

        if (!validator.IsValid)
            throw new InvalidOperationException($"Chart validation failed:\n{validator}");

        return chart;
    }

    public static (double minX, double maxX, double minY, double maxY) GetAxisBounds(this Chart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        return chart.GetDataBounds();
    }

    public static int GetTotalDataPoints(this Chart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        return chart.GetTotalDataPoints();
    }

    public static bool IsEmpty(this Chart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        return chart.Series.Count == 0 || chart.GetTotalDataPoints() == 0;
    }

    public static Chart ApplyPalette(this Chart chart, ColorPalette palette)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (palette == null)
            throw new ArgumentNullException(nameof(palette));

        var cloned = chart.Clone();
        for (int i = 0; i < cloned.Series.Count; i++)
        {
            cloned.Series[i].Color = palette.GetColorAtIndex(i);
        }

        return cloned;
    }

    public static Chart NormalizeData(this Chart chart, bool clone = true)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var targetChart = clone ? chart.Clone() : chart;

        foreach (var series in targetChart.Series)
        {
            var points = series.DataPoints.ToList();
            ChartValidator.ValidateChart(targetChart);
        }

        return targetChart;
    }

    public static Chart FilterSeries(this Chart chart, Func<ChartSeries, bool> predicate, bool clone = true)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var targetChart = clone ? chart.Clone() : chart;
        var seriesToRemove = targetChart.Series.Where(s => !predicate(s)).ToList();

        foreach (var series in seriesToRemove)
        {
            targetChart.Series.Remove(series);
        }

        return targetChart;
    }

    public static ChartBuilder ToBuilder(this Chart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        var builder = new ChartBuilder(chart.Type, chart.Id);
        return builder;
    }
}
