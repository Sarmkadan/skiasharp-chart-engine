# Interactivity options

The types in `Models/InteractivityOptions.cs` configure tooltip behavior, describe tooltip hit-test results, and track the zoomed or panned chart viewport. They are in the `SkiaSharpChartEngine.Models` namespace.

## `TooltipOptions`

Controls tooltip availability, hit testing, and visual appearance.

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `Enabled` | `bool` | `true` | Enables or disables tooltips for the chart. |
| `BackgroundColor` | `string` | `"#FFFFFF"` | Tooltip background color in hex format. The string length must be from 7 through 9 characters. |
| `BorderColor` | `string` | `"#CCCCCC"` | Tooltip border color in hex format. The string length must be from 7 through 9 characters. |
| `TextColor` | `string` | `"#333333"` | Tooltip text color in hex format. The string length must be from 7 through 9 characters. |
| `Padding` | `float` | `8` | Internal padding, in pixels, around the tooltip text. Valid values range from `0` through `64`; assigning a value outside that range throws `ArgumentOutOfRangeException`. |
| `BorderRadius` | `float` | `6` | Corner radius of the tooltip box, in pixels. Valid values range from `0` through `32`; assigning a value outside that range throws `ArgumentOutOfRangeException`. |
| `FontSize` | `float` | `12` | Tooltip text size, in pixels. Valid values range from `8` through `48`; assigning a value outside that range throws `ArgumentOutOfRangeException`. |
| `HitRadius` | `float` | `16` | Maximum distance, in pixels, between the pointer and a data point for that point to count as a tooltip hit. Valid values range from `4` through `128`; assigning a value outside that range throws `ArgumentOutOfRangeException`. |
| `ContentTemplate` | `string?` | `null` | Optional tooltip content template. It supports the `{x}`, `{y}`, `{label}`, and `{series}` placeholders and has a maximum length of 1,000 characters. |
| `BorderWidth` | `float` | `1` | Width of the tooltip border stroke, in pixels. The declared validation range is `0` through `8`. |
| `ShadowOpacity` | `float` | `0.15` | Drop-shadow opacity, where `0` means no shadow and `1` means fully opaque. The declared validation range is `0` through `1`. |

`Clone()` creates a new `TooltipOptions` instance with the same values. `ToString()` returns a concise representation of selected option values.

## `TooltipHitResult`

Describes the outcome of looking for the data point nearest to a pointer position.

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `IsHit` | `bool` | `false` | Indicates whether a data point was found within the configured tooltip hit radius. |
| `DataPoint` | `DataPoint?` | `null` | Nearest matched data point, or `null` when no point was hit. |
| `Series` | `ChartSeries?` | `null` | Series that owns the matched data point, or `null` when no series was matched. |
| `SeriesIndex` | `int` | `-1` | Zero-based index of the matched series in the chart. The default `-1` indicates that no series has been selected. |
| `DistancePx` | `double` | `double.MaxValue` | Euclidean distance, in canvas pixels, from the pointer to the matched data point. |
| `CanvasX` | `float` | `0` | Canvas X coordinate, in pixels, of the matched data point. |
| `CanvasY` | `float` | `0` | Canvas Y coordinate, in pixels, of the matched data point. |
| `TooltipText` | `string` | `string.Empty` | Formatted tooltip text ready for rendering. |
| `Region` | `ChartRegion` | `ChartRegion.Outside` | Logical chart region containing the pointer coordinate. It is set independently of whether a data-point hit was found. |

The static `TooltipHitResult.Miss` field provides a shared result whose `IsHit` value is `false`.

## `ViewportState`

Tracks zoom factors, pan offsets, and visible data ranges for an interactive chart viewport.

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `ZoomX` | `double` | `1.0` | Horizontal zoom factor. `1.0` means no zoom and values greater than `1.0` zoom in. Valid values range from `0.01` through `100`; assigning a value outside that range throws `ArgumentOutOfRangeException`. |
| `ZoomY` | `double` | `1.0` | Vertical zoom factor. `1.0` means no zoom and values greater than `1.0` zoom in. Valid values range from `0.01` through `100`; assigning a value outside that range throws `ArgumentOutOfRangeException`. |
| `PanX` | `double` | `0` | Horizontal pan offset in data-coordinate units. |
| `PanY` | `double` | `0` | Vertical pan offset in data-coordinate units. |
| `VisibleXRange` | `(double Min, double Max)` | `(0, 0)` | Visible X-axis range in data coordinates after applying the current zoom and pan. |
| `VisibleYRange` | `(double Min, double Max)` | `(0, 0)` | Visible Y-axis range in data coordinates after applying the current zoom and pan. |
| `IsDefault` | `bool` (read-only) | `true` | Reports whether both zoom factors are `1.0` and both pan offsets are `0`. Visible ranges do not affect this value. |

`Reset()` restores both zoom factors to `1.0`, both pan offsets to `0`, and both visible ranges to their default tuple value. `Clone()` creates a new `ViewportState` with the same zoom, pan, and visible-range values.
