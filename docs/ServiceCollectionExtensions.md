# ServiceCollectionExtensions

`ServiceCollectionExtensions` provides dependency-injection registration methods for SkiaSharp Chart Engine applications. The extensions are defined in the `SkiaSharpChartEngine.Configuration` namespace and return the same `IServiceCollection`, so they can be chained with other service registrations.

## Full registration

```csharp
IServiceCollection AddSkiaSharpChartEngine(
    this IServiceCollection services,
    Action<ChartEngineOptions>? configureOptions = null)
```

`AddSkiaSharpChartEngine` registers the complete core service graph:

| Service | Implementation | Lifetime |
| --- | --- | --- |
| `IChartRepository` | `ChartRepository` | Singleton |
| `IRenderCacheService` | `RenderCacheService` | Singleton |
| `IChartDataService` | `ChartDataService` | Singleton |
| `IConfigurationService` | `ConfigurationService` | Singleton |
| `IChartRenderingService` | `ChartRenderingService` | Singleton |
| `IExportService` | `ExportService` | Singleton |
| `ChartEngine` | `ChartEngine` | Singleton |

The optional callback configures a `ChartEngineOptions` instance when services are registered. The render cache is constructed with the configured `CacheSize`. Options are captured at registration time; this extension does not register `ChartEngineOptions` or use the .NET options pattern.

If `configureOptions` is omitted, the defaults from `ChartEngineOptions` are used. Passing a null `services` value throws `ArgumentNullException`.

## Minimal registration

```csharp
IServiceCollection AddSkiaSharpChartEngineMinimal(
    this IServiceCollection services)
```

`AddSkiaSharpChartEngineMinimal` registers only these singleton services:

| Service | Implementation |
| --- | --- |
| `IChartDataService` | `ChartDataService` |
| `IConfigurationService` | `ConfigurationService` |

Use this method when an application needs basic chart data and configuration operations but does not need the repository, render cache, rendering, export, or `ChartEngine` facade registrations. Passing a null `services` value throws `ArgumentNullException`.

## ASP.NET Core `Program.cs` example

```csharp
using SkiaSharpChartEngine.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSkiaSharpChartEngine(options =>
{
    options.CacheSize = 250;
});

var app = builder.Build();

app.MapControllers();
app.Run();
```

Applications that use a `Startup` class can make the equivalent registration in `ConfigureServices`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using SkiaSharpChartEngine.Configuration;

public sealed class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSkiaSharpChartEngine(options =>
        {
            options.CacheSize = 250;
        });
    }
}
```

Services can then request the facade or one of its registered abstractions through constructor injection:

```csharp
using SkiaSharpChartEngine;

public sealed class ChartApplicationService
{
    private readonly ChartEngine _chartEngine;

    public ChartApplicationService(ChartEngine chartEngine)
    {
        _chartEngine = chartEngine;
    }
}
```

Use `AddSkiaSharpChartEngine`, rather than the minimal registration, when resolving `ChartEngine` or the rendering and export services.
