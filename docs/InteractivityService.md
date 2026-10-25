# InteractivityService

`InteractivityService` is the default implementation of `IInteractivityService`. It lives in `Services/InteractivityService.cs` under the `SkiaSharpChartEngine.Services` namespace and provides two capabilities over a chart's data coordinate space:

- **Tooltip hit-testing:** find the data point nearest to a canvas pointer position and produce tooltip text.
- **Viewport management:** zoom, pan, reset, and compute the visible data range.

The service is stateless apart from its logger, so one instance can be shared. Viewport operations never modify the viewport they receive; `Zoom` and `Pan` return a new `ViewportState`.

All coordinate maths maps the chart's data space to canvas pixels using the margin constants in `ChartConstants` (`DefaultMarginLeft` 80, `DefaultMarginTop` 40, `DefaultMarginRight` 40, `DefaultMarginBottom` 60). These are the same margins the renderers use, so hit positions line up with drawn points.

## Relationship to `InteractivityOptions`

There is no type named `InteractivityOptions`. The file `Models/InteractivityOptions.cs` holds three types that the service consumes or produces:

| Type | Role in `InteractivityService` |
| --- | --- |
| `TooltipOptions` | Optional `options` argument of `HitTest`, `HitTestAsync` and `FormatTooltip`. The service reads `Enabled`, `HitRadius` and `ContentTemplate`. A `null` argument is replaced with `new TooltipOptions()`. |
| `ViewportState` | Optional `viewport` argument of `HitTest` and `HitTestAsync`. Required input for `Zoom`, `Pan` and `GetVisibleRange`, and the return type of `Zoom`, `Pan` and `ResetViewport`. |
| `TooltipHitResult` | Return type of both `HitTest` overloads, and input to `FormatTooltip`. |

See [InteractivityOptions](InteractivityOptions.md) for the properties, defaults and validation rules of these types, and [IInteractivityService](IInteractivityService.md) for the interface contract.

`ChartRegion`, which `TooltipHitResult.Region` uses, is defined in `Constants/ChartConstants.cs`.

## Dependency injection

`InteractivityService` has one constructor and no parameterless overload:

```csharp
public InteractivityService(ILogger<InteractivityService> logger)
```

`logger` must not be `null`; otherwise the constructor throws `ArgumentNullException`.

`Extensions/InteractivityExtensions.cs` registers the service with `AddChartInteractivity()`, which adds `IInteractivityService` as a singleton backed by `InteractivityService`. For standalone use without a container, `InteractivityExtensions` keeps a private default instance that uses `NullLogger<InteractivityService>.Instance`.

`ChartInteractionService` depends on `IInteractivityService` and delegates its hit-testing to it.

## API

### `HitTest(Chart, float, float, float, float, TooltipOptions?, ViewportState?)`

```csharp
public TooltipHitResult HitTest(Chart chart, float pointerX, float pointerY,
    float canvasWidth, float canvasHeight,
    TooltipOptions? options = null, ViewportState? viewport = null)
```

Finds the data point nearest to the pointer. The method:

1. Throws `ArgumentNullException` if `chart` is `null`.
2. Throws `ArgumentOutOfRangeException` if `canvasWidth` or `canvasHeight` is `<= 0`.
3. Returns the shared `TooltipHitResult.Miss` if `options.Enabled` is `false`.
4. Computes the visible data range. If the range is degenerate (`maxX <= minX` or `maxY <= minY`), it logs a warning and returns `Miss`.
5. Computes the plot rectangle from the margins. If it has no positive area, it returns `Miss`.
6. Classifies the pointer into a `ChartRegion`.
7. Scans every point of every visible series (`IsVisible` is `true` and the series has data). A point is a candidate when its Euclidean canvas distance to the pointer is strictly less than `HitRadius`. The nearest candidate wins; on a tie, the first one scanned wins.

When a point matches, the result has `IsHit = true`, the matched `DataPoint`, `Series`, `SeriesIndex`, `DistancePx`, `CanvasX` and `CanvasY`, and `TooltipText` set by `FormatTooltip`. When nothing matches, the method returns a new result with `IsHit = false` and `Region` set.

Behaviour to be aware of:

- Points are not clipped to the plot area. A point whose canvas position falls outside the plot, because it lies outside the visible range, can still match if the pointer is within `HitRadius` of it.
- Only the early exits (disabled options, degenerate bounds, empty plot) return the shared `Miss` instance, and those results have `Region = ChartRegion.Outside`. Do not mutate `Miss`.
- Region classification is based on the pointer position alone: title band, Y-axis band, X-axis band, legend band (right of the plot), plot area, or outside.
- A `null` `viewport` is treated as a default `ViewportState` (no zoom or pan).

### `HitTest(Chart, SKPoint, float, float, TooltipOptions?, ViewportState?)`

```csharp
public TooltipHitResult HitTest(Chart chart, SKPoint point,
    float canvasWidth, float canvasHeight,
    TooltipOptions? options = null, ViewportState? viewport = null)
```

Forwards `point.X` and `point.Y` to the overload above. Argument checks and results are identical.

### `HitTestAsync(Chart, float, float, float, float, TooltipOptions?, ViewportState?, CancellationToken)`

