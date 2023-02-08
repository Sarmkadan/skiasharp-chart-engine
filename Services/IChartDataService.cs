// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Exceptions;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Interface for chart data operations and validation
/// </summary>
public interface IChartDataService
{
    void ValidateChart(Chart chart);

    void ValidateSeries(ChartSeries series);

    void ValidateDataPoint(DataPoint point);

    Task<bool> ValidateChartAsync(Chart chart, CancellationToken cancellationToken = default);

    (double min, double max) CalculateAxisRange(IEnumerable<double> values, AxisScaleType scaleType);

    void NormalizeDataPoints(List<DataPoint> points);

    Chart TransformChartData(Chart chart, Func<DataPoint, DataPoint> transformer);

    List<DataPoint> FilterDataPoints(List<DataPoint> points, Func<DataPoint, bool> predicate);
}
