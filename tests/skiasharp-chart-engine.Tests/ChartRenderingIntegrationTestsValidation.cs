using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Tests.Integration;

/// <summary>
/// Provides validation helpers for ChartRenderingIntegrationTests to ensure test configurations
/// and data are valid before execution. Validates null/empty strings, out-of-range numbers,
/// and other common issues that could cause test failures.
/// </summary>
public static class ChartRenderingIntegrationTestsValidation
{
    /// <summary>
    /// Validates chart configuration parameters to ensure they are within reasonable bounds.
    /// </summary>
    /// <param name="configuration">The chart configuration to validate.</param>
    /// <returns>An IReadOnlyList of validation problem descriptions. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if configuration is null.</exception>
    public static IReadOnlyList<string> ValidateChartConfiguration(this ChartConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var problems = new List<string>();

        // Validate dimensions
        if (configuration.Width <= 0)
        {
            problems.Add("Chart width must be greater than 0.");
        }

        if (configuration.Height <= 0)
        {
            problems.Add("Chart height must be greater than 0.");
        }

        // Validate reasonable maximum dimensions
        if (configuration.Width > 10000)
        {
            problems.Add("Chart width exceeds maximum reasonable value (10000).");
        }

        if (configuration.Height > 10000)
        {
            problems.Add("Chart height exceeds maximum reasonable value (10000).");
        }

        // Validate title and axis labels
        if (!string.IsNullOrWhiteSpace(configuration.Title) && configuration.Title.Length > 500)
        {
            problems.Add("Chart title exceeds maximum length of 500 characters.");
        }

        if (!string.IsNullOrWhiteSpace(configuration.XAxisLabel) && configuration.XAxisLabel.Length > 200)
        {
            problems.Add("X-axis label exceeds maximum length of 200 characters.");
        }

        if (!string.IsNullOrWhiteSpace(configuration.YAxisLabel) && configuration.YAxisLabel.Length > 200)
        {
            problems.Add("Y-axis label exceeds maximum length of 200 characters.");
        }

        // Validate color formats
        if (!string.IsNullOrWhiteSpace(configuration.BackgroundColor) && !IsValidColor(configuration.BackgroundColor))
        {
            problems.Add("Background color is not a valid hex color format (e.g., #RRGGBB or #RGB).");
        }

        if (!string.IsNullOrWhiteSpace(configuration.GridColor) && !IsValidColor(configuration.GridColor))
        {
            problems.Add("Grid color is not a valid hex color format (e.g., #RRGGBB or #RGB).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a chart series to ensure it has valid data points and configuration.
    /// </summary>
    /// <param name="series">The chart series to validate.</param>
    /// <returns>An IReadOnlyList of validation problem descriptions. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if series is null.</exception>
    public static IReadOnlyList<string> ValidateChartSeries(this ChartSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);

        var problems = new List<string>();

        // Validate series name
        if (string.IsNullOrWhiteSpace(series.Name))
        {
            problems.Add("Chart series name is null or whitespace.");
        }
        else if (series.Name.Length > 200)
        {
            problems.Add("Chart series name exceeds maximum length of 200 characters.");
        }

        // Validate color format
        if (!string.IsNullOrWhiteSpace(series.Color) && !IsValidColor(series.Color))
        {
            problems.Add("Series color is not a valid hex color format (e.g., #RRGGBB or #RGB).");
        }

        // Validate data points
        if (series.DataPoints is null)
        {
            problems.Add("Chart series data points collection is null.");
        }
        else if (series.DataPoints.Count == 0)
        {
            problems.Add("Chart series has no data points.");
        }
        else
        {
            // Validate each data point
            for (int i = 0; i < series.DataPoints.Count; i++)
            {
                var point = series.DataPoints[i];

                if (point is null)
                {
                    problems.Add($"Data point at index {i} is null.");
                }
                else
                {
                    // Validate X value (can be any number, but check for NaN/Infinity)
                    if (double.IsNaN(point.X) || double.IsInfinity(point.X))
                    {
                        problems.Add($"Data point at index {i} has invalid X value (NaN or Infinity).");
                    }

                    // Validate Y value (can be any number, but check for NaN/Infinity)
                    if (double.IsNaN(point.Y) || double.IsInfinity(point.Y))
                    {
                        problems.Add($"Data point at index {i} has invalid Y value (NaN or Infinity).");
                    }
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a chart to ensure it has valid configuration and data.
    /// </summary>
    /// <param name="chart">The chart to validate.</param>
    /// <returns>An IReadOnlyList of validation problem descriptions. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if chart is null.</exception>
    public static IReadOnlyList<string> ValidateChart(this Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        var problems = new List<string>();

        // Validate chart ID
        if (string.IsNullOrWhiteSpace(chart.Id))
        {
            problems.Add("Chart ID is null or whitespace.");
        }
        else if (chart.Id.Length > 500)
        {
            problems.Add("Chart ID exceeds maximum length of 500 characters.");
        }

        // Validate configuration
        if (chart.Configuration is null)
        {
            problems.Add("Chart configuration is null.");
        }
        else
        {
            IReadOnlyList<string> configProblems = chart.Configuration.ValidateChartConfiguration();
            problems.AddRange(configProblems);
        }

        // Validate series collection
        if (chart.Series is null)
        {
            problems.Add("Chart series collection is null.");
        }
        else if (chart.Series.Count == 0)
        {
            problems.Add("Chart has no series.");
        }
        else
        {
            // Validate each series
            for (int i = 0; i < chart.Series.Count; i++)
            {
                var series = chart.Series[i];

                if (series is null)
                {
                    problems.Add($"Series at index {i} is null.");
                }
                else
                {
                    IReadOnlyList<string> seriesProblems = series.ValidateChartSeries();
                    problems.AddRange(seriesProblems);
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a string represents a valid hex color format.
    /// Supports formats: #RGB, #RRGGBB, #RRGGBBAA
    /// </summary>
    /// <param name="color">The color string to validate.</param>
    /// <returns>True if valid color format; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if color is null.</exception>
    private static bool IsValidColor(string color)
    {
        ArgumentNullException.ThrowIfNull(color);

        if (string.IsNullOrWhiteSpace(color))
        {
            return true; // null/empty is acceptable (will use default)
        }

        // Remove # if present
        var hex = color.TrimStart('#');

        // Check length: 3, 6, or 8 characters
        if (hex.Length != 3 && hex.Length != 6 && hex.Length != 8)
        {
            return false;
        }

        // Check that all characters are valid hex digits
        return hex.All(c => Uri.IsHexDigit(c));
    }

    /// <summary>
    /// Determines whether the chart configuration is valid.
    /// </summary>
    /// <param name="configuration">The chart configuration to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if configuration is null.</exception>
    public static bool IsValidChartConfiguration(this ChartConfiguration configuration)
    {
        return ValidateChartConfiguration(configuration).Count == 0;
    }

    /// <summary>
    /// Ensures the chart configuration is valid, throwing an ArgumentException
    /// with a detailed message listing all validation problems if any are found.
    /// </summary>
    /// <param name="configuration">The chart configuration to validate.</param>
    /// <exception cref="ArgumentException">Thrown if configuration is invalid, with a message listing all problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown if configuration is null.</exception>
    public static void EnsureValidChartConfiguration(this ChartConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var problems = ValidateChartConfiguration(configuration);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Chart configuration is invalid. " +
                string.Join(" ", problems),
                nameof(configuration));
        }
    }

    /// <summary>
    /// Determines whether the chart series is valid.
    /// </summary>
    /// <param name="series">The chart series to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if series is null.</exception>
    public static bool IsValidChartSeries(this ChartSeries series)
    {
        return ValidateChartSeries(series).Count == 0;
    }

    /// <summary>
    /// Ensures the chart series is valid, throwing an ArgumentException
    /// with a detailed message listing all validation problems if any are found.
    /// </summary>
    /// <param name="series">The chart series to validate.</param>
    /// <exception cref="ArgumentException">Thrown if series is invalid, with a message listing all problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown if series is null.</exception>
    public static void EnsureValidChartSeries(this ChartSeries series)
    {
        ArgumentNullException.ThrowIfNull(series);

        var problems = ValidateChartSeries(series);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Chart series is invalid. " +
                string.Join(" ", problems),
                nameof(series));
        }
    }

    /// <summary>
    /// Determines whether the chart is valid.
    /// </summary>
    /// <param name="chart">The chart to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if chart is null.</exception>
    public static bool IsValidChart(this Chart chart)
    {
        return ValidateChart(chart).Count == 0;
    }

    /// <summary>
    /// Ensures the chart is valid, throwing an ArgumentException
    /// with a detailed message listing all validation problems if any are found.
    /// </summary>
    /// <param name="chart">The chart to validate.</param>
    /// <exception cref="ArgumentException">Thrown if chart is invalid, with a message listing all problems.</exception>
    /// <exception cref="ArgumentNullException">Thrown if chart is null.</exception>
    public static void EnsureValidChart(this Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);

        var problems = ValidateChart(chart);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "Chart is invalid. " +
                string.Join(" ", problems),
                nameof(chart));
        }
    }
}