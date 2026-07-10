// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Extension methods for <see cref="ChartBuilder"/> providing fluent configuration helpers
/// </summary>
public static class ChartBuilderExtensions
{
    /// <summary>
    /// Applies a predefined theme to the chart configuration
    /// </summary>
    /// <param name="builder">The chart builder instance</param>
    /// <param name="theme">The theme to apply</param>
    /// <returns>The configured chart builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChartBuilder WithTheme(this ChartBuilder builder, ChartTheme theme)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(theme);

        builder.WithBackgroundColor(theme.BackgroundColor.ToString());
        builder.WithGridColor(theme.GridColor.ToString());
        builder.WithAxisLabels(null, null);
        builder.ShowGrid(true);
        builder.ShowLegend(true);
        builder.ShowAxisLabels(true);
        builder.EnableAnimation(true);
        builder.WithAnimationDuration(1000);
        builder.EnableAntiAliasing(true);
        builder.WithMargins(20, 20, 20, 40);

        return builder;
    }

    /// <summary>
    /// Configures default properties for all series added to the chart
    /// </summary>
    /// <param name="builder">The chart builder instance</param>
    /// <param name="seriesType">The default series type</param>
    /// <param name="lineWidth">The default line width</param>
    /// <param name="color">The default series color</param>
    /// <returns>The configured chart builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChartBuilder WithSeriesDefaults(
        this ChartBuilder builder,
        ChartType seriesType = ChartType.LineChart,
        float lineWidth = 2.0f,
        string? color = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (color != null && !ColorHelper.IsValidHexColor(color))
            throw new ArgumentException("Invalid hex color format", nameof(color));

        // Store defaults in custom settings for retrieval when adding series
        builder.WithCustomSetting("SeriesDefaults_Type", seriesType);
        builder.WithCustomSetting("SeriesDefaults_LineWidth", lineWidth);
        if (color != null)
            builder.WithCustomSetting("SeriesDefaults_Color", color);

        return builder;
    }

    /// <summary>
    /// Adds multiple data points to the last series in a single call
    /// </summary>
    /// <param name="builder">The chart builder instance</param>
    /// <param name="dataPoints">Collection of data points to add</param>
    /// <returns>The configured chart builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or dataPoints is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when no series exists</exception>
    public static ChartBuilder AddDataPoints(
        this ChartBuilder builder,
        IEnumerable<DataPoint> dataPoints
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(dataPoints);

        if (builder.Build().Series.Count == 0)
            throw new InvalidOperationException("No series available. Add a series first.");

        foreach (var point in dataPoints)
        {
            builder.AddDataPointToLastSeries(point.X, point.Y, point.Label);
        }

        return builder;
    }

    /// <summary>
    /// Sets a custom configuration property on the chart
    /// </summary>
    /// <param name="builder">The chart builder instance</param>
    /// <param name="key">The property key</param>
    /// <param name="value">The property value</param>
    /// <returns>The configured chart builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or key is null</exception>
    public static ChartBuilder WithCustomSetting(
        this ChartBuilder builder,
        string key,
        object value
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(key);

        var config = builder.Build().Configuration;
        config.CustomSettings ??= new Dictionary<string, object>(StringComparer.Ordinal);
        config.CustomSettings[key] = value;

        return builder;
    }

    /// <summary>
    /// Sets the chart export format
    /// </summary>
    /// <param name="builder">The chart builder instance</param>
    /// <param name="format">The export format</param>
    /// <returns>The configured chart builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChartBuilder WithExportFormat(this ChartBuilder builder, ExportFormat format)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithCustomSetting("ExportFormat", format);
        return builder;
    }

    /// <summary>
    /// Sets the chart export DPI
    /// </summary>
    /// <param name="builder">The chart builder instance</param>
    /// <param name="dpi">The DPI value</param>
    /// <returns>The configured chart builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null</exception>
    public static ChartBuilder WithExportDPI(this ChartBuilder builder, int dpi)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (dpi < 72 || dpi > 600)
            throw new ArgumentOutOfRangeException(nameof(dpi), "DPI must be between 72 and 600");

        builder.WithCustomSetting("ExportDPI", dpi);
        return builder;
    }

    /// <summary>
    /// Adds a series with data points created from value tuples
    /// </summary>
    /// <param name="builder">The chart builder instance</param>
    /// <param name="seriesName">The name of the series</param>
    /// <param name="data">Collection of (x, y) value tuples</param>
    /// <param name="color">Optional series color</param>
    /// <returns>The configured chart builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder, seriesName, or data is null</exception>
    public static ChartBuilder AddSeries(
        this ChartBuilder builder,
        string seriesName,
        IEnumerable<(double x, double y)> data,
        string? color = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(seriesName);
        ArgumentNullException.ThrowIfNull(data);

        builder.AddSeries(seriesName, color);

        foreach (var (x, y) in data)
        {
            builder.AddDataPointToLastSeries(x, y);
        }

        return builder;
    }

    /// <summary>
    /// Adds a series with data points created from value tuples and labels
    /// </summary>
    /// <param name="builder">The chart builder instance</param>
    /// <param name="seriesName">The name of the series</param>
    /// <param name="data">Collection of (x, y, label) value tuples</param>
    /// <param name="color">Optional series color</param>
    /// <returns>The configured chart builder for fluent chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder, seriesName, or data is null</exception>
    public static ChartBuilder AddSeries(
        this ChartBuilder builder,
        string seriesName,
        IEnumerable<(double x, double y, string label)> data,
        string? color = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(seriesName);
        ArgumentNullException.ThrowIfNull(data);

        builder.AddSeries(seriesName, color);

        foreach (var (x, y, label) in data)
        {
            builder.AddDataPointToLastSeries(x, y, label);
        }

        return builder;
    }
}
