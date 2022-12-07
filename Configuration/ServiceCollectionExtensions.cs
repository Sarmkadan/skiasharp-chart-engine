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
    /// Add all SkiaSharp Chart Engine services to the DI container
    /// </summary>
    public static IServiceCollection AddSkiaSharpChartEngine(this IServiceCollection services, Action<ChartEngineOptions>? configureOptions = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        var options = new ChartEngineOptions();
        configureOptions?.Invoke(options);

        // Add repositories
        services.AddSingleton<IChartRepository, ChartRepository>();

        // Add services
        services.AddSingleton(typeof(IRenderCacheService), _ => new RenderCacheService(
            _.GetRequiredService<Microsoft.Extensions.Logging.ILogger<RenderCacheService>>(),
            options.CacheSize
        ));

        services.AddSingleton<IChartDataService, ChartDataService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IChartRenderingService, ChartRenderingService>();
        services.AddSingleton<IExportService, ExportService>();

        return services;
    }

    /// <summary>
    /// Add minimal required services for basic chart operations
    /// </summary>
    public static IServiceCollection AddSkiaSharpChartEngineMinimal(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddSingleton<IChartDataService, ChartDataService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        return services;
    }
}
