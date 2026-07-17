namespace SkiaSharpChartEngine.API.Controllers;

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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is null or empty.</exception>
    public static async Task<ApiResponse<ChartDto>> GetChartByIdAsync(this ChartController controller, string id)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(id);

        return await controller.GetChartAsync(id);
    }

    /// <summary>
    /// Updates a chart's configuration.
    /// </summary>
    /// <param name="controller">The <see cref="ChartController"/> instance.</param>
    /// <param name="id">The ID of the chart to update.</param>
    /// <param name="configuration">The new configuration.</param>
    /// <returns>A task that yields an <see cref="ApiResponse{bool}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> or <paramref name="configuration"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    public static async Task<ApiResponse<bool>> UpdateChartConfigurationAsync(
        this ChartController controller,
        string id,
        ChartConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(id);

        var updateRequest = new UpdateChartRequest
        {
            Configuration = configuration
        };

        return await controller.UpdateChartAsync(id, updateRequest);
    }

    /// <summary>
    /// Deletes a chart and its associated data.
    /// </summary>
    /// <param name="controller">The <see cref="ChartController"/> instance.</param>
    /// <param name="id">The ID of the chart to delete.</param>
    /// <returns>A task that yields an <see cref="ApiResponse{bool}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    public static async Task<ApiResponse<bool>> DeleteChartAndDataAsync(
        this ChartController controller,
        string id)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentException.ThrowIfNullOrEmpty(id);

        return await controller.DeleteChartAsync(id);
    }
}
