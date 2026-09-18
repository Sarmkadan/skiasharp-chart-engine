# CLAUDE.md

SkiaSharp-based .NET 10 chart rendering library (line/bar/pie/heatmap/etc., PNG/SVG export) with an ASP.NET Core Web API host in the same project. Root namespace `SkiaSharpChartEngine`, assembly `SkiaSharpChartEngine`.

## Build

```bash
dotnet restore
dotnet build skiasharp-chart-engine.csproj -c Release   # or: make build
dotnet run --project skiasharp-chart-engine.csproj      # starts Web API (Swagger in Development)
make pack                                               # NuGet package -> ./nupkg
make docker-build / docker-run                          # port 5000
```

SDK pinned by `global.json` (10.0.100, rollForward latestMinor). Solution: `skiasharp-chart-engine.sln` (main project + tests + benchmarks).

## Test

```bash
dotnet test tests/skiasharp-chart-engine.Tests/ -c Release
make test        # note: runs `dotnet test` against the main csproj, prefer the explicit path above
make ci          # clean + restore + build + test + lint + analyze
```

xUnit + FluentAssertions 7 + Moq. Test files live in `tests/skiasharp-chart-engine.Tests/`, one `<Class>Tests.cs` per unit, optionally split into `<Class>TestsExtensions.cs` / `<Class>TestsValidation.cs`.

## Lint / Format

```bash
make format      # dotnet format
make lint        # dotnet format --verify-no-changes
make analyze     # build with TreatWarningsAsErrors=true
```

Style is enforced by `.editorconfig` (4 spaces, Allman braces, `csharp_new_line_before_open_brace = all`). `Nullable` and `ImplicitUsings` enabled via `Directory.Build.props`; `GlobalUsings.cs` adds `Constants` and `Exceptions` namespaces globally.

## Key directories and entry points

- `Program.cs` - Web API host (`AddSkiaSharpChartEngine()`, controllers, Swagger, HealthCheckService).
- `ChartEngine.cs` - library facade; `ChartEngine.Create()` builds a standalone DI container. `ChartEngineExtensions.cs` / `ChartEngineJsonExtensions.cs` add convenience overloads.
- `Configuration/ServiceCollectionExtensions.cs` - DI registration; `ChartEngineOptions` holds cache/concurrency settings.
- `Services/` - `I*Service` interfaces + implementations (rendering, data, export, cache, configuration, interaction).
- `Models/` - `Chart`, `ChartSeries`, `DataPoint`, `RenderResult`, `ExportOptions`, etc.
- `Rendering/` - one `*ChartRenderer.cs` per chart type implementing `IChartRenderer`.
- `API/Controllers`, `API/Requests`, `API/Responses` - Web API layer.
- `Caching/`, `Pipeline/`, `Streaming/`, `Reports/`, `Events/`, `Animation/`, `Repository/`, `Diagnostics/`, `Middleware/`, `Workers/`, `CLI/`, `Utilities/`, `Extensions/`, `Validation/`, `Formatters/`, `Serializers/`.
- `tests/`, `benchmarks/`, `examples/` - separate projects; `docs/` - per-class markdown docs.

Important: `skiasharp-chart-engine.csproj` has a large `<Compile Remove>` list. `API/`, `CLI/`, `Middleware/`, `Workers/`, `Formatters/`, `Validation/`, `Rendering/`, `Integration/`, `Serializers/` and several individual files are EXCLUDED from the build. Check the csproj before assuming a file compiles or before adding code to those folders.

## Conventions

- File header banner (`// Author: Vladyslav Zaiets | https://sarmkadan.com`) on every `.cs` file.
- File-scoped namespaces matching folder: `SkiaSharpChartEngine.<Folder>`.
- One public type per file; interfaces prefixed `I` and kept in their own files next to the implementation.
- Companion partial-style files: `<Class>Extensions.cs`, `<Class>JsonExtensions.cs`, `<Class>Validation.cs` hold extension methods / JSON helpers / validators for `<Class>`.
- XML doc comments on all public APIs; constructor argument null-checks via `?? throw new ArgumentNullException(nameof(x))`.
- Services are resolved via `Microsoft.Extensions.DependencyInjection`; logging via `ILogger<T>`.
- Async methods end in `Async` and accept `CancellationToken`; sync wrappers often provided alongside.
- Keep methods under ~50 lines (CONTRIBUTING.md).
- Commit history uses conventional prefixes (`chore:`, `docs:`, `feat:`, `fix:`).
