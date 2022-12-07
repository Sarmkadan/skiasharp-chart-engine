// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Fluent builder for creating charts with configuration
/// </summary>
public class ChartBuilder
{
    private readonly Chart _chart;
    private readonly ChartConfiguration _config;

    public ChartBuilder(ChartType chartType = ChartType.LineChart, string? chartId = null)
    {
        _config = new ChartConfiguration();
        _chart = new Chart(chartType, chartId);
    }

    public ChartBuilder WithTitle(string title)
    {
        _config.Title = title;
        return this;
    }

    public ChartBuilder WithSubtitle(string? subtitle)
    {
        _config.Subtitle = subtitle;
        return this;
    }

    public ChartBuilder WithSize(int width, int height)
    {
        _config.Width = width;
        _config.Height = height;
        return this;
    }

    public ChartBuilder WithMargins(int top, int right, int bottom, int left)
    {
        _config.MarginTop = top;
        _config.MarginRight = right;
        _config.MarginBottom = bottom;
        _config.MarginLeft = left;
        return this;
    }

    public ChartBuilder WithAxisLabels(string? xLabel, string? yLabel)
    {
        _config.XAxisLabel = xLabel;
        _config.YAxisLabel = yLabel;
        return this;
    }

    public ChartBuilder WithAxisScales(AxisScaleType xScale, AxisScaleType yScale)
    {
        _config.XAxisScaleType = xScale;
        _config.YAxisScaleType = yScale;
        return this;
    }

    public ChartBuilder WithAxisRange(double? xMin, double? xMax, double? yMin, double? yMax)
    {
        _config.XAxisMin = xMin;
        _config.XAxisMax = xMax;
        _config.YAxisMin = yMin;
        _config.YAxisMax = yMax;
        return this;
    }

    public ChartBuilder WithBackgroundColor(string hexColor)
    {
        if (!ColorHelper.IsValidHexColor(hexColor))
            throw new ArgumentException("Invalid hex color format");

        _config.BackgroundColor = hexColor;
        return this;
    }

    public ChartBuilder WithGridColor(string hexColor)
    {
        if (!ColorHelper.IsValidHexColor(hexColor))
            throw new ArgumentException("Invalid hex color format");

        _config.GridColor = hexColor;
        return this;
    }

    public ChartBuilder ShowGrid(bool show)
    {
        _config.ShowGrid = show;
        return this;
    }

    public ChartBuilder ShowLegend(bool show)
    {
        _config.ShowLegend = show;
        return this;
    }

    public ChartBuilder ShowAxisLabels(bool show)
    {
        _config.ShowAxisLabels = show;
        return this;
    }

    public ChartBuilder ShowDataPointLabels(bool show)
    {
        _config.ShowDataPointLabels = show;
        return this;
    }

    public ChartBuilder EnableAnimation(bool enable)
    {
        _config.EnableAnimation = enable;
        return this;
    }

    public ChartBuilder WithAnimationDuration(int durationMs)
    {
        _config.AnimationDurationMs = durationMs;
        return this;
    }

    public ChartBuilder EnableAntiAliasing(bool enable)
    {
        _config.AntiAlias = enable;
        return this;
    }

    public ChartBuilder AddSeries(string seriesName, string? color = null)
    {
        var series = new ChartSeries(seriesName);
        if (!string.IsNullOrEmpty(color))
        {
            if (!ColorHelper.IsValidHexColor(color))
                throw new ArgumentException("Invalid hex color format");
            series.Color = color;
        }
        _chart.AddSeries(series);
        return this;
    }

    public ChartBuilder AddSeriesWithData(string seriesName, List<DataPoint> dataPoints, string? color = null)
    {
        var series = new ChartSeries(seriesName);
        if (!string.IsNullOrEmpty(color))
        {
            if (!ColorHelper.IsValidHexColor(color))
                throw new ArgumentException("Invalid hex color format");
            series.Color = color;
        }
        series.AddDataPoints(dataPoints);
        _chart.AddSeries(series);
        return this;
    }

    public ChartBuilder AddDataPointToLastSeries(double x, double y, string? label = null)
    {
        if (_chart.Series.Count == 0)
            throw new InvalidOperationException("No series available. Add a series first.");

        var lastSeries = _chart.Series[^1];
        lastSeries.AddDataPoint(x, y, label);
        return this;
    }

    public Chart Build()
    {
        var config = _config.Clone();
        foreach (var prop in typeof(ChartConfiguration).GetProperties())
        {
            var value = prop.GetValue(config);
            typeof(ChartConfiguration).GetProperty(prop.Name)?.SetValue(_chart.Configuration, value);
        }

        return _chart;
    }
}
