# StreamingServiceExtensions

`StreamingServiceExtensions` is a static class in the `SkiaSharpChartEngine.Streaming` namespace. It provides one extension method, `AddChartStreaming`, which registers the real-time chart streaming service in a `Microsoft.Extensions.DependencyInjection` container. The source is in `Streaming/StreamingServiceExtensions.cs`.

The `Streaming/` folder is not excluded from the build in `skiasharp-chart-engine.csproj`, so this class is compiled into the library.

## API

### `AddChartStreaming`

```csharp
public static IServiceCollection AddChartStreaming(this IServiceCollection services)
```

Registers `IChartStreamingService` as a singleton, with `ChartStreamingService` as its implementation. The registration is a plain `AddSingleton<IChartStreamingService, ChartStreamingService>()` call. It does not register any of the dependencies that `ChartStreamingService` needs.

- **Parameters:**
  - `services` (`IServiceCollection`, `this`): The service collection to configure.
- **Returns:**
  - `IServiceCollection`: The same instance, for fluent chaining.
- **Exceptions:**
  - `ArgumentNullException`: Thrown when `services` is `null`.

## Dependencies

The container must be able to resolve the following services when `IChartStreamingService` is first requested. `ChartStreamingService` has a single public constructor and throws `ArgumentNullException` for any argument that is `null`.

| Parameter | Type | Source |
|-----------|------|--------|
| `renderingService` | `IChartRenderingService` | Must be registered by the caller, for example by `AddSkiaSharpChartEngine()`. |
| `logger` | `ILogger<ChartStreamingService>` | Provided by the logging infrastructure (`Microsoft.Extensions.Logging`). |

If `IChartRenderingService` is missing, the failure happens when the service is resolved, not when `AddChartStreaming` is called.

## Lifetime

`IChartStreamingService` is registered as a singleton. `ChartStreamingService` implements `IDisposable`. When the container is disposed, it disposes the singleton instance.

## Usage

```csharp
services.AddSkiaSharpChartEngine();   // registers IChartRenderingService
services.AddChartStreaming();

// Later, resolve the streaming service from the container.
var streaming = provider.GetRequiredService<IChartStreamingService>();
```

The order of the two calls does not matter, because resolution happens after the container is built.

## Notes

- The method does not use `TryAddSingleton`. Calling `AddChartStreaming` more than once adds another registration for the same service type. `GetService` and `GetRequiredService` then return the last registration.
- The method is not called anywhere else in the current source tree. Callers must invoke it themselves.
- The implementation is documented in `docs/ChartStreamingService.md`.
