# InteractivityExtensions

`InteractivityExtensions` provides a set of extension methods that integrate interactive chart behaviors—such as tooltip retrieval, zooming, panning, and viewport reset—into the dependency injection pipeline and the runtime manipulation of chart viewports. It bridges raw pointer or touch input with the chart’s coordinate system, returning structured hit-test results and producing updated viewport states that can be applied directly to a `SKChart` surface.

## API

### `AddChartInteractivity`

```csharp
public static IServiceCollection AddChartInteractivity(this IServiceCollection services)
```

Registers the services required for chart interactivity (e.g., hit-testing pipelines, coordinate transforms, and default interactivity strategies) into the provided `IServiceCollection`. Call this during application startup to make `GetTooltipAtAsync` and related methods available through dependency injection.

- **Parameters:**  
  `services` — the `IServiceCollection` to augment. Must not be null.
- **Returns:**  
  The same `IServiceCollection` instance, enabling fluent chaining.
- **Throws:**  
  `ArgumentNullException` when `services` is null.

---

### `GetTooltipAt` (synchronous)

```csharp
public static TooltipHitResult GetTooltipAt(this SKChart chart, PointF location)
public static TooltipHitResult GetTooltipAt(this SKChart chart, PointF location, TooltipOptions options)
```

Performs a synchronous hit-test at the given `location` (in chart-relative coordinates) and returns the nearest tooltip-eligible data point. The overload without `TooltipOptions` uses default sensitivity and filtering.

- **Parameters:**  
  `chart` — the `SKChart` instance to query.  
  `location` — a `PointF` in the chart’s pixel coordinate space.  
  `options` — (optional) a `TooltipOptions` object controlling hit radius, series filtering, and multi-series resolution.
- **Returns:**  
  A `TooltipHitResult` containing the hit series, data point index, screen position, and associated model values. If no data point falls within the configured radius, `TooltipHitResult.Empty` is returned.
- **Throws:**  
  `ArgumentNullException` when `chart` is null.  
  `InvalidOperationException` when the chart has not completed at least one render pass (axes and transforms are uninitialized).

---

### `GetTooltipAtAsync`

```csharp
public static Task<TooltipHitResult> GetTooltipAtAsync(this SKChart chart, PointF location, CancellationToken cancellationToken = default)
```

Asynchronous version of `GetTooltipAt` that supports cooperative cancellation. This overload always uses the default `TooltipOptions` registered via `AddChartInteractivity`. It is intended for scenarios where hit-testing may involve deferred data resolution or async series accessors.

- **Parameters:**  
  `chart` — the `SKChart` instance to query.  
  `location` — a `PointF` in chart pixel coordinates.  
  `cancellationToken` — a `CancellationToken` to cancel the hit-test operation.
- **Returns:**  
  A `Task<TooltipHitResult>` that resolves to the hit result or `TooltipHitResult.Empty`.
- **Throws:**  
  `ArgumentNullException` when `chart` is null.  
  `OperationCanceledException` when the token is signaled before completion.  
  `InvalidOperationException` when interactivity services are not registered or the chart is not yet rendered.

---

### `ZoomAt`

```csharp
public static ViewportState ZoomAt(this SKChart chart, PointF anchor, float scaleFactor)
```

Computes a new `ViewportState` that zooms the chart around a fixed screen-space `anchor` by the given multiplicative `scaleFactor`. Values greater than 1.0 magnify the view; values between 0 and 1.0 shrink it. The returned state preserves the anchor point’s data-space position.

- **Parameters:**  
  `chart` — the source `SKChart`.  
  `anchor` — the screen-space point (in chart pixel coordinates) that should remain stationary during the zoom.  
  `scaleFactor` — the multiplier applied to the current viewport extents.
- **Returns:**  
  A new `ViewportState` representing the zoomed viewport. The caller is responsible for applying it to the chart.
- **Throws:**  
  `ArgumentNullException` when `chart` is null.  
  `ArgumentOutOfRangeException` when `scaleFactor` is zero or negative.  
  `InvalidOperationException` when the chart’s current viewport is not available.

