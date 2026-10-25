# TransitionServiceExtensions

`TransitionServiceExtensions` is a static class in the `SkiaSharpChartEngine.Animation` namespace (`Animation/TransitionServiceExtensions.cs`). It wires the v2 animated-transitions subsystem into an `IServiceCollection` and adds helper methods on `Chart` and `AnimationSettings` that create `TransitionTimeline` objects and `TransitionOptions` values.

The class has two groups of members:

- **DI registration**: `AddChartTransitions` and `AddSkiaSharpChartEngineWithTransitions`.
- **Factory helpers**: `BeginTransition`, `TransitionTo`, `ToAnimatedTimeline` (on `Chart`) and `ToTransitionOptions` (on `AnimationSettings`).

## Registration

### `AddChartTransitions`

```csharp
public static IServiceCollection AddChartTransitions(
    this IServiceCollection services,
    Action<TransitionOptions>? configureOptions = null)
```

Registers the transition subsystem:

1. Creates a new `TransitionOptions` with its default values and passes it to `configureOptions` (if supplied).
2. Registers that instance as a singleton `TransitionOptions`.
3. Registers `ChartTransitionEngine` as a singleton `IChartTransitionEngine`.

**Parameters**
- `services` (`IServiceCollection`, `this`): The service collection to configure.
- `configureOptions` (`Action<TransitionOptions>?`, optional): Customises the default options before they are registered. Default: `null`.

**Return Value**
- `IServiceCollection`: The same `services` instance, for chaining.

**Exceptions**
- `ArgumentNullException`: Thrown when `services` is `null`.

**Notes**
- The method does not check whether the core engine is registered. `ChartTransitionEngine` depends on `IChartRenderingService` and `ILogger<ChartTransitionEngine>`, so resolving `IChartTransitionEngine` fails unless `AddSkiaSharpChartEngine` has been called on the same container.
- Each call appends new registrations. MS.DI resolves the last registration for a single-service request, so calling the method twice does not replace the first call's registrations.

---

### `AddSkiaSharpChartEngineWithTransitions`

```csharp
public static IServiceCollection AddSkiaSharpChartEngineWithTransitions(
    this IServiceCollection services,
    Action<ChartEngineOptions>? configureEngine = null,
    Action<TransitionOptions>? configureTransitions = null)
```

Calls `AddSkiaSharpChartEngine(configureEngine)` and then `AddChartTransitions(configureTransitions)`, in that order. It is a convenience wrapper for the two calls.

**Parameters**
- `services` (`IServiceCollection`, `this`): The service collection to configure.
- `configureEngine` (`Action<ChartEngineOptions>?`, optional): Customises `ChartEngineOptions`. Passed through unchanged. Default: `null`.
- `configureTransitions` (`Action<TransitionOptions>?`, optional): Customises the default `TransitionOptions`. Passed through unchanged. Default: `null`.

**Return Value**
- `IServiceCollection`: The same `services` instance, for chaining.

**Exceptions**
- `ArgumentNullException`: Thrown when `services` is `null`.

---

## Chart Helpers

### `BeginTransition`

```csharp
public static TransitionTimeline BeginTransition(this Chart chart)
```

Creates a timeline that starts with `chart` as its keyframe at time 0. It is equivalent to `new TransitionTimeline().AddKeyframe(chart, 0)`, so the keyframe uses the default easing of `AddKeyframe`, `TransitionEasing.EaseInOutCubic`.

**Parameters**
- `chart` (`Chart`, `this`): The opening chart state.

**Return Value**
- `TransitionTimeline`: A new timeline containing one keyframe. More keyframes can be added to it.

**Exceptions**
- `ArgumentNullException`: Thrown when `chart` is `null`.

---

### `TransitionTo`

```csharp
public static TransitionTimeline TransitionTo(
    this Chart from,
    Chart to,
    double durationMs,
    TransitionEasing easing = TransitionEasing.EaseInOutCubic)
```

Creates a two-keyframe timeline from `from` to `to`. It validates its arguments and then calls `TransitionTimeline.Between(from, to, durationMs, easing)`.

**Parameters**
- `from` (`Chart`, `this`): The starting chart state. Keyframe at time 0.
- `to` (`Chart`): The ending chart state. Keyframe at `durationMs`.
- `durationMs` (`double`): Length of the transition in milliseconds. Must be greater than 0.
- `easing` (`TransitionEasing`, optional): Easing for the segment. Default: `EaseInOutCubic`.

