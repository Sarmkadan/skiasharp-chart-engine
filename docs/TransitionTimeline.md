# TransitionTimeline

`TransitionTimeline` is a fluent builder for defining sequences of state changes for animations within the SkiaSharp chart engine. It allows developers to construct complex, multi-stage transitions between visual states by chaining keyframe-based interpolation steps, facilitating the orchestration of temporal animation sequences.

## API

### AddKeyframe
`public TransitionTimeline AddKeyframe(TransitionKeyframe keyframe)`
Appends a `TransitionKeyframe` to the timeline sequence. Returns the current `TransitionTimeline` instance to support fluent chaining.

### AppendTransition
`public TransitionTimeline AppendTransition(TransitionTimeline transition)`
Appends the specified `TransitionTimeline` to the end of this instance. Returns the current `TransitionTimeline` instance.
*Throws:* `ArgumentNullException` if `transition` is null.

### StartWith
`public TransitionTimeline StartWith(TransitionKeyframe keyframe)`
Defines the initial `TransitionKeyframe` for the timeline. Returns the current `TransitionTimeline` instance.

### Repeat
`public TransitionTimeline Repeat(int count)`
Sets the number of times this timeline repeats during animation. Returns the current `TransitionTimeline` instance.

### Between (static)
`public static TransitionTimeline Between(TransitionKeyframe from, TransitionKeyframe to)`
Creates a new `TransitionTimeline` representing a direct transition between two `TransitionKeyframe` states.

### FromSteps (static)
`public static TransitionTimeline FromSteps(IEnumerable<TransitionKeyframe> steps)`
Creates a new `TransitionTimeline` initialized with a collection of `TransitionKeyframe` steps.

### GetSegments
`public IEnumerable<(TransitionKeyframe From, TransitionKeyframe To)> GetSegments()`
Enumerates all transitions in the timeline as a sequence of (From, To) pairs for evaluation during the rendering process.

### Validate
`public void Validate()`
Validates the internal integrity of the timeline, ensuring keyframes and segments are correctly structured.
*Throws:* `InvalidOperationException` if the timeline is in an invalid state.

### ToString
`public override string ToString()`
Returns a string representation of the timeline, summarizing its structure and segment count.

## Usage

### Simple Transition
```csharp
var start = new TransitionKeyframe(0.0f);
var end = new TransitionKeyframe(1.0f);

// Create a direct A to B timeline
var timeline = TransitionTimeline.Between(start, end);
```

### Complex Sequential Transition
```csharp
var timeline = new TransitionTimeline()
    .StartWith(new TransitionKeyframe(0.0f))
    .AddKeyframe(new TransitionKeyframe(0.5f))
    .AddKeyframe(new TransitionKeyframe(1.0f))
    .Repeat(2);

// Ensure the timeline is valid before use
timeline.Validate();
```

## Notes

*   **Thread Safety:** `TransitionTimeline` is not thread-safe. It is designed for builder-style configuration on a single thread. Modifications to the timeline after it has been provided to the rendering pipeline may result in unpredictable animation behavior.
*   **Validation:** Calling `Validate()` is recommended before passing the timeline to the engine. An empty or improperly structured timeline will throw an `InvalidOperationException` upon validation if the required interpolation steps are missing or misconfigured.
*   **Keyframe Data:** Ensure all `TransitionKeyframe` objects used within the timeline define appropriate duration and interpolation properties to avoid unexpected rendering results.
