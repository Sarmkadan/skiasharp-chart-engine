# Transition models

The types in `Animation/TransitionModels.cs` describe easing, timeline keyframes, rendered PNG frames, and the result of rendering a complete chart transition. They are in the `SkiaSharpChartEngine.Animation` namespace.

## `TransitionEasing`

`TransitionEasing` selects the curve used to turn linear progress into eased progress.

| Value | Numeric value | Purpose |
| --- | ---: | --- |
| `Linear` | 0 | Maintains constant velocity. |
| `EaseInQuad` | 1 | Applies gradual quadratic acceleration. |
| `EaseOutQuad` | 2 | Applies gradual quadratic deceleration. |
| `EaseInOutQuad` | 3 | Accelerates and then decelerates quadratically. |
| `EaseInCubic` | 4 | Applies strong cubic acceleration. |
| `EaseOutCubic` | 5 | Applies strong cubic deceleration. |
| `EaseInOutCubic` | 6 | Accelerates and then decelerates cubically. |
| `EaseInExpo` | 7 | Applies exponential acceleration. |
| `EaseOutExpo` | 8 | Applies exponential deceleration. |
| `EaseInOutExpo` | 9 | Applies exponential acceleration and deceleration. |
| `EaseInBack` | 10 | Overshoots slightly while easing in. |
| `EaseOutBack` | 11 | Reaches the target, overshoots slightly, and settles. |
| `EaseInOutBack` | 12 | Applies overshoot at both ends. |
| `EaseInElastic` | 13 | Applies an inward elastic spring effect. |
| `EaseOutElastic` | 14 | Applies an outward elastic spring effect. |
| `EaseInOutElastic` | 15 | Applies an elastic spring effect at both ends. |
| `EaseOutBounce` | 16 | Simulates a ball bouncing to rest. |
| `EaseInBounce` | 17 | Simulates a ball bouncing from rest. |
| `EaseInOutBounce` | 18 | Applies bounce at both ends. |
| `Spring` | 19 | Applies a damped spring with stiffness 200, damping 20, and mass 1. |

## `TransitionEasingCalculator`

This static class evaluates a `TransitionEasing` value for normalized linear progress. Its `Calculate(TransitionEasing easing, double t)` method clamps `t` to `[0, 1]` before evaluating the curve. The `Spring` curve intentionally can transiently return a value outside `[0, 1]`. An unrecognized enum value falls back to the clamped linear value.

```csharp
double eased = TransitionEasingCalculator.Calculate(
    TransitionEasing.EaseInOutCubic,
    0.5);
```

## `TransitionKeyframe`

`TransitionKeyframe` is an immutable record representing a chart snapshot at an absolute point on an animation timeline.

| Property | Type | Purpose |
| --- | --- | --- |
| `Chart` | `Chart` | The chart state captured by the keyframe. |
| `TimeMs` | `double` | The non-negative absolute offset, in milliseconds, from the start of the timeline. Keyframes are ordered by this value. |
| `Easing` | `TransitionEasing` | The easing used from this keyframe to the next. It defaults to `EaseInOutCubic`; the final keyframe's easing is unused. |

```csharp
var keyframe = new TransitionKeyframe(
    chart,
    timeMs: 500,
    easing: TransitionEasing.EaseOutQuad);
```

## `TransitionRenderFrame`

`TransitionRenderFrame` is an immutable record containing one PNG-encoded frame produced during transition rendering.

| Property | Type | Purpose |
| --- | --- | --- |
| `FrameIndex` | `int` | The zero-based position in the complete frame sequence. |
| `TimeMs` | `double` | The frame timestamp in milliseconds from the start of the animation. |
| `Progress` | `double` | Overall normalized animation progress in `[0, 1]`. |
| `ImageData` | `byte[]` | The raw PNG bytes returned by the rendering service. |

```csharp
var frame = new TransitionRenderFrame(
    FrameIndex: 0,
    TimeMs: 0,
    Progress: 0,
    ImageData: pngBytes);
```

## `TransitionResult`

`TransitionResult` represents the outcome of rendering a complete transition sequence. Its properties use `init` accessors.

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `ChartId` | `string` | Required | Identifies the chart whose transition was rendered. |
| `Success` | `bool` | `false` | Indicates whether every frame rendered without error. |
| `Frames` | `IReadOnlyList<TransitionRenderFrame>` | Empty array | Contains rendered frames in chronological order. |
| `TotalDurationMs` | `double` | `0` | Gives the full timeline duration in milliseconds. |
| `RenderTimeMs` | `long` | `0` | Gives the wall-clock rendering time in milliseconds. |
| `ErrorMessage` | `string?` | `null` | Describes the error when `Success` is `false`. |
| `Exception` | `Exception?` | `null` | Contains the exception that caused failure, if one exists. |
| `Metadata` | `IReadOnlyDictionary<string, object>` | Empty dictionary | Contains arbitrary metadata produced during rendering. |

`CreateSuccess` requires a non-empty `chartId` and a non-null frame list, sets `Success` to `true`, and uses an empty dictionary when metadata is omitted. `CreateFailure` requires a non-empty chart ID and error message, sets `Success` to `false`, and optionally records an exception. `ToString()` reports the chart ID, success state, frame count, and duration.

```csharp
var frames = new[]
{
    new TransitionRenderFrame(0, 0, 0, firstPngBytes),
    new TransitionRenderFrame(1, 500, 1, lastPngBytes)
};

TransitionResult result = TransitionResult.CreateSuccess(
    chartId: "sales-chart",
    frames: frames,
    totalDurationMs: 500,
    renderTimeMs: 42,
    metadata: new Dictionary<string, object>
    {
        ["renderer"] = "default"
    });
```