**Return Value**
- `TransitionTimeline`: A new two-keyframe timeline.

**Exceptions**
- `ArgumentNullException`: Thrown when `from` or `to` is `null`.
- `ArgumentOutOfRangeException`: Thrown when `durationMs` is less than or equal to 0.

---

### `ToAnimatedTimeline`

```csharp
public static TransitionTimeline ToAnimatedTimeline(
    this Chart from,
    Chart to,
    TransitionEasing easing = TransitionEasing.EaseInOutCubic)
```

Creates a two-keyframe timeline like `TransitionTo`, but takes the duration from the starting chart's configuration:

- If `from.Configuration.AnimationDurationMs` is greater than 0, that value is used.
- Otherwise the duration is `500.0` ms.

The method calls `TransitionTimeline.Between` directly, so it does not go through `TransitionTo`.

**Parameters**
- `from` (`Chart`, `this`): The starting chart. Its `Configuration.AnimationDurationMs` sets the segment duration.
- `to` (`Chart`): The ending chart state.
- `easing` (`TransitionEasing`, optional): Easing for the segment. Default: `EaseInOutCubic`.

**Return Value**
- `TransitionTimeline`: A new two-keyframe timeline.

**Exceptions**
- `ArgumentNullException`: Thrown when `from` or `to` is `null`.

---

## Settings Bridge

### `ToTransitionOptions`

```csharp
public static TransitionOptions ToTransitionOptions(this AnimationSettings settings)
```

Creates a `TransitionOptions` from the v1 `AnimationSettings`. Only the frame rate is copied:

- `FrameRate` is set to `settings.FrameRate` clamped to the range 1–120. `TransitionOptions.FrameRate` throws outside that range, so the clamp prevents an exception for values set through a property or object initializer. `AnimationSettings`' constructor validates the frame rate, but its property setter does not.
- All other `TransitionOptions` properties keep their defaults (for example `Quality` 95 and `LoopCount` 1). `DurationMs`, `EasingType`, `Enabled`, and the opacity settings are not used.

**Parameters**
- `settings` (`AnimationSettings`, `this`): The source settings.

**Return Value**
- `TransitionOptions`: A new instance. It is not registered in the DI container.

**Exceptions**
- `ArgumentNullException`: Thrown when `settings` is `null`.

---

## Usage

### Registering the services

```csharp
using Microsoft.Extensions.DependencyInjection;
using SkiaSharpChartEngine.Animation;

var services = new ServiceCollection();

// One call: engine + transitions
services.AddSkiaSharpChartEngineWithTransitions(
    configureEngine: null,
    configureTransitions: options => options.Quality = 90);

// Or, when the engine is already registered:
// services.AddChartTransitions();
```

### Building a two-state transition

```csharp
using SkiaSharpChartEngine.Animation;
using SkiaSharpChartEngine.Models;

Chart before = BuildChart(dataA);   // application code
Chart after = BuildChart(dataB);    // application code

// 600 ms, elastic-style easing from the TransitionEasing enum
TransitionTimeline timeline = before.TransitionTo(after, 600, TransitionEasing.EaseInOutElastic);

// Duration taken from before.Configuration.AnimationDurationMs (500 ms if it is 0 or less)
TransitionTimeline auto = before.ToAnimatedTimeline(after);
```

### Converting v1 settings

```csharp
using SkiaSharpChartEngine.Animation;
using SkiaSharpChartEngine.Models;

var settings = new AnimationSettings(750, 30);
TransitionOptions options = settings.ToTransitionOptions();   // FrameRate = 30, other options at defaults
```

## Notes

- **Ordering**: `AddChartTransitions` does not check for the core engine. Register the core engine first, or use `AddSkiaSharpChartEngineWithTransitions`.
- **Singleton options**: The `TransitionOptions` instance registered by `AddChartTransitions` is a singleton. Changes made to it at run time affect every consumer that resolves it. `ToTransitionOptions` returns a fresh instance each time.
- **Duration rules differ**: `TransitionTo` requires a positive `durationMs`. `ToAnimatedTimeline` never throws for the duration; it falls back to 500 ms.
- **Chaining**: `BeginTransition` returns a `TransitionTimeline`, so further segments are added with `TransitionTimeline.AppendTransition(Chart toChart, double durationMs, TransitionEasing easing = EaseInOutCubic)`. The XML `<example>` on `BeginTransition` uses that method.