```csharp
public Task<TooltipHitResult> HitTestAsync(Chart chart, float pointerX, float pointerY,
    float canvasWidth, float canvasHeight,
    TooltipOptions? options = null, ViewportState? viewport = null,
    CancellationToken cancellationToken = default)
```

Checks `cancellationToken` first, then runs `HitTest` synchronously and returns a completed task. No work is actually offloaded. Exceptions thrown by `HitTest` (such as `ArgumentNullException`) are thrown synchronously from this method, not stored in the returned `Task`. Cancellation throws `OperationCanceledException` from the same synchronous call.

### `HitTestAsync(Chart, SKPoint, float, float, TooltipOptions?, ViewportState?, CancellationToken)`

```csharp
public Task<TooltipHitResult> HitTestAsync(Chart chart, SKPoint point,
    float canvasWidth, float canvasHeight,
    TooltipOptions? options = null, ViewportState? viewport = null,
    CancellationToken cancellationToken = default)
```

Same behaviour as the overload above, with the pointer given as an `SKPoint`.

### `Zoom`

```csharp
public ViewportState Zoom(Chart chart, ViewportState current,
    float anchorX, float anchorY, float canvasWidth, float canvasHeight, double factor)
```

Returns a clone of `current` zoomed by `factor`. The method:

1. Throws `ArgumentNullException` for a `null` `chart` or `current`.
2. Throws `ArgumentOutOfRangeException` if `factor <= 0`.
3. Multiplies `ZoomX` and `ZoomY` by `factor` and clamps each to `[0.01, 100]`.
4. Converts the anchor canvas point to data coordinates using the current visible range.
5. Sets the new visible range to the full data span divided by the new zoom, centred on the anchor's data coordinate. Pan offsets are updated to match.

The anchor data point ends up at the centre of the new visible range. It is not kept under the same pixel. Canvas dimensions are not validated here, so the caller must pass positive values.

### `Pan`

```csharp
public ViewportState Pan(Chart chart, ViewportState current,
    float deltaX, float deltaY, float canvasWidth, float canvasHeight)
```

Returns a clone of `current` translated by a pixel delta. Positive `deltaX` moves the chart content right (the visible window shifts left in data space). Positive `deltaY` moves the content down. The delta is converted to data units using the plot size and the current visible span.

If the plot area has no positive size, the method returns an unchanged clone. Panning is not clamped to the data bounds, so the view can be moved entirely off the data.

### `ResetViewport`

```csharp
public ViewportState ResetViewport(Chart chart)
```

Returns a new `ViewportState` with zoom 1, pan 0, and `VisibleXRange` and `VisibleYRange` set to the full data bounds. It logs at Information level.

### `GetVisibleRange`

```csharp
public (double minX, double maxX, double minY, double maxY) GetVisibleRange(
    Chart chart, ViewportState viewport)
```

Returns the visible data range. If the viewport is not default and both cached `VisibleXRange` and `VisibleYRange` are set, those cached ranges are returned. Otherwise the range is derived from the full data bounds, the zoom factors and the pan offsets. Throws `ArgumentNullException` for a `null` argument.

### `FormatTooltip`

```csharp
public string FormatTooltip(TooltipHitResult hit, TooltipOptions? options = null)
```

Returns an empty string if `hit` is `null`, is not a hit, or has no `DataPoint`. Otherwise:

- If `options.ContentTemplate` is not blank, it replaces the placeholders `{x}` and `{y}` (formatted with `"G6"`), `{label}`, and `{series}`. Replacement is sequential (`{x}`, `{y}`, `{label}`, `{series}`), so a label that contains a later placeholder, such as `{series}`, is replaced as well.
- Otherwise it returns the default text: `Series — Label`, then `X: <x>` and `Y: <y>` on separate lines. The label part is omitted when the label is blank, and the series name falls back to `Series`.

`options.Enabled` is not checked here. Number formatting uses the current culture.

## Data bounds

`Zoom`, `Pan`, `ResetViewport` and `GetVisibleRange` derive the full data range from the chart's visible series only. Hiding a series therefore changes the bounds and the data-space meaning of any stored zoom or pan offsets.

If there are no visible points, the bounds default to `(0, 1, 0, 1)`. A zero-width or zero-height span is widened by 1 on each side so that the scale stays finite.

## Logging

| Level | Message |
| --- | --- |
| Warning | Chart has degenerate data bounds; hit-test skipped. |
| Debug | A hit-test matched a point (series, coordinates, distance). |
| Debug | A zoom or pan was applied. |
| Information | The viewport was reset. |

## Thread safety

The service holds no mutable state. Viewport objects are mutable, but `Zoom` and `Pan` work on clones, so a viewport can be shared as long as callers do not mutate it concurrently. `HitTest` reads the `Chart` and its series, so the chart must not be modified while a hit-test is running.

## Example

```csharp
var logger = NullLogger<InteractivityService>.Instance;
var service = new InteractivityService(logger);

var viewport = new ViewportState();

// Zoom in on the pointer position, then hit-test the same pointer.
viewport = service.Zoom(chart, viewport, pointerX, pointerY, 800f, 600f, 2.0);

var options = new TooltipOptions { HitRadius = 24f, ContentTemplate = "{series}: {x}, {y}" };
TooltipHitResult hit = service.HitTest(chart, pointerX, pointerY, 800f, 600f, options, viewport);

if (hit.IsHit)
{
    Console.WriteLine(hit.TooltipText);
}
```
