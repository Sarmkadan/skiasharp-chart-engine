# TransitionOptions

`TransitionOptions` encapsulates configuration parameters that govern how animated transitions between chart states are executed by the rendering pipeline. It controls playback behavior, parallelism during frame generation, and diagnostic frame-timing collection, providing a single configuration point for transition rendering characteristics.

## API

### PlaybackMode Playback

Gets or sets the playback mode that determines how the transition animation progresses over time. The `PlaybackMode` enum controls whether the animation runs forward, in reverse, loops, or follows a custom progression curve.

**Type:** `PlaybackMode` (enum)

### bool EnableParallelRendering

Gets or sets a value indicating whether the transition engine may render multiple animation frames concurrently. When `true`, the engine distributes frame computation across available threads up to the limit specified by `MaxParallelism`.

**Type:** `bool`

### int MaxParallelism

Gets or sets the maximum degree of parallelism permitted when `EnableParallelRendering` is `true`. A value of `0` or less typically resets to `Environment.ProcessorCount`. Values above the available logical core count are clamped internally by the scheduler.

**Type:** `int`

### bool CollectFrameTimings

Gets or sets a value indicating whether the engine should record detailed timing metrics for each rendered transition frame. When enabled, frame durations and scheduling delays are captured for later analysis via `MetricsAggregatorWorker`.

**Type:** `bool`

### void Validate()

Validates the current option values against internal constraints. Throws an exception if any combination of settings is invalid or contradictory.

**Throws:** `ArgumentException` or `InvalidOperationException` when configuration rules are violated (e.g., `MaxParallelism` set to a negative value while `EnableParallelRendering` is `true`, or incompatible `PlaybackMode` combinations).

### TransitionOptions Clone()

Creates a deep copy of the current `TransitionOptions` instance. All properties are duplicated, ensuring the clone can be mutated independently without affecting the original.

**Returns:** `TransitionOptions` — a new instance with identical property values.

### override string ToString()

Returns a string representation of the current configuration, typically listing all property values in a human-readable format suitable for logging or debugging output.

**Returns:** `string`

## Usage

### Example 1: Configuring a diagnostic transition with timing collection

```csharp
var options = new TransitionOptions
{
    Playback = PlaybackMode.Forward,
    EnableParallelRendering = true,
    MaxParallelism = 4,
    CollectFrameTimings = true
};

// Validate before passing to the render pipeline
options.Validate();

RenderPipelineService.ConfigureTransition(options);
```

### Example 2: Cloning and overriding for a specific chart

```csharp
var baseOptions = new TransitionOptions
{
    Playback = PlaybackMode.EaseInOut,
    EnableParallelRendering = false,
    CollectFrameTimings = false
};

// Create a per-chart variant without modifying the base
var chartOptions = baseOptions.Clone();
chartOptions.Playback = PlaybackMode.Forward;
chartOptions.CollectFrameTimings = true;
chartOptions.Validate();

Console.WriteLine(chartOptions.ToString());
```

## Notes

- **Validation timing:** `Validate()` must be called explicitly before passing the options to `RenderPipelineService` or any transition worker. The engine does not implicitly validate on assignment.
- **Parallelism clamping:** Setting `MaxParallelism` to a value exceeding `Environment.ProcessorCount` may result in silent clamping. Consult `ToString()` output or post-validation state to confirm the effective value.
- **Clone independence:** `Clone()` performs a deep copy. Modifying reference-type sub-properties on the original after cloning does not affect the clone, and vice versa.
- **Thread safety:** `TransitionOptions` itself is not thread-safe. Concurrent reads and writes to properties without external synchronization may produce inconsistent state. The intended usage pattern is to configure, validate, and then pass an effectively immutable snapshot to the rendering pipeline.
- **Frame timing overhead:** Enabling `CollectFrameTimings` introduces minor per-frame allocation and timing overhead. It should be disabled in production deployments where diagnostic data is not required.
- **PlaybackMode compatibility:** Certain `PlaybackMode` values may impose constraints on other options. `Validate()` will surface these conflicts. Always call `Validate()` after mutating `Playback`.
