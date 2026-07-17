using System;
using System.Collections.Generic;

namespace SkiaSharpChartEngine.Tests.Services;

/// <summary>
/// Provides validation helpers for <see cref="ChartRenderingServiceTests"/> instances.
/// Validates that test instances are properly initialized and contain valid test dependencies.
/// </summary>
public static class ChartRenderingServiceTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="ChartRenderingServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ChartRenderingServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate mock dependencies are not null
        if (value.LoggerMock is null)
        {
            problems.Add("Logger mock dependency is null.");
        }

        if (value.DataServiceMock is null)
        {
            problems.Add("Data service mock dependency is null.");
        }

        if (value.CacheServiceMock is null)
        {
            problems.Add("Cache service mock dependency is null.");
        }

        if (value.Service is null)
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ChartRenderingServiceTests value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ChartRenderingServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
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
