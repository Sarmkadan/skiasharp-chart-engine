using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiasharpChartEngine.API.Requests
{
    public static class CreateChartRequestExtensions
    {
        /// <summary>
        /// Validates the chart request and throws an exception if it's invalid.
        /// </summary>
        /// <param name="request">The chart request to validate.</param>
        /// <exception cref="ArgumentException">Thrown if the request is invalid.</exception>
        public static void Validate(this CreateChartRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!request.IsValid)
            {
                throw new ArgumentException("The chart request is invalid.", nameof(request));
            }
        }

        /// <summary>
        /// Returns a list of chart series with default values if the series are null.
        /// </summary>
        /// <param name="request">The chart request.</param>
        /// <returns>A list of chart series.</returns>
        public static List<ChartSeries> GetSeriesOrDefault(this CreateChartRequest request)
        {
            return request.Series ?? new List<ChartSeries>();
        }

        /// <summary>
        /// Returns the chart configuration with default values if the configuration is null.
        /// </summary>
        /// <param name="request">The chart request.</param>
        /// <returns>The chart configuration.</returns>
        public static ChartConfiguration GetConfigurationOrDefault(this CreateChartRequest request)
        {
            return request.Configuration ?? new ChartConfiguration();
        }
    }
}
