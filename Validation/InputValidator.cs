// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Validation;

/// <summary>
/// Validates input data for chart rendering
/// Ensures data integrity and prevents invalid operations
/// </summary>
public class InputValidator
{
    private readonly ILogger<InputValidator> _logger;
    private readonly List<ValidationRule> _rules;

    public InputValidator(ILogger<InputValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _rules = InitializeDefaultRules();
    }

    /// <summary>
    /// Validates a complete chart
    /// </summary>
    public ValidationResult ValidateChart(Chart chart)
    {
        var result = new ValidationResult();

        if (chart == null)
        {
            result.AddError("Chart cannot be null");
            return result;
        }

        ValidateChartBasics(chart, result);
        ValidateChartSeries(chart, result);
        ValidateConfiguration(chart.Configuration, result);

        return result;
    }

    /// <summary>
    /// Validates chart series data
    /// </summary>
    public ValidationResult ValidateSeriesData(List<ChartSeries> series)
    {
        var result = new ValidationResult();

        if (series == null || series.Count == 0)
        {
            result.AddError("Chart must have at least one series");
            return result;
        }

        if (series.Count > 100)
        {
            result.AddWarning("Chart has more than 100 series, which may impact performance");
        }

        foreach (var s in series)
        {
            ValidateSeries(s, result);
        }

        return result;
    }

    /// <summary>
    /// Validates chart configuration
    /// </summary>
    public ValidationResult ValidateConfiguration(ChartConfiguration? config)
    {
        var result = new ValidationResult();

        if (config == null)
        {
            result.AddWarning("No configuration provided, using defaults");
            return result;
        }

        if (config.Width < 100 || config.Width > 4000)
        {
            result.AddError("Chart width must be between 100 and 4000 pixels");
        }

        if (config.Height < 100 || config.Height > 4000)
        {
            result.AddError("Chart height must be between 100 and 4000 pixels");
        }

        return result;
    }

    /// <summary>
    /// Validates data points
    /// </summary>
    public ValidationResult ValidateDataPoints(List<DataPoint> dataPoints)
    {
        var result = new ValidationResult();

        if (dataPoints == null || dataPoints.Count == 0)
        {
            result.AddError("Series must have at least one data point");
            return result;
        }

        if (dataPoints.Count > 100000)
        {
            result.AddWarning("Series has more than 100,000 points, rendering may be slow");
        }

        var validPoints = 0;
        foreach (var point in dataPoints)
        {
            if (!float.IsNaN(point.Value) && !float.IsInfinity(point.Value))
            {
                validPoints++;
            }
        }

        if (validPoints == 0)
        {
            result.AddError("Series has no valid data points");
        }
        else if (validPoints < dataPoints.Count * 0.8)
        {
            result.AddWarning($"Series has {dataPoints.Count - validPoints} invalid/NaN values");
        }

        return result;
    }

    /// <summary>
    /// Adds a custom validation rule
    /// </summary>
    public void AddRule(ValidationRule rule)
    {
        if (rule != null)
            _rules.Add(rule);
    }

    private void ValidateChartBasics(Chart chart, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(chart.Id))
        {
            result.AddError("Chart ID cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(chart.Title))
        {
            result.AddWarning("Chart title is empty");
        }
        else if (chart.Title.Length > 200)
        {
            result.AddWarning("Chart title exceeds 200 characters");
        }
    }

    private void ValidateChartSeries(Chart chart, ValidationResult result)
    {
        if (chart.Series == null || chart.Series.Count == 0)
        {
            result.AddError("Chart must have at least one data series");
            return;
        }

        foreach (var series in chart.Series)
        {
            ValidateSeries(series, result);
        }
    }

    private void ValidateSeries(ChartSeries series, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(series.Name))
        {
            result.AddWarning("Series name is empty");
        }

        if (series.DataPoints == null || series.DataPoints.Count == 0)
        {
            result.AddError($"Series '{series.Name}' has no data points");
            return;
        }

        var dataResult = ValidateDataPoints(series.DataPoints);
        if (!dataResult.IsValid)
        {
            result.Errors.AddRange(dataResult.Errors);
            result.Warnings.AddRange(dataResult.Warnings);
        }
    }

    private void ValidateConfiguration(ChartConfiguration? config, ValidationResult result)
    {
        if (config == null)
        {
            result.AddWarning("No chart configuration provided");
            return;
        }

        var configResult = ValidateConfiguration(config);
        if (!configResult.IsValid)
        {
            result.Errors.AddRange(configResult.Errors);
            result.Warnings.AddRange(configResult.Warnings);
        }
    }

    private List<ValidationRule> InitializeDefaultRules()
    {
        return new List<ValidationRule>
        {
            new ValidationRule
            {
                Name = "MinDataPoints",
                Description = "Series must have at least 1 data point",
                Rule = (chart) => chart.Series?.All(s => s.DataPoints?.Count > 0) ?? false
            },
            new ValidationRule
            {
                Name = "MaxSeriesCount",
                Description = "Chart should not exceed 100 series",
                Rule = (chart) => (chart.Series?.Count ?? 0) <= 100
            }
        };
    }
}

/// <summary>
/// Validation result
/// </summary>
public class ValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();

    public bool IsValid => Errors.Count == 0;

    public void AddError(string message)
    {
        if (!string.IsNullOrEmpty(message))
            Errors.Add(message);
    }

    public void AddWarning(string message)
    {
        if (!string.IsNullOrEmpty(message))
            Warnings.Add(message);
    }

    public override string ToString()
    {
        var parts = new List<string>();

        if (Errors.Count > 0)
            parts.Add($"Errors: {string.Join(", ", Errors)}");

        if (Warnings.Count > 0)
            parts.Add($"Warnings: {string.Join(", ", Warnings)}");

        return string.Join(" | ", parts);
    }
}

/// <summary>
/// Custom validation rule
/// </summary>
public class ValidationRule
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Func<Chart, bool>? Rule { get; set; }
}
