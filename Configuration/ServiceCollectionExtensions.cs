// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using SkiaSharpChartEngine.Repository;
using SkiaSharpChartEngine.Services;

namespace SkiaSharpChartEngine.Configuration;

/// <summary>
/// Extension methods for dependency injection setup
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all SkiaSharp Chart Engine services to the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">Optional action to configure <see cref="ChartEngineOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddSkiaSharpChartEngine(this IServiceCollection services, Action<ChartEngineOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new ChartEngineOptions();
        configureOptions?.Invoke(options);

        // Add repositories
        services.AddSingleton<IChartRepository, ChartRepository>();

        // Add services
        services.AddSingleton<IRenderCacheService>(provider => new RenderCacheService(
            provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RenderCacheService>>(),
            options.CacheSize
        ));

        services.AddSingleton<IChartDataService, ChartDataService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IChartRenderingService, ChartRenderingService>();
        services.AddSingleton<IExportService, ExportService>();

        return services;
    }

    /// <summary>
    /// Adds minimal required services for basic chart operations.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="services"/> is <see langword="null"/>.</exception>
    public static IServiceCollection AddSkiaSharpChartEngineMinimal(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IChartDataService, ChartDataService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        return services;
    }
}