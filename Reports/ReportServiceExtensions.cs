// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharpChartEngine.Reports;

/// <summary>
/// Extension methods for registering PDF report generation services with the dependency injection container.
/// </summary>
public static class ReportServiceExtensions
{
    /// <summary>
    /// Registers <see cref="IPdfReportGenerator"/> (and its default implementation
    /// <see cref="PdfReportGenerator"/>) as a singleton in the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddPdfReportGenerator(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IPdfReportGenerator, PdfReportGenerator>();
        return services;
    }
}
