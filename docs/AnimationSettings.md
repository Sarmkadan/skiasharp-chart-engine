# AnimationSettings

The `AnimationSettings` class encapsulates the configuration parameters required to control animation behavior within the SkiaSharp Chart Engine. It defines the timing, interpolation method, opacity transitions, and trigger conditions for chart animations, providing utility methods to calculate frame counts, progress values, and interpolated results based on the specified easing function.

## API

### Properties

#### `Enabled`
Gets or sets a boolean value indicating whether animation is active. When `false`, charts render immediately without interpolation.

#### `DurationMs`
Gets or sets the total duration of the animation in milliseconds. This value determines the time span over which the `EasingType` is applied.

#### `FrameRate`
Gets or sets the target frames per second (FPS) for the animation loop. This value is used to calculate the total number of frames required for the duration.

#### `EasingType`
Gets or sets the `EasingFunction` delegate or enum value that defines the mathematical interpolation curve (e.g., linear, quadratic, cubic) used to calculate progress between start and end states.

#### `AnimateOnLoad`
Gets or sets a boolean value indicating whether the animation should trigger when the chart is first rendered or loaded.

#### `AnimateOnUpdate`
Gets or sets a boolean value indicating whether the animation should trigger when the chart data or configuration is updated.

#### `StartOpacity`
Gets or sets the initial opacity value (0.0 to 1.0) at the beginning of the animation sequence.

#### `EndOpacity`
Gets or sets the final opacity value (0.0 to 1.0) at the conclusion of the animation sequence.

### Constructors

#### `AnimationSettings()`
Initializes a new instance of the `AnimationSettings` class with default values. Typically sets `Enabled` to true, a default duration (e.g., 300ms), and a standard easing function.

#### `AnimationSettings(AnimationSettings source)`
Initializes a new instance of the `AnimationSettings` class by copying values from an existing instance. This acts as a copy constructor.

### Methods

#### `Validate()`
Validates the current configuration of the settings.
*   **Parameters**: None.
*   **Return Value**: `void`.
*   **Exceptions**: Throws an `InvalidOperationException` if `DurationMs` is less than or equal to zero, `FrameRate` is invalid, or opacity values are outside the range [0.0, 1.0].

#### `GetTotalFrames()`
Calculates the total number of frames required for the animation based on `DurationMs` and `FrameRate`.
*   **Parameters**: None.
*   **Return Value**: `int` representing the frame count.
*   **Exceptions**: May throw if the instance has not been validated and contains invalid duration or framerate values.

#### `GetProgress(int currentFrame)`
Calculates the normalized progress (0.0 to 1.0) for a specific frame index, applying the configured `EasingType`.
*   **Parameters**: 
    *   `currentFrame` (`int`): The current frame index in the animation sequence.
*   **Return Value**: `double` representing the eased progress value.
*   **Exceptions**: Throws `ArgumentOutOfRangeException` if `currentFrame` is negative or exceeds the total frame count.

#### `Clone()`
Creates a deep copy of the current `AnimationSettings` instance.
*   **Parameters**: None.
*   **Return Value**: A new `AnimationSettings` object with identical property values.
*   **Exceptions**: None.

#### `ToString()`
Returns a string representation of the animation settings, typically including the duration, frame rate, and enabled status.
*   **Parameters**: None.
*   **Return Value**: `string`.
*   **Exceptions**: None.

#### `Calculate(double startValue, double endValue, double progress)`
A static helper method to compute an interpolated value between a start and end point based on a given progress ratio.
*   **Parameters**:
    *   `startValue` (`double`): The starting numerical value.
    *   `endValue` (`double`): The ending numerical value.
    *   `progress` (`double`): The normalized progress (0.0 to 1.0), usually the output of `GetProgress`.
*   **Return Value**: `double` representing the interpolated value.
*   **Exceptions**: None; expects `progress` to be within [0.0, 1.0] for meaningful results, but does not strictly enforce bounds.

## Usage

### Example 1: Configuring a Fade-In Animation on Load
This example demonstrates how to configure settings to fade a chart in over 500 milliseconds using a quadratic ease-out function, triggered only on the initial load.

```csharp
using SkiaSharpChartEngine;

var settings = new AnimationSettings
{
    Enabled = true,
    DurationMs = 500,
    FrameRate = 60,
    EasingType = EasingFunction.QuadraticEaseOut,
    AnimateOnLoad = true,
    AnimateOnUpdate = false,
    StartOpacity = 0.0,
    EndOpacity = 1.0
};

settings.Validate();

int totalFrames = settings.GetTotalFrames();
double progressAtHalfway = settings.GetProgress(totalFrames / 2);

Console.WriteLine($"Animation configured for {totalFrames} frames.");
Console.WriteLine($"Progress at midpoint: {progressAtHalfway:F4}");
```

### Example 2: Cloning and Modifying for Data Updates
This example shows how to derive a new configuration from an existing one for update events, altering the duration and easing type while preserving other constraints.

```csharp
using SkiaSharpChartEngine;

// Base configuration
var baseSettings = new AnimationSettings
{
    DurationMs = 300,
    FrameRate = 30,
    EasingType = EasingFunction.Linear,
    StartOpacity = 0.5,
    EndOpacity = 1.0
};

// Create a specific configuration for updates
var updateSettings = baseSettings.Clone();
updateSettings.AnimateOnLoad = false;
updateSettings.AnimateOnUpdate = true;
updateSettings.DurationMs = 800;
updateSettings.EasingType = EasingFunction.CubicEaseInOut;

updateSettings.Validate();

// Calculate an interpolated opacity value manually
double currentProgress = 0.75;
double currentOpacity = AnimationSettings.Calculate(
    updateSettings.StartOpacity, 
    updateSettings.EndOpacity, 
    currentProgress
);

Console.WriteLine($"Current Opacity: {currentOpacity:F2}");
```

## Notes

*   **Validation Requirements**: The `Validate()` method must be called after modifying properties such as `DurationMs`, `FrameRate`, or opacity values before invoking `GetTotalFrames()` or `GetProgress()`. Failure to do so may result in runtime exceptions if the internal state represents an impossible animation timeline (e.g., zero duration).
*   **Thread Safety**: The `AnimationSettings` class is not thread-safe. While the static `Calculate` method is safe for concurrent use as it operates purely on input parameters, instance members (properties and methods like `GetProgress`) should not be accessed simultaneously from multiple threads without external synchronization, particularly if properties are being modified.
*   **Easing Function Dependency**: The accuracy of `GetProgress` relies entirely on the correctness of the assigned `EasingType`. If a custom delegate is assigned to `EasingType`, it must guarantee a return value between 0.0 and 1.0 for inputs in the same range to prevent visual artifacts.
*   **Opacity Range**: Although `StartOpacity` and `EndOpacity` are defined as `double`, logical validity requires them to remain within the range [0.0, 1.0]. Values outside this range will cause `Validate()` to throw an exception.
