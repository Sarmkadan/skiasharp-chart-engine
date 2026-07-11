// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Validation helpers for <see cref="ChartInteractionServiceTests"/>.
/// </summary>
public static class ChartInteractionServiceTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="ChartInteractionServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ChartInteractionServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // ChartInteractionServiceTests is a test fixture class with no public state members
        // All validation is structural (null checks already performed above)
        // This ensures the validation contract is complete even if no data members exist

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ChartInteractionServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ChartInteractionServiceTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ChartInteractionServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, listing all problems.</exception>
    public static void EnsureValid(this ChartInteractionServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            string.Join("\n", problems),
            nameof(value));
    }
}