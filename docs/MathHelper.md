# MathHelper

The `MathHelper` class is a static utility providing a collection of mathematical functions tailored for the requirements of the `skiasharp-chart-engine`. It facilitates common operations such as data normalization, scaling, statistical analysis, and easing functions, ensuring consistent mathematical behavior across chart rendering components.

## API

### GetMinMax
Calculates the minimum and maximum values from a collection.
*   **Parameters**:
    *   `values` (`IEnumerable<float>` or `float[]`): The dataset to analyze.
*   **Returns**: A `(float Min, float Max)` tuple representing the bounds of the input data.
*   **Exceptions**: Throws `ArgumentException` if the input collection is null or empty.

### Normalize
Transforms a value into a normalized range between 0.0 and 1.0 based on provided bounds.
*   **Parameters**:
    *   `value` (`float`): The value to normalize.
    *   `min` (`float`): The minimum bound.
    *   `max` (`float`): The maximum bound.
*   **Returns**: A `float` between 0.0 and 1.0.

### ScaleToRange
Maps a value from an input range to a target range.
*   **Parameters**:
    *   `value` (`float`): The value to scale.
    *   `min` (`float`): The minimum of the input range.
    *   `max` (`float`): The maximum of the input range.
    *   `targetMin` (`float`): The minimum of the target range.
    *   `targetMax` (`float`): The maximum of the target range.
*   **Returns**: The scaled `float` value.

### Lerp
Performs a linear interpolation between two values.
*   **Parameters**:
    *   `start` (`float`): The starting value.
    *   `end` (`float`): The ending value.
    *   `t` (`float`): The interpolation factor (typically 0.0 to 1.0).
*   **Returns**: The interpolated `float` value.

### GenerateAxisTicks
Calculates a set of evenly spaced tick values for a chart axis.
*   **Parameters**:
    *   `min` (`float`): The minimum axis value.
    *   `max` (`float`): The maximum axis value.
    *   `count` (`int`): The desired number of ticks.
*   **Returns**: A `List<float>` containing the calculated tick positions.

### Clamp
Restricts a value within a specified inclusive range.
*   **Parameters**:
    *   `value` (`float`): The value to clamp.
    *   `min` (`float`): The lower bound.
    *   `max` (`float`): The upper bound.
*   **Returns**: The clamped `float` value.

### Average
Calculates the arithmetic mean of a dataset.
*   **Parameters**:
    *   `values` (`IEnumerable<float>` or `float[]`): The dataset.
*   **Returns**: The `float` average value.
*   **Exceptions**: Throws `ArgumentException` if the input collection is null or empty.

### StandardDeviation
Calculates the population standard deviation of a dataset.
*   **Parameters**:
    *   `values` (`IEnumerable<float>` or `float[]`): The dataset.
*   **Returns**: The `float` standard deviation.

### EaseInOutQuad
Applies a quadratic ease-in-ease-out easing function.
*   **Parameters**:
    *   `t` (`float`): A value representing progress (typically 0.0 to 1.0).
*   **Returns**: The eased `float` value.

### ApproximatelyEqual
Checks if two floating-point numbers are effectively equal within a small tolerance.
*   **Parameters**:
    *   `a` (`float`): The first value.
    *   `b` (`float`): The second value.
    *   `epsilon` (`float`): The maximum allowable difference.
*   **Returns**: `bool` true if the difference is within `epsilon`.

### RoundToMagnitude
Rounds a value to the nearest multiple of a specified magnitude.
*   **Parameters**:
    *   `value` (`float`): The value to round.
    *   `magnitude` (`float`): The magnitude base.
*   **Returns**: The rounded `float` value.

## Usage

```csharp
// Example 1: Normalizing data and scaling for chart rendering
float rawValue = 75.0f;
float minBound = 0.0f;
float maxBound = 100.0f;

float normalized = MathHelper.Normalize(rawValue, minBound, maxBound);
float scaled = MathHelper.ScaleToRange(normalized, 0, 1, 0, 500); // Scale to 500px chart height

// Example 2: Generating axis ticks
var ticks = MathHelper.GenerateAxisTicks(0, 1000, 5);
// Result: { 0, 250, 500, 750, 1000 }
```

## Notes

*   **Thread Safety**: This class is entirely static and contains no internal mutable state. All methods are thread-safe and can be invoked concurrently from multiple threads without synchronization.
*   **Floating Point Precision**: When using `ApproximatelyEqual` or performing calculations involving very large or very small magnitudes, be mindful of standard IEEE 754 floating-point limitations.
*   **Input Validation**: Methods operating on `IEnumerable<float>` or `float[]` will throw an `ArgumentException` if the input is `null` or contains no elements; callers should ensure datasets are validated before invocation.
