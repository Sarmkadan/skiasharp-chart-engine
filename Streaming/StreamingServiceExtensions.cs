// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;

namespace SkiaSharpChartEngine.Streaming;

/// <summary>
/// Extension methods for registering real-time chart streaming services with the DI container.
/// </summary>
public static class StreamingServiceExtensions
{
    /// <summary>
    /// Registers <see cref="IChartStreamingService"/> (and its default implementation
    /// <see cref="ChartStreamingService"/>) as a singleton in the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddChartStreaming(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.AddSingleton<IChartStreamingService, ChartStreamingService>();
        return services;
    }
}
