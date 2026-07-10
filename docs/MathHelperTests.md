# MathHelperTests

The `MathHelperTests` class contains unit tests for the static utility methods defined in the `MathHelper` class of the `skiasharp-chart-engine` project. Each test method validates a specific behavior or edge case of the corresponding helper function, ensuring correctness for common mathematical operations used in chart calculations such as normalization, interpolation, standard deviation, axis tick generation, and clamping.

## API

The following public test methods are defined in `MathHelperTests`. Each method is a parameterless test (e.g., decorated with `[Fact]` or `[Test]`) that exercises a particular scenario and asserts the expected outcome.

- **`GetMinMax_EmptyCollection_ThrowsArgumentException`**  
  Verifies that `MathHelper.GetMinMax` throws an `ArgumentException` when the input collection is empty.

- **`Normalize_WhenMinAndMaxAreEqual_ReturnsHalf`**  
  Verifies that `MathHelper.Normalize` returns `0.5` when the minimum and maximum bounds are equal (i.e., a zero‑width range).

- **`Normalize_MidpointBetweenBounds_ReturnsZeroPointFive`**  
  Verifies that `MathHelper.Normalize` returns `0.5` when the input value is exactly the midpoint between the given minimum and maximum bounds.

- **`Lerp_TExceedsOne_ClampsToEndValue`**  
  Verifies that `MathHelper.Lerp` clamps the interpolation parameter `t` to `1.0` when `t > 1`, returning the end value.

- **`Lerp_TIsZero_ReturnsStartValue`**  
  Verifies that `MathHelper.Lerp` returns the start value when the interpolation parameter `t` is `0`.

- **`StandardDeviation_CollectionWithSingleElement_ThrowsArgumentException`**  
  Verifies that `MathHelper.StandardDeviation` throws an `ArgumentException` when the input collection contains only one element.

- **`StandardDeviation_TwoSymmetricValues_ReturnsExpectedDeviation`**  
  Verifies that `MathHelper.StandardDeviation` computes the correct standard deviation for two symmetric values (e.g., `{a, -a}`).

- **`GenerateAxisTicks_ZeroRange_ReturnsSingleTick`**  
  Verifies that `MathHelper.GenerateAxisTicks` returns a single tick value when the data range is zero.

- **`Clamp_ValueAboveMax_ReturnsMax`**  
  Verifies that `MathHelper.Clamp` returns the maximum bound when the input value exceeds it.

- **`Clamp_ValueBelowMin_ReturnsMin`**  
  Verifies that `MathHelper.Clamp` returns the minimum bound when the input value is below it.

## Usage

The following examples demonstrate how to use the underlying `MathHelper` methods in a typical chart‑engine context.

### Example 1: Normalizing and interpolating a data point

```csharp
using SkiasharpChartEngine.Utilities;

double[] data = { 10, 20, 30, 40, 50 };
double min = data.Min();
double max = data.Max();

// Normalize a value to the [0,1] range
double normalized = MathHelper.Normalize(25, min, max); // returns 0.375

// Interpolate between two colors (represented as doubles for simplicity)
double start = 0.0;
double end = 1.0;
double interpolated = MathHelper.Lerp(start, end, 0.75); // returns 0.75
```

### Example 2: Computing standard deviation and generating axis ticks

```csharp
using SkiasharpChartEngine.Utilities;

double[] measurements = { 2.5, 3.1, 2.8, 3.0, 2.9 };
double stdDev = MathHelper.StandardDeviation(measurements); // ≈ 0.216

// Generate axis ticks for a chart with zero range (e.g., all values equal)
double[] ticks = MathHelper.GenerateAxisTicks(100, 100); // returns { 100 }
```

## Notes

- **Edge cases**  
  - `GetMinMax` and `StandardDeviation` throw `ArgumentException` for empty or single‑element collections, respectively, because these operations are undefined for such inputs.  
  - `Normalize` returns `0.5` when `min == max` to avoid division by zero and provide a sensible default for degenerate ranges.  
  - `Lerp` clamps the interpolation parameter `t` to the closed interval `[0, 1]`, ensuring the result never exceeds the start or end values.  
  - `GenerateAxisTicks` returns a single tick when the range is zero, preventing an empty tick list.  
  - `Clamp` safely restricts any value to the specified inclusive bounds.

- **Thread safety**  
  All `MathHelper` methods are static and stateless; they do not modify any shared state. Therefore, they are inherently thread‑safe and can be called concurrently from multiple threads without synchronization.
