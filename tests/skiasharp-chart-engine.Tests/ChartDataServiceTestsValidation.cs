// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

namespace SkiaSharpChartEngine.Tests.Services;

/// <summary>
/// Provides validation helpers for <see cref="ChartDataServiceTests"/> to ensure test data integrity.
/// </summary>
public static class ChartDataServiceTestsValidation
{
    /// <summary>
    /// Validates a <see cref="ChartDataServiceTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The instance to validate</param>
    /// <returns>List of validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> Validate(this ChartDataServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ChartDataServiceTests members
        ValidateChartDataServiceTests(value, problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ChartDataServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static bool IsValid(this ChartDataServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ChartDataServiceTests"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The instance to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid</exception>
    public static void EnsureValid(this ChartDataServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ChartDataServiceTests instance is invalid:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }

    private static void ValidateChartDataServiceTests(ChartDataServiceTests value, List<string> problems)
    {
        // ChartDataServiceTests currently has no instance members to validate
        // This method exists for future extensibility

        if (value == null)
        {
            problems.Add("ChartDataServiceTests instance cannot be null.");
            return;
        }
    }
}
