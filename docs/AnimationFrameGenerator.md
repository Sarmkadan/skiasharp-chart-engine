# AnimationFrameGenerator

The `AnimationFrameGenerator` class produces a sequence of frames for chart animations. It encapsulates the logic for interpolating chart states over a number of frames, exposing the current frame index, animation progress, and the underlying data values. The generator can produce either full `Chart` objects or lightweight `AnimationFrame` data containers, depending on the caller’s need for rendering or serialization.

## API

### `public AnimationFrameGenerator()`

Initializes a new instance of the `AnimationFrameGenerator` with default settings. No parameters are required. The generator is ready to produce frames once the data source is provided (typically through the `Values` property or via a chart reference).

### `public List<Chart> GenerateFrames()`

Generates and returns a list of `Chart` objects, each representing a single frame of the animation. The number of frames is determined by the generator’s internal configuration (e.g., a fixed frame count or a duration-based calculation).

- **Returns**: `List<Chart>` – A collection of fully constructed chart frames.
- **Throws**: `InvalidOperationException` if the generator has not been properly initialized with data (e.g., `Values` is null or empty).

### `public List<AnimationFrame> GenerateDataFrames()`

Generates and returns a list of `AnimationFrame` objects. Each `AnimationFrame` contains the interpolated data values for that frame, without the overhead of a full chart object. This is useful for custom rendering or when only the numeric data is needed.

- **Returns**: `List<AnimationFrame>` – A collection of lightweight frame data containers.
- **Throws**: `InvalidOperationException` if the generator has not been properly initialized with data.

### `public int FrameNumber { get; }`

Gets the current frame index during generation. The value is meaningful only while a generation method (`GenerateFrames` or `GenerateDataFrames`) is executing. After generation completes, this property retains the last frame number.

- **Value**: An integer from 0 to (total frames – 1).

### `public double Progress { get; }`

Gets the animation progress as a value between 0.0 and 1.0. During generation, this property reflects the normalized progress of the current frame relative to the entire animation. After generation, it holds the final progress value (1.0).

- **Value**: A `double` in the range [0.0, 1.0].

### `public List<double> Values { get; set; }`

Gets or sets the list of data values that drive the animation. The generator uses these values to interpolate between the starting and ending states of the chart. Setting this property after construction reinitializes the internal state.

- **Value**: A `List<double>` containing the data points. Can be `null` or empty, but calling a generation method in that state will throw an `InvalidOperationException`.

## Usage

### Example 1: Generating chart frames for a slideshow

```csharp
using SkiaSharp;
using SkiasharpChartEngine;

var generator = new AnimationFrameGenerator();
generator.Values = new List<double> { 10, 20, 30, 40, 50 };

// Generate 30 frames (default frame count)
List<Chart> frames = generator.GenerateFrames();

foreach (var chart in frames)
{
    // Render each chart to a bitmap or display in a UI
    using var surface = SKSurface.Create(new SKImageInfo(800, 600));
    var canvas = surface.Canvas;
    chart.Draw(canvas);
    // Save or present the frame...
}
```

### Example 2: Using data frames and monitoring progress

```csharp
using SkiasharpChartEngine;

var generator = new AnimationFrameGenerator();
generator.Values = new List<double> { 5, 15, 25 };

// Generate data frames and log progress
List<AnimationFrame> dataFrames = generator.GenerateDataFrames();

for (int i = 0; i < dataFrames.Count; i++)
{
    Console.WriteLine($"Frame {generator.FrameNumber}: Progress = {generator.Progress:P}");
    Console.WriteLine($"Values: {string.Join(", ", dataFrames[i].Values)}");
}
```

## Notes

- **Edge Cases**:  
  - If `Values` is `null` or contains fewer than two elements, the generator cannot interpolate and will throw `InvalidOperationException` when `GenerateFrames` or `GenerateDataFrames` is called.  
  - A single value in `Values` produces a single frame with no interpolation.  
  - The `FrameNumber` and `Progress` properties are only valid during a generation call; reading them before any generation method is called returns default values (0 and 0.0 respectively).

- **Thread Safety**:  
  Instances of `AnimationFrameGenerator` are **not thread-safe**. Concurrent calls to generation methods or simultaneous reads/writes of `Values` from multiple threads may produce inconsistent state. External synchronization (e.g., a lock) is required if the generator is shared across threads.
