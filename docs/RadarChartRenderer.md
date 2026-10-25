# RadarChartRenderer

Renderer for radar (spider) charts. Draws one axis per category radiating from the centre, concentric polygon gridlines at 25 %, 50 %, 75 % and 100 % of the radius, and the data as a filled, translucent polygon with a stroked outline.

> **Build status:** `Rendering/**/*.cs` is excluded from compilation in `skiasharp-chart-engine.csproj` (`<Compile Remove="Rendering/**/*.cs" />`). This class is therefore not part of the built assembly today. It can only be used once that exclusion is removed or the file is moved out of `Rendering/`.

## Overview

The `RadarChartRenderer` class implements the `IChartRenderer` interface. It draws only the **first** series of the chart; any additional series are ignored. The data values are expected to be pre-normalised to the range `0..1`, where `1` is the outer ring.

## Properties

The class exposes no public properties. Its only state is the logger and a set of private constants (see [Constants](#constants)).

## Constructor

```csharp
public RadarChartRenderer(ILogger<RadarChartRenderer> logger)
```

Parameters:
- `logger`: Logger instance for rendering diagnostics. Must not be `null`; otherwise an `ArgumentNullException` is thrown.

## Methods

### Public Methods

| Method | Description |
|--------|-------------|
| `Render(SKCanvas canvas, Chart chart, SKRect bounds)` | Renders the radar chart onto the provided canvas within the specified bounds. Implements `IChartRenderer.Render`. |

### Private Methods

| Method | Description |
|--------|-------------|
| `_renderTitleAndSubtitle` | Draws the chart title (bold) and subtitle, centred horizontally at the top of `bounds`. Returns early when both are empty. |
| `_getSeriesColor` | Returns a colour from a fixed eight-colour palette by index, wrapping with modulo. |

## Rendering Details

### Error Handling

`Render` wraps all of its work in a `try/catch (Exception)`. Errors are logged at `Error` level and **not rethrown**. This includes the `ChartEngineException` raised for fewer than three categories, so callers receive no exception from `Render` for invalid input. Check the log output if a chart appears blank.

### Early Exits

`Render` returns without drawing (logging a warning only for the first case) when:

1. `canvas`, `chart`, `chart.Series` is `null`, or `chart.Series` is empty. A warning is logged.
2. The first series is `null`, or has no data points. Nothing is drawn, including the title.

### Layout Calculation

1. **Title/Subtitle Space**: If a title is set, `ChartConstants.TitleFontSize + 8` pixels are reserved at the top. If a subtitle is set, `ChartConstants.SubtitleFontSize + 6` more pixels are reserved.
2. **Plot Bounds**: The plot area is `bounds` with its top moved down by the reserved title height.
3. **Centre**: The centre of the plot area (`MidX`, `MidY`).
4. **Radius**: `min(width, height) / 3`. The divisor of 3 leaves room for category labels outside the outer ring.
5. **Angles**: `360 / categoryCount` degrees between axes. Axis `i` sits at angle `-90 + i * step`, so the first category is at the top and the rest follow clockwise in screen coordinates.

### Drawing Order

1. **Title and subtitle**, drawn at `bounds.Top + 4` (not the reduced plot bounds).
2. **Gridlines**: four closed polygons at radii `radius * level / 4` for `level = 1..4`. Light grey, 0.5 px stroke.
3. **Axes**: one line per category from the centre to the outer radius. Grey, 1 px stroke.
4. **Data polygon**: the first series. Each vertex is at `radius * clamp(value, 0, 1)` along its axis. The fill uses the series colour at alpha 80 (about 31 %). The outline is 2 px in the full series colour.
5. **Category labels**: placed at `radius + 20` along each axis, centred on the anchor point, drawn at 11 pt black.

### Value Handling

- Values are cast from `double` to `float` and clamped to `[0, 1]`. Values outside this range are silently clipped; no scaling is applied.
- A missing label (`DataPoint.Label` is `null`) is replaced with `Category {i + 1}`.

### Colours

- Series colour: the palette is `RoyalBlue`, `OrangeRed`, `ForestGreen`, `Gold`, `Violet`, `Turquoise`, `Salmon`, `SeaGreen`. Only index `0` is used, so the first series is always `RoyalBlue`.
- Title and subtitle use `chart.Configuration.TextColor`, parsed with `SKColor.Parse`. An invalid value throws inside `_renderTitleAndSubtitle`, which is caught by `Render`'s handler.
- Gridlines, axes and category labels use fixed colours. They do not follow `TextColor`.

### Constants

| Constant | Value | Purpose |
|----------|-------|---------|
| `LabelDistance` | `20f` | Distance from the outer radius to the category label anchor. |
| `AxisStrokeWidth` | `1f` | Stroke width for axis lines. |
| `OutlineStrokeWidth` | `2f` | Stroke width for the data polygon outline. |
| `FillAlpha` | `80` | Alpha for the data polygon fill (0–255). |

The gridline count (four rings) and the label font size (11) are inline literals, not named constants.

## Known Limitations

- **Not compiled**: see the build status note above.
- **First series only**: additional series in `chart.Series` are not drawn and no legend is rendered.
- **No value scaling**: values must be pre-normalised to `0..1`. Out-of-range values are clipped rather than rescaled.
- **Minimum three categories**: with fewer, the renderer logs an error and draws nothing, not even the title.
- **Labels can overflow**: the reserved space is only `LabelDistance` beyond the outer ring. Long labels near the left or right edge may extend past `bounds`.
- **Unused local**: `currentAngle` is declared in `Render` but never read.
- **Fixed palette and label colours**: only the title and subtitle honour `ChartConfiguration.TextColor`.

## Related

- `IChartRenderer`: the interface this class implements.
- `PieChartRenderer`: the behaviour of the first series is modelled on this renderer.
- `ChartConstants`: source of `TitleFontSize` and `SubtitleFontSize`.
- `ChartConfiguration`: provides `Title`, `Subtitle` and `TextColor`.
