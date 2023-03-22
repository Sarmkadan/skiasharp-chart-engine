using System;
using System.Collections.Generic;
using System.Globalization;

namespace SkiaSharpChartEngine.Tests;

/// <summary>
/// Provides validation helpers for <see cref="ChartEngineTests"/> instances.
/// </summary>
public static class ChartEngineTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="ChartEngineTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ChartEngineTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // ChartEngineTests is a test fixture class with no public state to validate
        // The validation is primarily about ensuring the instance itself is not null
        // and that it's a valid test fixture (which is always true for a non-null instance)

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ChartEngineTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ChartEngineTests value)
    {
        try
        {
            _ = value.Validate();
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that the specified <see cref="ChartEngineTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this ChartEngineTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                string.Join(Environment.NewLine, problems),
                nameof(value));
        }
    }
}