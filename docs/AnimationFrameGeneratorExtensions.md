# AnimationFrameGeneratorExtensions

> **Status: not part of the build.** `Rendering/AnimationFrameGeneratorExtensions.cs` is excluded by `<Compile Remove="Rendering/**/*.cs" />` in `skiasharp-chart-engine.csproj`, so `dotnet build` never compiles it. Even if it were included, it would not compile: it declares `namespace Rendering` (not `SkiaSharpChartEngine.Rendering`), and it reads members that `AnimationFrameGenerator` does not have (`Progress`, `FrameNumber`, `Values`) or uses as properties methods that take parameters (`GenerateFrames`, `GenerateDataFrames`). This document describes the file as written, including what each method is intended to do, and is kept for reference until the extensions are either fixed or removed.

Provides four extension methods on `AnimationFrameGenerator` for reading the state and output of a generator: progress as a percentage, the current frame's `Chart`, the list of `AnimationFrame` objects, and the list of values.

## API

### `public static double GetProgressPercentage(this AnimationFrameGenerator generator)`

Returns the generator's progress scaled to a 0–100 range, computed as `generator.Progress * 100.0`.

**Parameters**
- `generator` (`AnimationFrameGenerator`, `this`): The generator to read.

**Return Value**
- `double`: The progress multiplied by 100.

**Exceptions**
- `ArgumentNullException`: Thrown when `generator` is `null` (via `ArgumentNullException.ThrowIfNull`).

**Compile issue**: `AnimationFrameGenerator` has no `Progress` member. The progress value exists only on `AnimationFrame.Progress`.

---

### `public static Chart? GetCurrentChart(this AnimationFrameGenerator generator)`

Returns the `Chart` at the generator's current frame index. The method does not generate frames. It reads a previously generated list.

**Behavior**
1. Throws if `generator` is `null`.
2. Reads the frame list from `generator.GenerateFrames`.
3. Returns `null` if the list is `null` or empty.
4. Reads the index from `generator.FrameNumber`.
5. Returns `frames[index]` if the index is within `[0, Count)`, otherwise `null`.

**Parameters**
- `generator` (`AnimationFrameGenerator`, `this`): The generator to read.

**Return Value**
- `Chart?`: The chart at the current frame, or `null` if the index is out of range or no frames exist.

**Exceptions**
- `ArgumentNullException`: Thrown when `generator` is `null`.

**Compile issue**: `AnimationFrameGenerator.GenerateFrames` is a method with the signature `GenerateFrames(Chart, Chart, int, EasingFunction)`, not a property returning a list. `FrameNumber` does not exist on the generator; it exists on `AnimationFrame`. The XML `<see cref>` to `AnimationFrameGenerator.FrameNumber` is also unresolvable.

---

### `public static IReadOnlyList<AnimationFrame> GetAnimationFrames(this AnimationFrameGenerator generator)`

Returns the `AnimationFrame` collection produced by the generator, typed as read-only.

**Parameters**
- `generator` (`AnimationFrameGenerator`, `this`): The generator to read.

**Return Value**
- `IReadOnlyList<AnimationFrame>`: The frames from `generator.GenerateDataFrames`.

**Exceptions**
- `ArgumentNullException`: Thrown when `generator` is `null`.

**Notes**
- The method returns the underlying list, not a copy. The read-only type is a compile-time view, so a caller can cast back to `List<AnimationFrame>` and modify it.
- **Compile issue**: `GenerateDataFrames` is a method on `AnimationFrameGenerator` that takes `(List<double>, List<double>, int)`. It is not a property.

---

### `public static IReadOnlyList<double> GetValues(this AnimationFrameGenerator generator)`

Returns the numeric values held by the generator, typed as read-only.

**Parameters**
- `generator` (`AnimationFrameGenerator`, `this`): The generator to read.

**Return Value**
- `IReadOnlyList<double>`: The values from `generator.Values`.

**Exceptions**
- `ArgumentNullException`: Thrown when `generator` is `null`.

**Compile issue**: `AnimationFrameGenerator` has no `Values` member. The public members are the constructor `AnimationFrameGenerator(ILogger<AnimationFrameGenerator>)`, `GenerateFrames(...)`, and `GenerateDataFrames(...)`. `AnimationFrame.Values` (a `List<double>`) is the only `Values` property in the file.

## Member Reference Check

| Referenced in extensions | Exists on `AnimationFrameGenerator`? | Where it exists |
|---|---|---|
| `Progress` (property) | No | `AnimationFrame.Progress` (`double`) |
| `FrameNumber` (property) | No | `AnimationFrame.FrameNumber` (`int`) |
| `Values` (property) | No | `AnimationFrame.Values` (`List<double>`) |
| `GenerateFrames` (used as property) | No, it is a method `GenerateFrames(Chart, Chart, int, EasingFunction)` | — |
| `GenerateDataFrames` (used as property) | No, it is a method `GenerateDataFrames(List<double>, List<double>, int)` | — |

## Related

- [AnimationFrameGenerator](AnimationFrameGenerator.md): the generator these methods extend. That document does not match the current source either. It describes a parameterless constructor and `FrameNumber`/`Progress`/`Values` properties on the generator, which the source does not define. The source constructor takes an `ILogger<AnimationFrameGenerator>`.
- `AnimationFrame` (in `Rendering/AnimationFrameGenerator.cs`): the per-frame data type that carries `FrameNumber`, `Progress`, and `Values`.
