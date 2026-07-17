using System;
using System.Collections.Generic;

namespace SkiaSharpChartEngine.Tests.Caching;

/// <summary>
/// Provides validation helpers for <see cref="RenderResultCacheTests"/> instances.
/// Validates that test instances are in a valid state before execution.
/// </summary>
public static class RenderResultCacheTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="RenderResultCacheTests"/> instance.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RenderResultCacheTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="RenderResultCacheTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this RenderResultCacheTests value) => value?.Validate().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="RenderResultCacheTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The test instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance has validation problems.</exception>
    public static void EnsureValid(this RenderResultCacheTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RenderResultCacheTests instance is not valid. Problems:\n{string.Join("\n", problems)}");
        }
    }

}
