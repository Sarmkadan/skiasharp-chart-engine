using System;
using System.Collections.Generic;

namespace SkiasharpChartEngine.API.Requests
{
    /// <summary>
    /// Provides extension methods for <see cref="CreateChartRequest"/> to simplify common operations.
    /// </summary>
    public static class CreateChartRequestExtensions
    {
        /// <summary>
        /// Validates the chart request and throws an exception if it's invalid.
        /// </summary>
        /// <param name="request">The chart request to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The request is invalid.</exception>
        public static void Validate(this CreateChartRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!request.IsValid)
            {
                throw new ArgumentException("The chart request is invalid.", nameof(request));
            }
        }

        /// <summary>
        /// Returns the series collection, ensuring it's never null.
        /// </summary>
        /// <param name="request">The chart request.</param>
        /// <returns>The series collection, or an empty list if null.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<ChartSeries> GetSeriesOrDefault(this CreateChartRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return request.Series ?? [];
        }

        /// <summary>
        /// Returns the chart configuration, ensuring it's never null.
        /// </summary>
        /// <param name="request">The chart request.</param>
        /// <returns>The chart configuration, or a new instance if null.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
        public static ChartConfiguration GetConfigurationOrDefault(this CreateChartRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return request.Configuration ?? new ChartConfiguration();
        }
    }
}