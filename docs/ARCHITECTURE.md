# Architecture

This document describes the actual structure of the codebase as it exists today. Where behavior differs from older docs (e.g. `docs/architecture.md`), this file reflects the code.

## Overview

The repository is a single .NET 10 project (`skiasharp-chart-engine.csproj`) that combines:

1. **A chart-rendering library** built around the `ChartEngine` facade (`ChartEngine.cs`), usable standalone via `ChartEngine.Create()`.
2. **An ASP.NET Core Web API host** (`Program.cs` + `API/Controllers/`) exposing chart CRUD, rendering, export, template, and health endpoints.
3. **Supporting subsystems** (caching, streaming, events, workers, CLI plumbing) that are compiled in but mostly not wired into the web host - see "Known limitations".

All chart drawing is done with SkiaSharp (CPU rasterization); export formats are detected from file extension (PNG, JPEG, SVG, PDF, WebP - see `ChartEngineExtensions.DetectFormatFromPath`).

## Component breakdown

| Directory | Contents |
|---|---|
| `ChartEngine.cs` | Facade. Resolves `IChartRenderingService`, `IChartRepository`, `IExportService`, `IConfigurationService`, `IChartDataService` from an `IServiceProvider` and exposes sync + async render/export/CRUD methods. |
| `Configuration/` | `ServiceCollectionExtensions.AddSkiaSharpChartEngine()` - the DI composition root for the library; `ChartEngineOptions`. |
| `Services/` | Core service implementations: `ChartRenderingService` (validation → cache check → SkiaSharp draw → encode), `ExportService` (format selection + file I/O, delegates raster work to the rendering service), `ChartDataService` (validation/statistics), `ConfigurationService` (defaults + templates), `RenderCacheService` (in-memory render cache, size from `ChartEngineOptions.CacheSize`). |
| `Repository/` | `ChartRepository` - in-memory `Dictionary<string, Chart>` store behind `IChartRepository`. No persistence; contents are lost on restart. |
| `Rendering/` | Per-chart-type renderers (`LineChartRenderer`, `BarChartRenderer`, `PieChartRenderer`, `ScatterPlotRenderer`, `HeatmapRenderer`) and `AnimationFrameGenerator`. |
| `API/` | ASP.NET Core controllers (`ChartController`, `DataController`, `ExportController`, `TemplateController`, `HealthController`) plus request/response DTOs. |
| `Middleware/` | Error handling, logging, rate limiting, performance monitoring, request validation middleware with `Use*` extension methods. **Not added to the pipeline in `Program.cs`** (see limitations). |
| `Models/` | `Chart`, `ChartSeries`, `DataPoint`, `ChartConfiguration`, `RenderResult`, `ExportOptions`, enums. |
| `Caching/` | Cache key building, cache policies, `RenderResultCache`, `DistributedCacheService`. |
| `Pipeline/` | `ChartRenderingPipeline` - a composable stage-based render pipeline, independent of `ChartRenderingService`. |
| `Workers/` | `CacheCleanupWorker`, `ChartProcessingWorker`, `MetricsAggregatorWorker`. These are plain `IDisposable` classes with their own timers/loops, **not** `BackgroundService` implementations, and are not registered as hosted services. |
| `Streaming/`, `Events/`, `Integration/`, `CLI/` | Streaming chart updates, event publisher/dispatcher, HTTP/webhook clients, and CLI argument parsing. Compiled but not wired into the web host. |
| `Diagnostics/` | `HealthCheckService` (custom, not `Microsoft.Extensions.Diagnostics.HealthChecks`), metrics collectors, `PerformanceMonitor`. |
| `tests/`, `benchmarks/`, `Examples/` | xUnit tests, BenchmarkDotNet benchmarks, usage examples. |

## Data flow (web host)

```
HTTP request
  → ASP.NET Core routing (MapControllers)
  → Controller (ChartController / ExportController inject ChartEngine;
     DataController / TemplateController inject services directly)
  → ChartEngine facade
      → ChartDataService.Validate…        (fail fast on bad input)
      → RenderCacheService lookup          (key: chart id + last-modified)
      → ChartRenderingService              (SkiaSharp surface/canvas → encode)
      → ExportService                      (file output for export endpoints)
      → ChartRepository                    (in-memory CRUD)
  ← RenderResult / ApiResponse DTO
```

Standalone library use skips the HTTP layer: `ChartEngine.Create()` builds its own `ServiceCollection`, calls `AddSkiaSharpChartEngine()`, and returns the facade.

## Key design decisions

- **Facade over DI services.** `ChartEngine` is a thin coordinator; every capability lives in a replaceable interface (`IChartRenderingService`, `IExportService`, ...). Trade-off: the facade takes `IServiceProvider` in its constructor (service-locator style) rather than the individual services, which hides its dependencies but simplifies `ChartEngine.Create()`.
- **Singleton lifetimes everywhere.** All services in `AddSkiaSharpChartEngine` are singletons. This is correct given that the repository and render cache are process-wide in-memory stores; introducing scoped state into these services would be a bug.
- **In-memory persistence.** `ChartRepository` deliberately trades durability for zero external dependencies. Any multi-instance or restart-safe deployment needs a real `IChartRepository` implementation.
- **Sync and async API pairs.** Every facade operation exists in both flavors so the library is usable from non-async callers; the sync variants are separate code paths, not `.Result` wrappers.
- **Options captured at registration.** `AddSkiaSharpChartEngine(configureOptions)` materializes `ChartEngineOptions` once and closes over it (e.g. for `RenderCacheService` cache size). It does not use `IOptions<T>`, so options cannot be changed after registration or bound to configuration reload.

## Extension points

- **Custom storage:** implement `IChartRepository` and register it before/instead of the default.
- **Custom rendering/export:** replace `IChartRenderingService` / `IExportService` registrations.
- **Interactivity:** `Extensions/InteractivityExtensions.AddInteractivity()` registers `IInteractivityService` as an opt-in add-on.
- **Pipeline stages:** `Pipeline/ChartRenderingPipeline` supports composing custom render stages independently of the default service.
- **Health checks:** `HealthCheckService.RegisterCheck(IHealthCheck)` accepts custom checks (custom interface in `Diagnostics/HealthCheck.cs`).

## Known limitations

- **Middleware is not wired.** `Program.cs` does not call any of the `Use*` extensions in `Middleware/`; rate limiting, request validation, custom error handling, and performance monitoring are inert unless the host adds them.
- **Workers are not hosted services.** Nothing starts `CacheCleanupWorker` / `ChartProcessingWorker` / `MetricsAggregatorWorker` in the web host; they only run if constructed explicitly.
- **No persistence** - repository and caches are in-memory only.
- **No authentication.** `Program.cs` calls `UseAuthorization()` but no authentication scheme is configured; all endpoints are anonymous.
- **`docs/*.md` API pages are generated-style reference docs** and some examples construct controllers manually with mismatched constructor arguments; prefer the source for exact signatures.
