// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Reflection;

namespace SkiaSharpChartEngine.Tests.Reports;

/// <summary>
/// Provides validation helpers for <see cref="PdfReportGeneratorTests"/> instances.
/// Validates that test instances are properly initialized with required dependencies.
/// </summary>
public static class PdfReportGeneratorTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="PdfReportGeneratorTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this PdfReportGeneratorTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        try
        {
            // Validate private mock dependencies using reflection
            var renderMockField = value.GetType().GetField("_renderMock", BindingFlags.NonPublic | BindingFlags.Instance);
            if (renderMockField?.GetValue(value) is null)
            {
                problems.Add("Render mock dependency is null.");
            }

            var loggerMockField = value.GetType().GetField("_loggerMock", BindingFlags.NonPublic | BindingFlags.Instance);
            if (loggerMockField?.GetValue(value) is null)
            {
                problems.Add("Logger mock dependency is null.");
            }

            // Validate private PdfReportGenerator instance
            var generatorField = value.GetType().GetField("_generator", BindingFlags.NonPublic | BindingFlags.Instance);
            if (generatorField?.GetValue(value) is null)
            {
                problems.Add("PdfReportGenerator instance is null.");
            }
        }
        catch (TargetInvocationException ex)
        {
            problems.Add($"Reflection-based validation failed: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            problems.Add($"Unexpected error during validation: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="PdfReportGeneratorTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this PdfReportGeneratorTests? value)
    {
        return value is null ? throw new ArgumentNullException(nameof(value)) : Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="PdfReportGeneratorTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this PdfReportGeneratorTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"PdfReportGeneratorTests instance is not valid. Problems: {string.Join(", ", problems)}");
        }
    }
}