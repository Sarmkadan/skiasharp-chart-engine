using System;
using System.Collections.Generic;

namespace SkiaSharpChartEngine.Tests;

/// <summary>
/// Provides validation extension methods for <see cref="ChartEngineTests"/> instances.
/// </summary>
public static class ChartEngineTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="ChartEngineTests"/> instance.
    /// </summary>
    /// <remarks>
    /// ChartEngineTests is a test fixture class with no public state to validate.
    /// This validation method ensures the instance is not null and that it's a valid test fixture.
    /// For a non-null ChartEngineTests instance, validation always succeeds.
    /// </remarks>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ChartEngineTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ChartEngineTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ChartEngineTests value) => value.Validate() is not null;

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