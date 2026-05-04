// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Constants;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Service for chart data validation and transformation
/// </summary>
public class ChartDataService : IChartDataService
{
    private readonly ILogger<ChartDataService> _logger;

    public ChartDataService(ILogger<ChartDataService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ValidateChart(Chart chart)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (chart.Series.Count == 0)
            throw new InvalidChartDataException("Chart must contain at least one series");

        if (chart.Series.All(s => s.GetDataPointCount() == 0))
            throw new InvalidChartDataException("Chart must contain at least one data point across all series");

        foreach (var series in chart.Series)
        {
            ValidateSeries(series);
        }

        chart.Configuration.Validate();

        _logger.LogInformation($"Chart {chart.Id} validation passed");
    }

    public void ValidateSeries(ChartSeries series)
    {
        if (series == null)
            throw new ArgumentNullException(nameof(series));

        if (string.IsNullOrWhiteSpace(series.Name))
            throw new InvalidChartDataException("Series name cannot be empty");

        if (series.GetDataPointCount() > ChartConstants.MaxDataPoints)
            throw new InvalidChartDataException($"Series contains too many data points (max: {ChartConstants.MaxDataPoints})");

        foreach (var point in series.DataPoints)
        {
            ValidateDataPoint(point);
        }

        if (series.LineWidth <= 0)
            throw new InvalidChartDataException("Line width must be greater than 0");

        _logger.LogInformation($"Series '{series.Name}' validation passed");
    }

    public void ValidateDataPoint(DataPoint point)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        if (double.IsNaN(point.X) || double.IsInfinity(point.X))
            throw new InvalidChartDataException("DataPoint X value is invalid (NaN or Infinity)");

        if (double.IsNaN(point.Y) || double.IsInfinity(point.Y))
            throw new InvalidChartDataException("DataPoint Y value is invalid (NaN or Infinity)");
    }

    public async Task<bool> ValidateChartAsync(Chart chart, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                ValidateChart(chart);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chart validation failed");
                return false;
            }
        }, cancellationToken);
    }

    public (double min, double max) CalculateAxisRange(IEnumerable<double> values, AxisScaleType scaleType)
    {
        if (values == null || !values.Any())
            return (0, 1);

        var validValues = values.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToList();

        if (!validValues.Any())
            return (0, 1);

        var min = validValues.Min();
        var max = validValues.Max();

        return scaleType switch
        {
            AxisScaleType.Linear => (min, max),
            AxisScaleType.Logarithmic => (Math.Max(1, min), Math.Max(2, max)),
            AxisScaleType.Categorical => (min, max),
            AxisScaleType.DateTimeLinear => (min, max),
            _ => (min, max)
        };
    }

    public void NormalizeDataPoints(List<DataPoint> points)
    {
        if (points == null || points.Count == 0)
            return;

        var validPoints = points.Where(p => !double.IsNaN(p.X) && !double.IsNaN(p.Y)).ToList();

        if (validPoints.Count == 0)
            return;

        var xValues = validPoints.Select(p => p.X);
        var yValues = validPoints.Select(p => p.Y);

        var (xMin, xMax) = CalculateAxisRange(xValues, AxisScaleType.Linear);
        var (yMin, yMax) = CalculateAxisRange(yValues, AxisScaleType.Linear);

        var xRange = xMax - xMin > 0 ? xMax - xMin : 1;
        var yRange = yMax - yMin > 0 ? yMax - yMin : 1;

        foreach (var point in points)
        {
            point.X = (point.X - xMin) / xRange;
            point.Y = (point.Y - yMin) / yRange;
        }

        _logger.LogInformation($"Normalized {points.Count} data points");
    }

    public Chart TransformChartData(Chart chart, Func<DataPoint, DataPoint> transformer)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (transformer == null)
            throw new ArgumentNullException(nameof(transformer));

        var cloned = chart.Clone();

        foreach (var series in cloned.Series)
        {
            var transformedPoints = series.DataPoints.Select(p => transformer(p.Clone())).ToList();
            series.ClearDataPoints();
            series.AddDataPoints(transformedPoints);
        }

        _logger.LogInformation($"Transformed chart {chart.Id} data");
        return cloned;
    }

    public List<DataPoint> FilterDataPoints(List<DataPoint> points, Func<DataPoint, bool> predicate)
    {
        if (points == null)
            throw new ArgumentNullException(nameof(points));

        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var filtered = points.Where(predicate).ToList();
        _logger.LogInformation($"Filtered {points.Count} points down to {filtered.Count}");

        return filtered;
    }
}
