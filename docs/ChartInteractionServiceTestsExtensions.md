# ChartInteractionServiceTestsExtensions

Static helper class providing extension-style methods for writing unit tests that verify chart interaction behavior, such as hit testing and tooltip validation, against the SkiaSharp chart engine.

## API

### `CreateChart()`
Creates and returns a new `Chart` instance configured with default axes, a single series, and basic styling suitable for interaction testing.

- **Parameters**: None
- **Return value**: A new `Chart` instance ready for interaction tests.
- **Exceptions**: Throws `InvalidOperationException` if the chart cannot be initialized due to missing resources or configuration errors.

---

### `MakeHit(Chart chart, float x, float y)`
Constructs a `TooltipHitResult` as if a tooltip were triggered at the specified screen coordinates.

- **Parameters**:
  - `chart`: The chart under test.
  - `x`: The x-coordinate in screen space.
  - `y`: The y-coordinate in screen space.
- **Return value**: A `TooltipHitResult` populated with the nearest data point and metadata.
- **Exceptions**: Throws `ArgumentNullException` if `chart` is `null`.

---

### `ShouldBeValidHit(TooltipHitResult result)`
Asserts that the given hit result represents a valid data point hit (i.e., not a miss and not null).

- **Parameters**:
  - `result`: The hit result to validate.
- **Exceptions**: Throws `XunitException` if the result is `null`, the hit point is invalid, or the associated data item is missing.

---

### `ShouldBeMiss(TooltipHitResult result)`
Asserts that the given hit result represents a miss (no data point under the cursor).

- **Parameters**:
  - `result`: The hit result to validate.
- **Exceptions**: Throws `XunitException` if the result is not a miss or is `null`.

---
### `SetupHitTest(Chart chart, float x, float y)`
Configures the chart’s internal hit-testing state so that subsequent calls to `MakeHit` will return a result as if the cursor were at `(x, y)`.

- **Parameters**:
  - `chart`: The chart under test.
  - `x`: The simulated x-coordinate.
  - `y`: The simulated y-coordinate.
- **Exceptions**: Throws `ArgumentNullException` if `chart` is `null`.

---
### `SetupHitMiss(Chart chart)`
Configures the chart so that any subsequent hit test will return a miss.

- **Parameters**:
  - `chart`: The chart under test.
- **Exceptions**: Throws `ArgumentNullException` if `chart` is `null`.

## Usage
