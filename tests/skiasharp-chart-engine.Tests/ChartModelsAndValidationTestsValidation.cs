// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

namespace SkiaSharpChartEngine.Tests.Models;

/// <summary>
/// Validation helpers for chart models tested in <see cref="ChartModelsAndValidationTests"/> class.
/// Provides methods to validate chart models and throw descriptive exceptions for invalid data.
/// </summary>
public static class ChartModelsAndValidationTestsValidation
{
    /// <summary>
    /// Validates a <see cref="DataPoint"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The data point to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this DataPoint value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (double.IsNaN(value.X))
        {
            problems.Add("DataPoint.X cannot be NaN");
        }

        if (double.IsInfinity(value.X))
        {
            problems.Add("DataPoint.X cannot be Infinity");
        }

        if (double.IsNaN(value.Y))
        {
            problems.Add("DataPoint.Y cannot be NaN");
        }

        if (double.IsInfinity(value.Y))
        {
            problems.Add("DataPoint.Y cannot be Infinity");
        }

        if (string.IsNullOrWhiteSpace(value.Color) || !ColorHelper.IsValidHexColor(value.Color))
        {
            problems.Add("DataPoint.Color must be a valid hex color (#RRGGBB or #RRGGBBAA)");
        }

        if (value.CustomRadius.HasValue && value.CustomRadius.Value <= 0)
        {
            problems.Add("DataPoint.CustomRadius must be positive if specified");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DataPoint"/> instance is valid.
    /// </summary>
    /// <param name="value">The data point to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this DataPoint value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Validates the specified <see cref="DataPoint"/> instance and throws an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if the instance is invalid.
    /// </summary>
    /// <param name="value">The data point to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static void EnsureValid(this DataPoint value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException($"DataPoint validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
    }

    /// <summary>
    /// Validates a <see cref="ChartSeries"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The chart series to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this ChartSeries value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("ChartSeries.Name cannot be empty or whitespace");
        }

        if (value.LineWidth <= 0)
        {
            problems.Add("ChartSeries.LineWidth must be greater than 0");
        }

        if (!ColorHelper.IsValidHexColor(value.Color))
        {
            problems.Add("ChartSeries.Color must be a valid hex color (#RRGGBB or #RRGGBBAA)");
        }

        if (value.YAxisMin.HasValue && value.YAxisMax.HasValue && value.YAxisMin.Value > value.YAxisMax.Value)
        {
            problems.Add("ChartSeries.YAxisMin cannot be greater than YAxisMax");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ChartSeries"/> instance is valid.
    /// </summary>
    /// <param name="value">The chart series to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this ChartSeries value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Validates the specified <see cref="ChartSeries"/> instance and throws an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if the instance is invalid.
    /// </summary>
    /// <param name="value">The chart series to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static void EnsureValid(this ChartSeries value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException($"ChartSeries validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
    }

    /// <summary>
    /// Validates a <see cref="Chart"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The chart to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this Chart value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Id))
        {
            problems.Add("Chart.Id cannot be empty");
        }

        if (value.Series.Count == 0)
        {
            problems.Add("Chart must contain at least one series");
        }

        if (value.CreatedAt == default)
        {
            problems.Add("Chart.CreatedAt cannot be default DateTime");
        }

        var totalDataPoints = 0;
        for (var i = 0; i < value.Series.Count; i++)
        {
            var seriesProblems = value.Series[i].Validate();
            if (seriesProblems.Count > 0)
            {
                problems.AddRange(seriesProblems);
            }
            totalDataPoints += value.Series[i].GetDataPointCount();
        }

        if (totalDataPoints == 0)
        {
            problems.Add("Chart must contain at least one data point across all series");
        }

    if (value.Configuration != null)
    {
        try
        {
            value.Configuration.Validate();
        }
        catch (InvalidOperationException ex)
        {
            problems.Add(ex.Message);
        }
    }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="Chart"/> instance is valid.
    /// </summary>
    /// <param name="value">The chart to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this Chart value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Validates the specified <see cref="Chart"/> instance and throws an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if the instance is invalid.
    /// </summary>
    /// <param name="value">The chart to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static void EnsureValid(this Chart value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException($"Chart validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
    }

    /// <summary>
    /// Validates a <see cref="ChartConfiguration"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The chart configuration to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static IReadOnlyList<string> Validate(this ChartConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.Width < ChartConstants.MinimumChartWidth || value.Width > ChartConstants.MaximumChartWidth)
        {
            problems.Add($"ChartConfiguration.Width must be between {ChartConstants.MinimumChartWidth} and {ChartConstants.MaximumChartWidth}");
        }

        if (value.Height < ChartConstants.MinimumChartHeight || value.Height > ChartConstants.MaximumChartHeight)
        {
            problems.Add($"ChartConfiguration.Height must be between {ChartConstants.MinimumChartHeight} and {ChartConstants.MaximumChartHeight}");
        }

        if (string.IsNullOrWhiteSpace(value.Title) || value.Title.Trim().Length == 0)
        {
            problems.Add("ChartConfiguration.Title cannot be empty");
        }

        if (!ColorHelper.IsValidHexColor(value.BackgroundColor))
        {
            problems.Add("ChartConfiguration.BackgroundColor must be a valid hex color");
        }

        if (!ColorHelper.IsValidHexColor(value.GridColor))
        {
            problems.Add("ChartConfiguration.GridColor must be a valid hex color");
        }

        if (!ColorHelper.IsValidHexColor(value.AxisColor))
        {
            problems.Add("ChartConfiguration.AxisColor must be a valid hex color");
        }

        if (!ColorHelper.IsValidHexColor(value.TextColor))
        {
            problems.Add("ChartConfiguration.TextColor must be a valid hex color");
        }

        if (value.MarginTop < 0 || value.MarginBottom < 0 || value.MarginLeft < 0 || value.MarginRight < 0)
        {
            problems.Add("ChartConfiguration margins cannot be negative");
        }

        if (value.AnimationDurationMs <= 0)
        {
            problems.Add("ChartConfiguration.AnimationDurationMs must be greater than 0");
        }

        if (value.ExportDPI <= 0)
        {
            problems.Add("ChartConfiguration.ExportDPI must be greater than 0");
        }

        if (value.ExportQuality <= 0 || value.ExportQuality > 1)
        {
            problems.Add("ChartConfiguration.ExportQuality must be between 0 and 1");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ChartConfiguration"/> instance is valid.
    /// </summary>
    /// <param name="value">The chart configuration to check</param>
    /// <returns>True if valid; false otherwise</returns>
public static bool IsValid(this ChartConfiguration value) => Validate(value).Count == 0;

    /// <summary>
    /// Validates the specified <see cref="ChartConfiguration"/> instance and throws an <see cref="ArgumentException"/>
    /// with a detailed message listing all validation problems if the instance is invalid.
    /// </summary>
    /// <param name="value">The chart configuration to validate</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null</exception>
    public static void EnsureValid(this ChartConfiguration value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException($"ChartConfiguration validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
    }
}
