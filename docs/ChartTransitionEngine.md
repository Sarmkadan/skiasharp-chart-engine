# ChartTransitionEngine

The `ChartTransitionEngine` serves as the primary orchestration component for generating visual transitions between chart states within the `skiasharp-chart-engine` library. It manages the animation lifecycle by constructing temporal timelines and executing the rendering of transition sequences or individual animation frames, utilizing SkiaSharp to output serialized graphic data.

## API

### Constructor
*   `public ChartTransitionEngine()`
    Initializes a new instance of the `ChartTransitionEngine`.

### Methods

*   `public TransitionTimeline CreateTimeline(TransitionOptions options)`
    Creates a new `TransitionTimeline` object based on the provided configuration options, defining the easing, duration, and interpolation settings for the subsequent transition.
    *   **Parameters:** `TransitionOptions options` - Configuration settings for the transition.
    *   **Returns:** `TransitionTimeline` - The configured timeline instance.

*   `public async Task<TransitionResult> RenderTransitionAsync(TransitionTimeline timeline, CancellationToken ct = default)`
    Renders an entire transition sequence based on a pre-configured `TransitionTimeline`.
    *   **Parameters:** `TransitionTimeline timeline` - The timeline to render; `CancellationToken ct` - Token to monitor for cancellation.
    *   **Returns:** `Task<TransitionResult>` - A result containing the rendered transition output.
    *   **Throws:** `OperationCanceledException` if the operation is cancelled.

*   `public async Task<TransitionResult> RenderTransitionAsync(ChartState startState, ChartState endState, TransitionOptions options, CancellationToken ct = default)`
    Renders a transition between two defined chart states using the provided configuration options. This method internally constructs a timeline.
    *   **Parameters:** `ChartState startState` - The initial state; `ChartState endState` - The target state; `TransitionOptions options` - The transition configuration; `CancellationToken ct` - Token to monitor for cancellation.
    *   **Returns:** `Task<TransitionResult>` - A result containing the rendered transition output.
    *   **Throws:** `OperationCanceledException` if the operation is cancelled.

*   `public async Task<byte[]> RenderFrameAsync(TransitionTimeline timeline, double progress)`
    Renders a single frame at a specific point in time along the provided `TransitionTimeline`.
    *   **Parameters:** `TransitionTimeline timeline` - The timeline containing transition parameters; `double progress` - The progress value (normalized between 0.0 and 1.0).
    *   **Returns:** `Task<byte[]>` - The raw bytes of the rendered image frame.
    *   **Throws:** `ArgumentOutOfRangeException` if `progress` is outside the range [0.0, 1.0].

## Usage

### Example 1: Rendering a Complete Transition
This example demonstrates a direct transition between two states using the engine's configuration-driven rendering.

```csharp
var engine = new ChartTransitionEngine();
var options = new TransitionOptions { Duration = TimeSpan.FromMilliseconds(500) };

// Perform a transition from stateA to stateB
var result = await engine.RenderTransitionAsync(stateA, stateB, options);

if (result.IsSuccess)
{
    // Handle the rendered transition output
    byte[] data = result.Payload;
}
```

### Example 2: Frame-by-Frame Rendering
This example shows how to manually control the rendering process for fine-grained animation control.

```csharp
var engine = new ChartTransitionEngine();
var timeline = engine.CreateTimeline(new TransitionOptions { Duration = TimeSpan.FromSeconds(1) });

// Render frames at 0%, 50%, and 100% progress
double[] progressSteps = { 0.0, 0.5, 1.0 };

foreach (var progress in progressSteps)
{
    byte[] frame = await engine.RenderFrameAsync(timeline, progress);
    // Save or display the frame bytes
}
```

## Notes

*   **Thread Safety:** While `ChartTransitionEngine` is generally safe to instantiate and configure across threads, the `Render` methods rely on SkiaSharp's rendering backend. Concurrent calls to render methods may lead to contention depending on the underlying SkiaSharp context implementation; ensure resource-intensive rendering operations are appropriately managed in high-throughput environments.
*   **Performance:** `RenderTransitionAsync` is computationally intensive and should be executed asynchronously to avoid blocking the caller, particularly when processing complex chart state transitions.
*   **Cancellation:** Always pass a valid `CancellationToken` to the asynchronous rendering methods, especially in UI scenarios where the user might navigate away before the transition completes.
*   **Progress Validation:** The `progress` parameter in `RenderFrameAsync` must be strictly within the range [0.0, 1.0]. Values outside this range will trigger an `ArgumentOutOfRangeException`.
