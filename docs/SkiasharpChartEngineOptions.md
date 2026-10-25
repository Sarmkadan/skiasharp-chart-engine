# SkiasharpChartEngineOptions

`SkiasharpChartEngineOptions` (namespace `SkiaSharpChartEngine.Configuration`, file `Configuration/SkiasharpChartEngineOptions.cs`) is the base options class for the chart engine. It holds ten settings for caching, rendering concurrency, default chart appearance, and data limits.

It is not the type you configure at runtime. `ChartEngineOptions` derives from it, and `AddSkiaSharpChartEngine(...)` passes a `ChartEngineOptions` to its callback. `ChartEngineOptions` redeclares every property below with its own type and default, so the base-class values are never read by the engine. See [Mapping to ChartEngineOptions](#mapping-to-chartengineoptions).

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `CacheEnabled` | `string` | `"true"` | Whether chart caching is enabled. Typed as `string`, unlike the `bool` on `ChartEngineOptions`. |
| `CacheDurationSeconds` | `int` | `3600` | How long, in seconds, cached chart images are retained. |
| `MaxConcurrentRenders` | `int` | `10` | Maximum number of chart render operations that may run at the same time. |
| `DefaultChartWidth` | `int` | `800` | Default chart width in pixels. |
| `DefaultChartHeight` | `int` | `600` | Default chart height in pixels. |
| `DefaultBackgroundColor` | `string` | `"#FFFFFF"` | Default chart background as a hex color string. |
| `UseAntiAliasing` | `bool` | `true` | Whether anti-aliasing is applied when rendering. |
| `MaxDataPointsPerSeries` | `int` | `1000` | Maximum data points allowed in one series. |
| `MaxSeriesPerChart` | `int` | `10` | Maximum number of series allowed in one chart. |
| `ValidateDataOnLoad` | `bool` | `true` | Whether incoming chart data is validated when it is loaded. |

Each property is annotated with `[Required]` from `System.ComponentModel.DataAnnotations`. The attribute has no effect unless something runs DataAnnotations validation on the instance. Nothing in this repository does.

`ToString()` returns a summary of `CacheEnabled`, `CacheDurationSeconds`, `MaxConcurrentRenders`, `DefaultChartWidth`, `DefaultChartHeight`, and `DefaultBackgroundColor`.

The class has no `Validate()` method. Validation exists only on `ChartEngineOptions`.

## Mapping to ChartEngineOptions

`ChartEngineOptions : SkiasharpChartEngineOptions`. The derived class redeclares the base properties with `new`-less declarations, so they hide the base members. The base members still exist, but a `ChartEngineOptions` instance reads and writes its own storage.

| Base property (`SkiasharpChartEngineOptions`) | Matching member on `ChartEngineOptions` | Difference |
|---|---|---|
| `CacheEnabled` (`string`, `"true"`) | `CacheEnabled` (`bool`) | Alias for `EnableCaching`. Type changes from `string` to `bool`. |
| `CacheDurationSeconds` (`int`, `3600`) | `CacheDurationSeconds` (`int`) | Backed by `CacheExpirationTime` (`TimeSpan`, default 1 hour). |
| `MaxConcurrentRenders` (`int`, `10`) | `MaxConcurrentRenders` (`int`) | Default is `Environment.ProcessorCount`. |
| `DefaultChartWidth` (`int`, `800`) | `DefaultChartWidth` (`int`) | Same default (`ChartConstants.DefaultChartWidth`). |
| `DefaultChartHeight` (`int`, `600`) | `DefaultChartHeight` (`int`) | Same default (`ChartConstants.DefaultChartHeight`). |
| `DefaultBackgroundColor` (`string`, `"#FFFFFF"`) | `DefaultBackgroundColor` (`string`) | Same default (`ChartConstants.DefaultBackgroundColor`). |
| `UseAntiAliasing` (`bool`, `true`) | `UseAntiAliasing` (`bool`) | Same default. |
| `MaxDataPointsPerSeries` (`int`, `1000`) | `MaxDataPointsPerSeries` (`int`) | Default is `ChartConstants.MaxDataPoints` (`100000`). |
| `MaxSeriesPerChart` (`int`, `10`) | `MaxSeriesPerChart` (`int`) | Default is `ChartConstants.MaxSeries` (`50`). |
| `ValidateDataOnLoad` (`bool`, `true`) | `ValidateDataOnLoad` (`bool`) | Same default. |

`ChartEngineOptions` adds members that do not exist on the base class:

- `CacheSize` (`int`, `ChartConstants.CacheSize` = 100): size of the render cache. `AddSkiaSharpChartEngine` uses it to construct `RenderCacheService`.
- `EnableCaching` (`bool`, `true`): the value that `CacheEnabled` aliases.
- `CacheExpirationTime` (`TimeSpan`, 1 hour): the value that `CacheDurationSeconds` aliases.
- `EnableLogging` (`bool`, `true`).
- `CustomSettings` (`Dictionary<string, object>?`, `null`).
- `Validate()`: throws `InvalidOperationException` if `CacheSize < 1`, `DefaultChartWidth < 200`, `DefaultChartHeight < 150`, `MaxDataPointsPerSeries < 10`, or `MaxSeriesPerChart < 1`.

### Practical implications

- Configure through `AddSkiaSharpChartEngine(options => ...)`, which takes `Action<ChartEngineOptions>`. This is how `Program.cs` sets `CacheEnabled`, `CacheDurationSeconds`, and `MaxConcurrentRenders`.
- Do not use `services.Configure<SkiasharpChartEngineOptions>(...)`. Nothing in the container reads that type, so the values have no effect.
- If you use `SkiasharpChartEngineOptions` directly, for example as a plain DTO, you must map its values to `ChartEngineOptions` yourself. The mapping above lists the equivalent members.
- `CacheEnabled` changes type between the two classes. On the base class it is a `string`. On `ChartEngineOptions` it is a `bool`.
- Defaults differ for `MaxConcurrentRenders`, `MaxDataPointsPerSeries`, and `MaxSeriesPerChart`. Use the `ChartEngineOptions` values when reasoning about runtime behavior.

## Usage

### Configuring the engine (recommended)

```csharp
using SkiaSharpChartEngine.Configuration;

builder.Services.AddSkiaSharpChartEngine(options =>
{
    options.CacheEnabled = true;                 // bool on ChartEngineOptions
    options.CacheDurationSeconds = 300;
    options.MaxConcurrentRenders = 4;
    options.DefaultChartWidth = 800;
    options.DefaultChartHeight = 600;
    options.DefaultBackgroundColor = "#FFFFFF";
});
```

### Standalone instance (base class)

```csharp
using SkiaSharpChartEngine.Configuration;

var baseOptions = new SkiasharpChartEngineOptions
{
    CacheEnabled = "true",                       // string on the base class
    CacheDurationSeconds = 300,
    MaxConcurrentRenders = 4,
    DefaultChartWidth = 800,
    DefaultChartHeight = 600,
    DefaultBackgroundColor = "#FFFFFF",
    UseAntiAliasing = true,
    MaxDataPointsPerSeries = 1000,
    MaxSeriesPerChart = 10,
    ValidateDataOnLoad = true
};

// Map to the type the engine consumes.
var options = new ChartEngineOptions
{
    CacheEnabled = bool.Parse(baseOptions.CacheEnabled),
    CacheDurationSeconds = baseOptions.CacheDurationSeconds,
    MaxConcurrentRenders = baseOptions.MaxConcurrentRenders,
    DefaultChartWidth = baseOptions.DefaultChartWidth,
    DefaultChartHeight = baseOptions.DefaultChartHeight,
    DefaultBackgroundColor = baseOptions.DefaultBackgroundColor,
    UseAntiAliasing = baseOptions.UseAntiAliasing,
    MaxDataPointsPerSeries = baseOptions.MaxDataPointsPerSeries,
    MaxSeriesPerChart = baseOptions.MaxSeriesPerChart,
    ValidateDataOnLoad = baseOptions.ValidateDataOnLoad
};

options.Validate();
```

## Notes

- **Thread safety:** The class is a plain configuration object with no internal synchronization. Reading it from several threads is safe as long as nothing writes to it. Change values before the engine starts, not during rendering.
- **Positive dimensions:** `DefaultChartWidth` and `DefaultChartHeight` should be positive. `ChartEngineOptions.Validate()` enforces minimums of 200 and 150 pixels. The base class does not validate anything.
- **Data limits:** `MaxDataPointsPerSeries` and `MaxSeriesPerChart` are declared here and on `ChartEngineOptions`. Only `ChartEngineOptions.Validate()` checks their values. Confirm how the rendering and data services apply them before relying on them to truncate or reject data.
- **Color format:** `DefaultBackgroundColor` should be a hex color string such as `"#FFFFFF"`. No validation happens on the base class.
