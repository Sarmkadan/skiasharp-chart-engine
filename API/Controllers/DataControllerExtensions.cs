using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SkiaSharpChartEngine.API.Controllers
{
    /// <summary>
    /// Extension methods for <see cref="DataController"/> that provide data processing operations.
    /// </summary>
    public static class DataControllerExtensions
    {
        /// <summary>
        /// Gets data points filtered by X-axis range.
        /// </summary>
        /// <param name="controller">The controller instance.</param>
        /// <param name="minX">Minimum X value (inclusive).</param>
        /// <param name="maxX">Maximum X value (inclusive).</param>
        /// <returns>Filtered data points or error response.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="minX"/> is greater than <paramref name="maxX"/>.</exception>
        public static async Task<ApiResponse<List<DataPoint>>> GetDataPointsInRangeAsync(
            this DataController controller,
            double minX,
            double maxX)
        {
            ArgumentNullException.ThrowIfNull(controller);

            if (minX > maxX)
            {
                return ApiResponse<List<DataPoint>>.BadRequest("Minimum X value cannot be greater than maximum X value.");
            }

            var dataPointsResponse = await controller.AggregateDataAsync();
            return dataPointsResponse.IsSuccess
                ? controller.FilterByRange(dataPointsResponse.Value, minX, maxX)
                : new ApiResponse<List<DataPoint>>(dataPointsResponse.StatusCode, dataPointsResponse.Message);
        }

        /// <summary>
        /// Validates data points and resamples them to a new sampling rate if validation succeeds.
        /// </summary>
        /// <param name="controller">The controller instance.</param>
        /// <param name="dataPoints">Data points to validate and resample.</param>
        /// <param name="newSamplingRate">Target number of data points after resampling.</param>
        /// <returns>Validation result or resampled data.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="dataPoints"/> is null.</exception>
        public static async Task<ApiResponse<object>> ValidateAndResampleDataAsync(
            this DataController controller,
            List<DataPoint> dataPoints,
            int newSamplingRate)
        {
            ArgumentNullException.ThrowIfNull(controller);
            ArgumentNullException.ThrowIfNull(dataPoints);

            var validationResponse = controller.ValidateDataPoints(dataPoints);
            return validationResponse.IsSuccess
                ? await controller.ResampleDataAsync(dataPoints, newSamplingRate)
                : new ApiResponse<object>(validationResponse.StatusCode, validationResponse.Message);
        }

        /// <summary>
        /// Aggregates data points and filters them by X-axis range.
        /// </summary>
        /// <param name="controller">The controller instance.</param>
        /// <param name="dataPoints">Data points to aggregate and filter.</param>
        /// <param name="minX">Minimum X value (inclusive).</param>
        /// <param name="maxX">Maximum X value (inclusive).</param>
        /// <returns>Aggregated and filtered data points or error response.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="controller"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="dataPoints"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="minX"/> is greater than <paramref name="maxX"/>.</exception>
        public static async Task<ApiResponse<List<DataPoint>>> AggregateAndFilterDataAsync(
            this DataController controller,
            List<DataPoint> dataPoints,
            double minX,
            double maxX)
        {
            ArgumentNullException.ThrowIfNull(controller);
            ArgumentNullException.ThrowIfNull(dataPoints);

            if (minX > maxX)
            {
                return ApiResponse<List<DataPoint>>.BadRequest("Minimum X value cannot be greater than maximum X value.");
            }

            var aggregatedResponse = await controller.AggregateDataAsync(dataPoints, "average", 1);
            return aggregatedResponse.IsSuccess
                ? controller.FilterByRange(aggregatedResponse.Value, minX, maxX)
                : new ApiResponse<List<DataPoint>>(aggregatedResponse.StatusCode, aggregatedResponse.Message);
        }
    }
}