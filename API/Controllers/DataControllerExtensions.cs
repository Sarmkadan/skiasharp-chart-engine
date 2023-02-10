using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace skiasharp_chart_engine.API.Controllers
{
    public static class DataControllerExtensions
    {
        public static async Task<ApiResponse<List<DataPoint>>> GetDataPointsInRangeAsync(this DataController controller, double minX, double maxX)
        {
            var dataPointsResponse = await controller.AggregateDataAsync();
            if (dataPointsResponse.IsSuccess)
            {
                return controller.FilterByRange(dataPointsResponse.Value, minX, maxX);
            }
            return new ApiResponse<List<DataPoint>>(dataPointsResponse.StatusCode, dataPointsResponse.Message);
        }

        public static async Task<ApiResponse<object>> ValidateAndResampleDataAsync(this DataController controller, List<DataPoint> dataPoints, int newSamplingRate)
        {
            var validationResponse = controller.ValidateDataPoints(dataPoints);
            if (validationResponse.IsSuccess)
            {
                return await controller.ResampleDataAsync(dataPoints, newSamplingRate);
            }
            return new ApiResponse<object>(validationResponse.StatusCode, validationResponse.Message);
        }

        public static async Task<ApiResponse<List<DataPoint>>> AggregateAndFilterDataAsync(this DataController controller, List<DataPoint> dataPoints, double minX, double maxX)
        {
            var aggregatedResponse = await controller.AggregateDataAsync();
            if (aggregatedResponse.IsSuccess)
            {
                return controller.FilterByRange(aggregatedResponse.Value, minX, maxX);
            }
            return new ApiResponse<List<DataPoint>>(aggregatedResponse.StatusCode, aggregatedResponse.Message);
        }
    }
}