---

### `PanBy`

```csharp
public static ViewportState PanBy(this SKChart chart, SizeF delta)
```

Computes a new `ViewportState` that pans (translates) the chart’s visible data window by the screen-space displacement `delta`. Positive `delta.Width` moves the viewport right in data space; positive `delta.Height` moves it downward.

- **Parameters:**  
  `chart` — the source `SKChart`.  
  `delta` — a `SizeF` representing the horizontal and vertical pixel displacement.
- **Returns:**  
  A new `ViewportState` with the translated extents.
- **Throws:**  
  `ArgumentNullException` when `chart` is null.  
  `InvalidOperationException` when the chart’s current viewport is not available.

---

### `ResetViewport`

```csharp
public static ViewportState ResetViewport(this SKChart chart)
```

Returns the default `ViewportState` that resets the chart to its initial, unzoomed, unpanned extents (typically the full data range or the explicitly configured default viewport).

- **Parameters:**  
  `chart` — the source `SKChart`.
- **Returns:**  
  A `ViewportState` representing the default viewport.
- **Throws:**  
  `ArgumentNullException` when `chart` is null.  
  `InvalidOperationException` when the chart has no default viewport configured and no data bounds are available.

## Usage

### Example 1: Registering interactivity and handling a mouse move for tooltips

```csharp
// Startup
var services = new ServiceCollection();
services.AddChartInteractivity();
var provider = services.BuildServiceProvider();

// In the chart control
private async void OnMouseMove(MouseEventArgs e)
{
    var location = new PointF(e.X, e.Y);
    var hit = await myChart.GetTooltipAtAsync(location);

    if (hit.IsHit)
    {
        tooltipLabel.Text = $"Series: {hit.SeriesName}, Value: {hit.Value:F2}";
        tooltipLabel.Location = Point.Round(hit.ScreenPosition);
        tooltipLabel.Visible = true;
    }
    else
    {
        tooltipLabel.Visible = false;
    }
}
```

### Example 2: Pinch-to-zoom and drag-to-pan on a touch-enabled chart

```csharp
private ViewportState currentViewport;

private void OnTouchPinch(PointF center, float scale)
{
    currentViewport = myChart.ZoomAt(center, scale);
    myChart.ApplyViewport(currentViewport);
}

private void OnTouchDrag(SizeF delta)
{
    currentViewport = myChart.PanBy(delta);
    myChart.ApplyViewport(currentViewport);
}

private void OnDoubleTap()
{
    currentViewport = myChart.ResetViewport();
    myChart.ApplyViewport(currentViewport);
}
```

## Notes

- All `GetTooltipAt*` methods require the chart to have completed at least one measure/layout/render cycle; calling them before the chart is fully initialized throws `InvalidOperationException`. Defer tooltip queries until after the `Loaded` event or the first `PaintSurface` callback.
- `ZoomAt`, `PanBy`, and `ResetViewport` are pure functions that compute new `ViewportState` values. They do **not** mutate the chart directly. The caller must explicitly apply the returned state to the chart’s viewport property or render context.
- `ZoomAt` with a `scaleFactor` of exactly 1.0 returns a viewport identical to the current one. Scale factors very close to zero may produce degenerate viewports; clamp to a sensible minimum (e.g., 0.01) to avoid numerical instability.
- `GetTooltipAtAsync` relies on services registered by `AddChartInteractivity`. If `AddChartInteractivity` has not been called, the method throws `InvalidOperationException`. In synchronous `GetTooltipAt` overloads, default options are created inline when none are supplied, so service registration is optional for the synchronous path.
- Thread safety: The extension methods themselves are stateless static functions and are safe to call from any thread. However, the `SKChart` instance they operate on is not guaranteed to be thread-safe. All calls should be marshaled to the UI or render thread that owns the chart. Concurrent calls to `GetTooltipAtAsync` on the same chart may produce overlapping read operations on chart internals; serialize access if the chart implementation is not explicitly documented as thread-safe.
