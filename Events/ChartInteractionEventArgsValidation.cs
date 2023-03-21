// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace SkiaSharpChartEngine.Events;

/// <summary>
/// Provides validation helpers for <see cref="ChartInteractionEventArgs"/> instances.
/// </summary>
public static class ChartInteractionEventArgsValidation
{
    /// <summary>
    /// Validates a <see cref="ChartInteractionEventArgs"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The event arguments to validate.</param>
    /// <returns>An empty list if valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this ChartInteractionEventArgs value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate PointerX (should be non-negative for screen coordinates)
        if (value.PointerX < 0)
        {
            errors.Add($"PointerX must be non-negative, but was {value.PointerX.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate PointerY (should be non-negative for screen coordinates)
        if (value.PointerY < 0)
        {
            errors.Add($"PointerY must be non-negative, but was {value.PointerY.ToString(CultureInfo.InvariantCulture)}.");
        }

        // Validate Region (should not be default/empty)
        if (value.Region == default)
        {
            errors.Add("Region must be specified.");
        }

        // Validate TooltipText (null or empty strings are invalid)
        if (value.TooltipText is null)
        {
            errors.Add("TooltipText must not be null.");
        }
        else if (string.IsNullOrWhiteSpace(value.TooltipText))
        {
            errors.Add("TooltipText must not be empty or whitespace.");
        }

        // Validate Timestamp (should not be default/MinValue which indicates it wasn't set)
        if (value.Timestamp == default)
        {
            errors.Add("Timestamp must be set to a valid DateTime value.");
        }

        // Validate Metadata (should not be null)
        if (value.Metadata is null)
        {
            errors.Add("Metadata dictionary must not be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="ChartInteractionEventArgs"/> instance is valid.
    /// </summary>
    /// <param name="value">The event arguments to check.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this ChartInteractionEventArgs value)
    {
        if (value is null)
        {
            return false;
        }

        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="ChartInteractionEventArgs"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The event arguments to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is invalid, with a message listing all validation errors.</exception>
    public static void EnsureValid(this ChartInteractionEventArgs value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ChartInteractionEventArgs is invalid. Problems:\n{string.Join("\n", errors)}");
    }
}