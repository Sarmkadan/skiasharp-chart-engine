using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SkiaSharpChartEngine.Tests.Services;

/// <summary>
/// Provides validation helpers for <see cref="ChartRenderingServiceTests"/> instances.
/// Validates that test instances are properly initialized and contain valid test data.
/// </summary>
public static class ChartRenderingServiceTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="ChartRenderingServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    public static IReadOnlyList<string> Validate(this ChartRenderingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate mock dependencies are not null
        if (value.GetType().GetField("_loggerMock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            problems.Add("Logger mock dependency is null.");
        }

        if (value.GetType().GetField("_dataServiceMock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            problems.Add("Data service mock dependency is null.");
        }

        if (value.GetType().GetField("_cacheServiceMock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            problems.Add("Cache service mock dependency is null.");
        }

        if (value.GetType().GetField("_service", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(value) is null)
        {
            problems.Add("ChartRenderingService instance is null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ChartRenderingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this ChartRenderingServiceTests value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ChartRenderingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid, containing a list of validation problems.</exception>
    public static void EnsureValid(this ChartRenderingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ChartRenderingServiceTests instance is not valid. Problems: {string.Join(" ", problems)}");
        }
    }
}
