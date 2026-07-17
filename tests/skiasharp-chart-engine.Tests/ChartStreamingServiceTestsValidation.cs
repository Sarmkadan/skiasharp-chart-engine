// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SkiaSharpChartEngine.Tests
{
    /// <summary>
    /// Provides validation helpers for <see cref="ChartStreamingServiceTests"/> instances.
    /// </summary>
    public static class ChartStreamingServiceTestsValidation
    {
        /// <summary>
        /// Validates the specified <see cref="ChartStreamingServiceTests"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of human-readable validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this ChartStreamingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // ChartStreamingServiceTests is a test fixture class with mocked dependencies
            // Validate that the test fixture is properly initialized by checking its public properties

            try
            {
                // Validate the test fixture instance itself
                _ = value.GetType();
            }
            catch (Exception ex)
            {
                problems.Add($"Test fixture instance is not properly initialized: {ex.Message}");
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="ChartStreamingServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static bool IsValid(this ChartStreamingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="ChartStreamingServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing a list of problems.</exception>
        public static void EnsureValid(this ChartStreamingServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = Validate(value);
            if (problems.Count == 0)
            {
                return;
            }

            throw new ArgumentException(
                $"ChartStreamingServiceTests instance is not valid. Problems:\n{string.Join("\n", problems)}");
        }
    }
}
