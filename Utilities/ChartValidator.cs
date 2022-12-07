// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Comprehensive validation utility for charts
/// </summary>
public static class ChartValidator
{
    public static ValidationResult ValidateChart(Chart chart)
    {
        var result = new ValidationResult();

        if (chart == null)
        {
            result.AddError("Chart cannot be null");
            return result;
        }

        if (string.IsNullOrWhiteSpace(chart.Id))
            result.AddError("Chart ID cannot be empty");

        if (chart.Series.Count == 0)
            result.AddError("Chart must contain at least one series");

        var totalDataPoints = 0;
        for (int i = 0; i < chart.Series.Count; i++)
        {
            var seriesResult = ValidateSeries(chart.Series[i], i);
            result.Merge(seriesResult);
            totalDataPoints += chart.Series[i].GetDataPointCount();
        }

        if (totalDataPoints == 0)
            result.AddError("Chart must contain at least one data point");

        var configResult = ValidateConfiguration(chart.Configuration);
        result.Merge(configResult);

        return result;
    }

    public static ValidationResult ValidateSeries(ChartSeries series, int seriesIndex = 0)
    {
        var result = new ValidationResult();

        if (series == null)
        {
            result.AddError($"Series at index {seriesIndex} is null");
            return result;
        }

        if (string.IsNullOrWhiteSpace(series.Name))
            result.AddError($"Series {seriesIndex} has an empty name");

        if (series.GetDataPointCount() == 0)
            result.AddWarning($"Series '{series.Name}' contains no data points");

        if (series.LineWidth <= 0)
            result.AddError($"Series '{series.Name}' has invalid line width: {series.LineWidth}");

        if (!ColorHelper.IsValidHexColor(series.Color))
            result.AddError($"Series '{series.Name}' has invalid color: {series.Color}");

        return result;
    }

    public static ValidationResult ValidateDataPoint(DataPoint point, int pointIndex = 0)
    {
        var result = new ValidationResult();

        if (point == null)
        {
            result.AddError($"DataPoint at index {pointIndex} is null");
            return result;
        }

        if (double.IsNaN(point.X) || double.IsInfinity(point.X))
            result.AddError($"DataPoint {pointIndex} has invalid X value: {point.X}");

        if (double.IsNaN(point.Y) || double.IsInfinity(point.Y))
            result.AddError($"DataPoint {pointIndex} has invalid Y value: {point.Y}");

        if (point.CustomRadius.HasValue && point.CustomRadius.Value <= 0)
            result.AddWarning($"DataPoint {pointIndex} has non-positive radius");

        return result;
    }

    public static ValidationResult ValidateConfiguration(ChartConfiguration config)
    {
        var result = new ValidationResult();

        if (config == null)
        {
            result.AddError("Configuration cannot be null");
            return result;
        }

        if (config.Width < Constants.ChartConstants.MinimumChartWidth)
            result.AddError($"Chart width ({config.Width}) is less than minimum ({Constants.ChartConstants.MinimumChartWidth})");

        if (config.Height < Constants.ChartConstants.MinimumChartHeight)
            result.AddError($"Chart height ({config.Height}) is less than minimum ({Constants.ChartConstants.MinimumChartHeight})");

        if (string.IsNullOrWhiteSpace(config.Title))
            result.AddWarning("Chart title is empty");

        if (!ColorHelper.IsValidHexColor(config.BackgroundColor))
            result.AddError($"Invalid background color: {config.BackgroundColor}");

        if (!ColorHelper.IsValidHexColor(config.GridColor))
            result.AddError($"Invalid grid color: {config.GridColor}");

        if (!ColorHelper.IsValidHexColor(config.AxisColor))
            result.AddError($"Invalid axis color: {config.AxisColor}");

        if (config.MarginTop < 0 || config.MarginBottom < 0 || config.MarginLeft < 0 || config.MarginRight < 0)
            result.AddError("Margins cannot be negative");

        return result;
    }

    public class ValidationResult
    {
        private readonly List<string> _errors = new();
        private readonly List<string> _warnings = new();

        public IReadOnlyList<string> Errors => _errors.AsReadOnly();
        public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();

        public bool IsValid => _errors.Count == 0;

        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                _errors.Add(message);
        }

        public void AddWarning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                _warnings.Add(message);
        }

        public void Merge(ValidationResult other)
        {
            if (other == null)
                return;

            _errors.AddRange(other._errors);
            _warnings.AddRange(other._warnings);
        }

        public override string ToString()
        {
            var lines = new List<string>();

            if (_errors.Count > 0)
            {
                lines.Add($"Errors ({_errors.Count}):");
                lines.AddRange(_errors.Select(e => $"  - {e}"));
            }

            if (_warnings.Count > 0)
            {
                lines.Add($"Warnings ({_warnings.Count}):");
                lines.AddRange(_warnings.Select(w => $"  - {w}"));
            }

            return lines.Count > 0 ? string.Join(Environment.NewLine, lines) : "Valid";
        }
    }
}
