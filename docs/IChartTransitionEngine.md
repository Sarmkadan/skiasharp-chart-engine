# IChartTransitionEngine

`IChartTransitionEngine` is the public contract for rendering animated transitions between chart states. It supports rendering a complete timeline, creating and rendering a two-chart transition, rendering one interpolated PNG frame, and creating an empty timeline.

The interface is declared in the `SkiaSharpChartEngine.Animation` namespace.

## Method signatures

### Render a timeline

```csharp
Task<TransitionResult> RenderTransitionAsync(
    TransitionTimeline timeline,
    TransitionOptions? options = null,
    CancellationToken cancellationToken = default);
```

Renders the ordered keyframes in `timeline`. The timeline must contain at least two keyframes with strictly ascending time offsets. `options` controls rendering settings such as frame rate, quality, and playback behavior; passing `null` uses default transition options. The returned result contains the PNG-encoded frames in chronological order. If rendering is cancelled, the result is unsuccessful and does not contain a partial frame list.

### Render a transition between two charts

```csharp
Task<TransitionResult> RenderTransitionAsync(
    Chart from,
    Chart to,
    AnimationSettings? settings = null,
    TransitionOptions? options = null,
    CancellationToken cancellationToken = default);
```

Builds a two-keyframe timeline from `from` and `to`, then renders it. Passing `null` for `settings` uses the default animation settings: 500 milliseconds, 60 frames per second, and `EasingFunction.EaseInOutQuad`. Passing `null` for `options` applies the default render options.

### Render one frame

```csharp
Task<byte[]> RenderFrameAsync(
    Chart from,
    Chart to,
    double progress,
    TransitionEasing easing = TransitionEasing.EaseInOutCubic,
    CancellationToken cancellationToken = default);
```

Renders one interpolated frame between `from` and `to`. `progress` is the linear progress from `0` to `1` before `easing` is applied. The returned byte array contains the PNG-encoded frame. The method can throw `InvalidOperationException` if the underlying rendering service does not produce image data.

### Create a timeline

```csharp
TransitionTimeline CreateTimeline();
```

Returns a new, empty `TransitionTimeline` that can be populated with keyframes through its fluent API.

## Usage

The following method receives an implementation through the `IChartTransitionEngine` interface and renders a transition between two charts:

```csharp
using SkiaSharpChartEngine.Animation;
using SkiaSharpChartEngine.Models;

static async Task<TransitionResult> RenderChartChangeAsync(
    IChartTransitionEngine transitionEngine,
    Chart currentChart,
    Chart nextChart,
    CancellationToken cancellationToken = default)
{
    return await transitionEngine.RenderTransitionAsync(
        currentChart,
        nextChart,
        settings: null,
        options: null,
        cancellationToken: cancellationToken);
}
```

The concrete implementation supplied as `transitionEngine` performs the render; callers depend only on the public interface.
