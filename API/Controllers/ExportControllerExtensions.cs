namespace API.Controllers;

/// <summary>
/// Provides extension methods for <see cref="ExportController"/>.
/// </summary>
public static class ExportControllerExtensions
{
    /// <summary>
    /// Determines if the export controller has any failed renders.
    /// </summary>
    /// <param name="controller">The export controller.</param>
    /// <returns>true if the export controller has any failed renders; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    public static bool HasFailedRenders(this ExportController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        return controller.FailedRenders > 0;
    }

    /// <summary>
    /// Gets the total number of charts rendered by the export controller.
    /// </summary>
    /// <param name="controller">The export controller.</param>
    /// <returns>The total number of charts rendered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    public static int GetTotalChartsRendered(this ExportController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        return controller.TotalCharts;
    }

    /// <summary>
    /// Gets a read-only list of individual render results.
    /// </summary>
    /// <param name="controller">The export controller.</param>
    /// <returns>A read-only list of individual render results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is null.</exception>
    public static IReadOnlyList<IndividualRenderResult> GetRenderResults(this ExportController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        return controller.RenderResults.AsReadOnly();
    }
}