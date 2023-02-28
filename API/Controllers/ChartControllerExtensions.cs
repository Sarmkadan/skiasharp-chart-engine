namespace Skiasharp.ChartEngine.API.Controllers;

/// <summary>
/// Extension methods for <see cref="ChartController"/>.
/// </summary>
public static class ChartControllerExtensions
{
    /// <summary>
    /// Retrieves a chart by its ID.
    /// </summary>
    /// <param name="controller">The <see cref="ChartController"/> instance.</param>
    /// <param name="id">The ID of the chart to retrieve.</param>
    /// <returns>A task that yields an <see cref="ApiResponse{ChartDto}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    public static async Task<ApiResponse<ChartDto>> GetChartByIdAsync(this ChartController controller, string id)
    {
        ArgumentNullException.ThrowIfNull(controller);
        if (controller.Id != id)
        {
            return ApiResponse<ChartDto>.NotFound("Chart not found");
        }
        return await controller.GetChartAsync();
    }

    /// <summary>
    /// Updates a chart's configuration.
    /// </summary>
    /// <param name="controller">The <see cref="ChartController"/> instance.</param>
    /// <param name="configuration">The new configuration.</param>
    /// <returns>A task that yields an <see cref="ApiResponse{bool}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="configuration"/> is null.</exception>
    public static async Task<ApiResponse<bool>> UpdateChartConfigurationAsync(this ChartController controller, ChartConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(configuration);
        controller.Configuration = configuration;
        return await controller.UpdateChartAsync();
    }

    /// <summary>
    /// Deletes a chart and its associated data.
    /// </summary>
    /// <param name="controller">The <see cref="ChartController"/> instance.</param>
    /// <returns>A task that yields an <see cref="ApiResponse{bool}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    public static async Task<ApiResponse<bool>> DeleteChartAndDataAsync(this ChartController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        var deleteResponse = await controller.DeleteChartAsync();
        if (deleteResponse.IsSuccess)
        {
            // Additional logic to delete associated data can be added here
        }
        return deleteResponse;
    }
}
