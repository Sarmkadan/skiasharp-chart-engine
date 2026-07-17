using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.API.Controllers;

/// <summary>
/// Provides validation helpers for <see cref="DataController"/> operations.
/// Validates method parameters and data integrity before processing.
/// </summary>
public static class DataControllerValidation
{
    /// <summary>
    /// Validates parameters for <see cref="DataController.GetDataStatisticsAsync"/> method.
    /// </summary>
    /// <param name="seriesId">The series identifier</param>
    /// <returns>List of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentException">Thrown if seriesId is null, empty, or whitespace</exception>
    public static IReadOnlyList<string> Validate(this string seriesId)
    {
        ArgumentException.ThrowIfNullOrEmpty(seriesId);

        if (string.IsNullOrWhiteSpace(seriesId))
        {
            return new[] { "Series ID cannot be null, empty, or whitespace." };
        }

        return Array.Empty<string>();
    }

    /// <summary>
    /// Validates parameters for <see cref="DataController.ValidateDataPoints"/> method.
    /// </summary>
    /// <param name="dataPoints">The list of data points to validate</param>
    /// <returns>List of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if dataPoints is null</exception>
    public static IReadOnlyList<string> Validate(this List<DataPoint> dataPoints)
    {
        ArgumentNullException.ThrowIfNull(dataPoints);

        var errors = new List<string>();

        if (dataPoints.Count == 0)
        {
            errors.Add("Data points list cannot be empty.");
        }

        // Validate each data point
        for (int i = 0; i < dataPoints.Count; i++)
        {
            var errorsForPoint = dataPoints[i].Validate();
            if (errorsForPoint.Count > 0)
            {
                errors.AddRange(errorsForPoint.Select(e => $"DataPoint[{i}]: {e}"));
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates a single <see cref="DataPoint"/> instance.
    /// </summary>
    /// <param name="dataPoint">The data point to validate</param>
    /// <returns>List of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if dataPoint is null</exception>
    public static IReadOnlyList<string> Validate(this DataPoint dataPoint)
    {
        ArgumentNullException.ThrowIfNull(dataPoint);

        var errors = new List<string>();

        // Validate X coordinate
        if (double.IsNaN(dataPoint.X))
        {
            errors.Add("X coordinate cannot be NaN.");
        }

        if (double.IsInfinity(dataPoint.X))
        {
            errors.Add("X coordinate cannot be Infinity.");
        }

        // Validate Y coordinate
        if (double.IsNaN(dataPoint.Y))
        {
            errors.Add("Y coordinate cannot be NaN.");
        }

        if (double.IsInfinity(dataPoint.Y))
        {
            errors.Add("Y coordinate cannot be Infinity.");
        }

        // Validate Color
        if (string.IsNullOrWhiteSpace(dataPoint.Color))
        {
            errors.Add("Color cannot be null, empty, or whitespace.");
        }
        else if (!IsValidHexColor(dataPoint.Color))
        {
            errors.Add("Color must be a valid hexadecimal color code (e.g., #RRGGBB or #RRGGBBAA).");
        }

        // Validate Label (optional but if present, should not be whitespace)
        if (dataPoint.Label != null && string.IsNullOrWhiteSpace(dataPoint.Label))
        {
            errors.Add("Label cannot be whitespace if provided.");
        }

        // Validate CustomRadius
        if (dataPoint.CustomRadius.HasValue && dataPoint.CustomRadius.Value <= 0)
        {
            errors.Add("CustomRadius must be positive if specified.");
        }

        // Validate Timestamp (if present, should be reasonable)
        if (dataPoint.Timestamp.HasValue)
        {
            var now = DateTime.UtcNow;
            if (dataPoint.Timestamp.Value > now.AddYears(1))
            {
                errors.Add("Timestamp cannot be more than 1 year in the future.");
            }

            if (dataPoint.Timestamp.Value < DateTime.MinValue.AddYears(1))
            {
                errors.Add("Timestamp cannot be before year 1.");
            }
        }

        // Validate State enum value
        try
        {
            var _ = dataPoint.State; // Access to ensure it's a valid enum value
        }
        catch (Exception)
        {
            errors.Add("State must be a valid DataPointState enum value.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="DataController.AggregateDataAsync"/> method.
    /// </summary>
    /// <param name="dataPoints">The list of data points to aggregate</param>
    /// <param name="aggregationType">The aggregation type identifier</param>
    /// <param name="bucketSize">The bucket size for aggregation</param>
    /// <returns>List of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if dataPoints is null</exception>
    /// <exception cref="ArgumentException">Thrown if aggregationType is null, empty, or whitespace</exception>
    public static IReadOnlyList<string> Validate(this List<DataPoint> dataPoints, string aggregationType, int bucketSize)
    {
        ArgumentNullException.ThrowIfNull(dataPoints);
        ArgumentException.ThrowIfNullOrEmpty(aggregationType);

        var errors = new List<string>();

        // Validate dataPoints
        errors.AddRange(dataPoints.Validate());

        // Validate aggregationType
        if (string.IsNullOrWhiteSpace(aggregationType))
        {
            errors.Add("Aggregation type cannot be null, empty, or whitespace.");
        }

        // Validate bucketSize
        if (bucketSize <= 0)
        {
            errors.Add("Bucket size must be positive.");
        }
        else if (bucketSize > 100000)
        {
            errors.Add("Bucket size cannot exceed 100,000.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="DataController.FilterByRange"/> method.
    /// </summary>
    /// <param name="dataPoints">The list of data points to filter</param>
    /// <param name="minValue">The minimum value for filtering</param>
    /// <param name="maxValue">The maximum value for filtering</param>
    /// <returns>List of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if dataPoints is null</exception>
    public static IReadOnlyList<string> Validate(this List<DataPoint> dataPoints, double minValue, double maxValue)
    {
        ArgumentNullException.ThrowIfNull(dataPoints);

        var errors = new List<string>();

        // Validate dataPoints
        errors.AddRange(dataPoints.Validate());

        // Validate minValue and maxValue
        if (minValue > maxValue)
        {
            errors.Add("Minimum value cannot be greater than maximum value.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for <see cref="DataController.ResampleDataAsync"/> method.
    /// </summary>
    /// <param name="dataPoints">The list of data points to resample</param>
    /// <param name="targetCount">The target number of data points</param>
    /// <returns>List of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if dataPoints is null</exception>
    public static IReadOnlyList<string> Validate(this List<DataPoint> dataPoints, int targetCount)
    {
        ArgumentNullException.ThrowIfNull(dataPoints);

        var errors = new List<string>();

        // Validate dataPoints
        errors.AddRange(dataPoints.Validate());

        // Validate targetCount
        if (targetCount <= 0)
        {
            errors.Add("Target count must be positive.");
        }
        else if (targetCount > 100000)
        {
            errors.Add("Target count cannot exceed 100,000.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a series ID is valid.
    /// </summary>
    /// <param name="seriesId">The series identifier</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this string seriesId) => Validate(seriesId).Count == 0;

    /// <summary>
    /// Ensures a series ID is valid, throwing if not.
    /// </summary>
    /// <param name="seriesId">The series identifier</param>
    /// <exception cref="ArgumentException">Thrown if seriesId is invalid</exception>
    public static void EnsureValid(this string seriesId)
    {
        var errors = Validate(seriesId);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors), nameof(seriesId));
        }
    }

    /// <summary>
    /// Checks if a list of data points is valid.
    /// </summary>
    /// <param name="dataPoints">The list of data points</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this List<DataPoint> dataPoints) => Validate(dataPoints).Count == 0;

    /// <summary>
    /// Ensures a list of data points is valid, throwing if not.
    /// </summary>
    /// <param name="dataPoints">The list of data points</param>
    /// <exception cref="ArgumentException">Thrown if dataPoints is invalid</exception>
    public static void EnsureValid(this List<DataPoint> dataPoints)
    {
        var errors = Validate(dataPoints);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors), nameof(dataPoints));
        }
    }

    /// <summary>
    /// Checks if a data point is valid.
    /// </summary>
    /// <param name="dataPoint">The data point</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this DataPoint dataPoint) => Validate(dataPoint).Count == 0;

    /// <summary>
    /// Ensures a data point is valid, throwing if not.
    /// </summary>
    /// <param name="dataPoint">The data point</param>
    /// <exception cref="ArgumentException">Thrown if dataPoint is invalid</exception>
    public static void EnsureValid(this DataPoint dataPoint)
    {
        var errors = Validate(dataPoint);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors), nameof(dataPoint));
        }
    }

    /// <summary>
    /// Checks if aggregation parameters are valid.
    /// </summary>
    /// <param name="dataPoints">The data points</param>
    /// <param name="aggregationType">The aggregation type</param>
    /// <param name="bucketSize">The bucket size</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this List<DataPoint> dataPoints, string aggregationType, int bucketSize) =>
        Validate(dataPoints, aggregationType, bucketSize).Count == 0;

    /// <summary>
    /// Ensures aggregation parameters are valid, throwing if not.
    /// </summary>
    /// <param name="dataPoints">The data points</param>
    /// <param name="aggregationType">The aggregation type</param>
    /// <param name="bucketSize">The bucket size</param>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid</exception>
    public static void EnsureValid(this List<DataPoint> dataPoints, string aggregationType, int bucketSize)
    {
        var errors = Validate(dataPoints, aggregationType, bucketSize);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors), nameof(dataPoints));
        }
    }

    /// <summary>
    /// Checks if filter parameters are valid.
    /// </summary>
    /// <param name="dataPoints">The data points</param>
    /// <param name="minValue">The minimum value</param>
    /// <param name="maxValue">The maximum value</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this List<DataPoint> dataPoints, double minValue, double maxValue) =>
        Validate(dataPoints, minValue, maxValue).Count == 0;

    /// <summary>
    /// Ensures filter parameters are valid, throwing if not.
    /// </summary>
    /// <param name="dataPoints">The data points</param>
    /// <param name="minValue">The minimum value</param>
    /// <param name="maxValue">The maximum value</param>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid</exception>
    public static void EnsureValid(this List<DataPoint> dataPoints, double minValue, double maxValue)
    {
        var errors = Validate(dataPoints, minValue, maxValue);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors), nameof(dataPoints));
        }
    }

    /// <summary>
    /// Checks if resample parameters are valid.
    /// </summary>
    /// <param name="dataPoints">The data points</param>
    /// <param name="targetCount">The target count</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this List<DataPoint> dataPoints, int targetCount) =>
        Validate(dataPoints, targetCount).Count == 0;

    /// <summary>
    /// Ensures resample parameters are valid, throwing if not.
    /// </summary>
    /// <param name="dataPoints">The data points</param>
    /// <param name="targetCount">The target count</param>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid</exception>
    public static void EnsureValid(this List<DataPoint> dataPoints, int targetCount)
    {
        var errors = Validate(dataPoints, targetCount);
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("\n", errors), nameof(dataPoints));
        }
    }

    private static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return false;
        }

        if (!color.StartsWith("#", StringComparison.Ordinal))
        {
            return false;
        }

        var hex = color.Substring(1);
        return hex.Length == 6 || hex.Length == 8;
    }
}