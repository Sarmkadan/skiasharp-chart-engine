# IInteractivityService

The `IInteractivityService` interface defines methods and properties for handling user interactions such as tooltips, zooming, panning, and viewport management in chart visualizations. It enables dynamic responses to user input by providing hit-testing capabilities and viewport state manipulation.

## API

### `public InteractivityService()`

Initializes a new instance of the interactivity service. This constructor is typically used by dependency injection containers or when manually instantiating the service.

### `public TooltipHitResult HitTest(PointF point)`

Determines which chart element, if any, is located at the specified screen point and returns a tooltip result.

- **Parameters**:
  - `point` (`PointF`): The screen coordinates to test against the chart elements.
- **Return value**: A `TooltipHitResult` containing the hit element and associated data, or `null` if no element was hit.
- **Exceptions**: Throws `ArgumentNullException` if `point` is invalid or out of bounds.

### `public TooltipHitResult HitTest(PointF point, bool includeOutOfBoundElements)`

Determines which chart element is located at the specified screen point, optionally including elements outside the visible viewport.

- **Parameters**:
  - `point` (`PointF`): The screen coordinates to test.
  - `includeOutOfBoundElements` (`bool`): If `true`, includes elements outside the visible viewport in the hit test.
- **Return value**: A `TooltipHitResult` containing the hit element and data, or `null` if no element was hit.
- **Exceptions**: Throws `ArgumentNullException` if `point` is invalid.

### `public Task<TooltipHitResult> HitTestAsync(PointF point)`

Asynchronously determines which chart element is located at the specified screen point and returns a tooltip result.

- **Parameters**:
  - `point` (`PointF`): The screen coordinates to test.
- **Return value**: A `Task<TooltipHitResult>` representing the asynchronous operation. The task resolves to a `TooltipHitResult` or `null` if no element was hit.
- **Exceptions**: Throws `ArgumentNullException` if `point` is invalid.

### `public Task<TooltipHitResult> HitTestAsync(PointF point, bool includeOutOfBoundElements)`

Asynchronously determines which chart element is located at the specified screen point, optionally including elements outside the visible viewport.

- **Parameters**:
  - `point` (`PointF`): The screen coordinates to test.
  - `includeOutOfBoundElements` (`bool`): If `true`, includes elements outside the visible viewport in the hit test.
- **Return value**: A `Task<TooltipHitResult>` representing the asynchronous operation. The task resolves to a `TooltipHitResult` or `null` if no element was hit.
- **Exceptions**: Throws `ArgumentNullException` if `point` is invalid.

### `public ViewportState Zoom(double factor, PointF? center = null)`

Applies a zoom transformation to the chart viewport centered optionally at a specific point.

- **Parameters**:
  - `factor` (`double`): The zoom factor. Values greater than 1 zoom in, less than 1 zoom out.
  - `center` (`PointF?`, optional): The screen point to center the zoom around. If `null`, the center of the viewport is used.
- **Return value**: A `ViewportState` representing the new viewport configuration after the zoom operation.
- **Exceptions**: Throws `ArgumentOutOfRangeException` if `factor` is zero or negative.

### `public ViewportState Pan(PointF delta)`

Shifts the viewport by the specified screen-space delta.

- **Parameters**:
  - `delta` (`PointF`): The screen-space offset to apply to the viewport.
- **Return value**: A `ViewportState` representing the new viewport configuration after the pan operation.
- **Exceptions**: Throws `ArgumentNullException` if `delta` is invalid.

### `public ViewportState ResetViewport()`

Resets the viewport to its default state, typically showing the full data range.

- **Return value**: A `ViewportState` representing the viewport after reset.
- **Exceptions**: None.

### `public (double minX, double maxX, double minY, double maxY) GetVisibleRange()`

Retrieves the current visible data range in the chart viewport.

- **Return value**: A tuple containing the minimum and maximum X and Y values visible in the viewport.
- **Exceptions**: None.

### `public string FormatTooltip(TooltipHitResult hitResult)`

Formats a tooltip string for the given hit result.

- **Parameters**:
  - `hitResult` (`TooltipHitResult`): The hit test result containing the chart element and data to format.
- **Return value**: A formatted string suitable for display as a tooltip.
- **Exceptions**: Throws `ArgumentNullException` if `hitResult` is `null`.

## Usage

### Example 1: Handling a Mouse Click with Tooltip
