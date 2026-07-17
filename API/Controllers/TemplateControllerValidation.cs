// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Validation helpers for TemplateController to ensure API input parameters
// meet business rules before processing. Validates null/empty strings,
// out-of-range numbers, default dates, and structural integrity.
//
// This class provides extension-style validation methods for TemplateController
// and related chart configuration objects.
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.API.Controllers;

/// <summary>
/// Provides validation helpers for <see cref="TemplateController"/> instances.
/// Validates controller state including template configurations, names, IDs,
/// and date/time values to ensure API operations can proceed safely.
/// </summary>
public static class TemplateControllerValidation
{
    /// <summary>
    /// Validates the specified <see cref="TemplateController"/> instance.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this TemplateController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate controller's internal state using pattern matching
        if (value._templates is null)
        {
            problems.Add("TemplateController._templates collection is null");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="TemplateController"/> instance is valid.
    /// </summary>
    /// <param name="value">The controller instance to check.</param>
    /// <returns>True if the controller is valid; otherwise, false.</returns>
    public static bool IsValid(this TemplateController value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="TemplateController"/> instance is valid.
    /// </summary>
    /// <param name="value">The controller instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the controller has validation problems.</exception>
    public static void EnsureValid(this TemplateController value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Join("\n", problems),
                nameof(value));
        }
    }

    /// <summary>
    /// Validates a template name parameter.
    /// </summary>
    /// <param name="name">The template name to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="paramName"/> is null or empty.</exception>
    public static IReadOnlyList<string> ValidateTemplateName(string? name, string paramName = "name")
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            problems.Add($"{paramName} cannot be null or whitespace");
        }
        else if (name.Length > 200)
        {
            problems.Add($"{paramName} cannot exceed 200 characters");
        }
        else if (name.Trim().Length == 0)
        {
            problems.Add($"{paramName} cannot be empty after trimming");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a template ID parameter.
    /// </summary>
    /// <param name="templateId">The template ID to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="paramName"/> is null or empty.</exception>
    public static IReadOnlyList<string> ValidateTemplateId(string? templateId, string paramName = "templateId")
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(templateId))
        {
            problems.Add($"{paramName} cannot be null or whitespace");
        }
        else if (templateId.Length > 100)
        {
            problems.Add($"{paramName} cannot exceed 100 characters");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a chart configuration parameter.
    /// </summary>
    /// <param name="config">The chart configuration to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="paramName"/> is null or empty.</exception>
    public static IReadOnlyList<string> ValidateChartConfiguration(ChartConfiguration? config, string paramName = "config")
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName);

        var problems = new List<string>();

        if (config is null)
        {
            problems.Add($"{paramName} cannot be null");
        }
        else
        {
            // Validate chart dimensions using pattern matching
            if (config.Width < ChartConstants.MinimumChartWidth || config.Width > ChartConstants.MaximumChartWidth)
            {
                problems.Add($"{paramName}.Width must be between {ChartConstants.MinimumChartWidth} and {ChartConstants.MaximumChartWidth} pixels");
            }

            if (config.Height < ChartConstants.MinimumChartHeight || config.Height > ChartConstants.MaximumChartHeight)
            {
                problems.Add($"{paramName}.Height must be between {ChartConstants.MinimumChartHeight} and {ChartConstants.MaximumChartHeight} pixels");
            }

            // Validate margins using switch expression for consistency
            if (config.MarginTop < 0)
            {
                problems.Add($"{paramName}.MarginTop cannot be negative");
            }

            if (config.MarginBottom < 0)
            {
                problems.Add($"{paramName}.MarginBottom cannot be negative");
            }

            if (config.MarginLeft < 0)
            {
                problems.Add($"{paramName}.MarginLeft cannot be negative");
            }

            if (config.MarginRight < 0)
            {
                problems.Add($"{paramName}.MarginRight cannot be negative");
            }

            // Validate title
            if (string.IsNullOrWhiteSpace(config.Title) || config.Title.Trim().Length == 0)
            {
                problems.Add($"{paramName}.Title cannot be null or whitespace");
            }
            else if (config.Title.Length > 500)
            {
                problems.Add($"{paramName}.Title cannot exceed 500 characters");
            }

            // Validate animation duration
            if (config.AnimationDurationMs < 0)
            {
                problems.Add($"{paramName}.AnimationDurationMs cannot be negative");
            }

            // Validate export settings
            if (config.ExportDPI < 1)
            {
                problems.Add($"{paramName}.ExportDPI must be at least 1");
            }

            if (config.ExportQuality < 0f || config.ExportQuality > 1f)
            {
                problems.Add($"{paramName}.ExportQuality must be between 0.0 and 1.0");
            }

            // Validate chart configuration using its own validation
            try
            {
                config.Validate();
            }
            catch (InvalidOperationException ex)
            {
                problems.Add($"{paramName} validation failed: {ex.Message}");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a chart template parameter.
    /// </summary>
    /// <param name="template">The chart template to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="paramName"/> is null or empty.</exception>
    public static IReadOnlyList<string> ValidateChartTemplate(ChartTemplate? template, string paramName = "template")
    {
        ArgumentException.ThrowIfNullOrEmpty(paramName);

        var problems = new List<string>();

        if (template is null)
        {
            problems.Add($"{paramName} cannot be null");
        }
        else
        {
            // Validate template name
            if (string.IsNullOrWhiteSpace(template.Name) || template.Name.Trim().Length == 0)
            {
                problems.Add($"{paramName}.Name cannot be null or whitespace");
            }
            else if (template.Name.Length > 200)
            {
                problems.Add($"{paramName}.Name cannot exceed 200 characters");
            }

            // Validate template ID
            if (string.IsNullOrWhiteSpace(template.TemplateId))
            {
                problems.Add($"{paramName}.TemplateId cannot be null or whitespace");
            }
            else if (template.TemplateId.Length > 100)
            {
                problems.Add($"{paramName}.TemplateId cannot exceed 100 characters");
            }

            // Validate chart type using pattern matching
            if (template.ChartType < ChartType.LineChart || template.ChartType > ChartType.ColumnChart)
            {
                problems.Add($"{paramName}.ChartType has an invalid value");
            }

            // Validate base configuration
            var configProblems = ValidateChartConfiguration(template.BaseConfiguration, $"{paramName}.BaseConfiguration");
            problems.AddRange(configProblems);

            // Validate created date
            if (template.CreatedAt == default)
            {
                problems.Add($"{paramName}.CreatedAt cannot be the default DateTime value");
            }

            // Validate default series
            if (template.DefaultSeries is null)
            {
                problems.Add($"{paramName}.DefaultSeries cannot be null");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a chart type parameter.
    /// </summary>
    /// <param name="chartType">The chart type to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> ValidateChartType(ChartType chartType, string paramName = "chartType")
    {
        var problems = new List<string>();

        if (chartType < ChartType.LineChart || chartType > ChartType.ColumnChart)
        {
            problems.Add($"{paramName} has an invalid ChartType value");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a cancellation token parameter.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cancellationToken"/> is null.</exception>
    public static IReadOnlyList<string> ValidateCancellationToken(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(cancellationToken);

        var problems = new List<string>();

        if (cancellationToken.IsCancellationRequested)
        {
            problems.Add("Operation was cancelled");
        }

        return problems.AsReadOnly();
    }
}