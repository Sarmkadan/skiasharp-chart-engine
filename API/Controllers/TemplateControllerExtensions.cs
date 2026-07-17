namespace API.Controllers;

/// <summary>
/// Provides extension methods for <see cref="TemplateController"/>.
/// </summary>
public static class TemplateControllerExtensions
{
    /// <summary>
    /// Checks if a template exists by ID.
    /// </summary>
    /// <param name="controller">The <see cref="TemplateController"/> instance.</param>
    /// <param name="templateId">The ID of the template to check.</param>
    /// <returns>True if the template exists, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="controller"/> is null.</exception>
/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="templateId"/> is less than or equal to 0.</exception>
    public static async Task<bool> TemplateExistsAsync(this TemplateController controller, int templateId)
    {
        ArgumentNullException.ThrowIfNull(controller);
	ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(templateId, 0);

        var response = await controller.GetTemplateById(templateId);
        return response.IsSuccess && response.Value is not null;
    }

    /// <summary>
    /// Deletes all templates of a specific type.
    /// </summary>
    /// <param name="controller">The <see cref="TemplateController"/> instance.</param>
    /// <param name="templateType">The type of templates to delete.</param>
    /// <returns>A task that completes when all templates have been deleted.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="controller"/> is null.</exception>
/// <exception cref="ArgumentException">Thrown if <paramref name="templateType"/> is null or empty.</exception>
    public static async Task DeleteAllTemplatesOfTypeAsync(this TemplateController controller, string templateType)
    {
        ArgumentNullException.ThrowIfNull(controller);
	ArgumentException.ThrowIfNullOrEmpty(templateType);

        var templates = await controller.GetTemplatesByType(templateType);
        if (templates.IsSuccess && templates.Value is not null)
        {
            foreach (var template in templates.Value)
            {
                await controller.DeleteTemplateAsync(template.Id);
            }
        }
    }
}
