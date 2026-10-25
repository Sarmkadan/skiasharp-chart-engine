# GaugeChartRenderer

Specialized renderer for gauge charts. It draws a semicircular dial with three colored zones, a needle that points at the first data point's value, a numeric value label, and min/max labels at the ends of the arc.

> **Build note:** `Rendering/GaugeChartRenderer.cs` is currently excluded from compilation. The project file contains `<Compile Remove="Rendering/**/*.cs" />`, so this class is not part of the built assembly and cannot be instantiated from the library as-is. This document describes the source file as written.

## Overview

The `GaugeChartRenderer` class implements the `IChartRenderer` interface and draws onto a SkiaSharp `SKCanvas`. It uses only the first series and its first data point. The value is normalized against the series' Y-axis range, so the needle position and the zone layout always share the same scale.

## Key Features

- Semicircular arc from -180° to 0° (left to right)
- Three fixed color zones covering the lower 33%, middle 33%, and upper 34% of the range
- Needle with a filled tip, with the value clamped to the range
- Centered numeric value label, formatted according to the range size
- Min and max labels at the arc's left and right ends
- Optional title and subtitle, using `ChartConstants.TitleFontSize` and `ChartConstants.SubtitleFontSize`
- Logging through `ILogger<GaugeChartRenderer>`; rendering errors are caught and logged rather than thrown

## Usage

```csharp
var renderer = new GaugeChartRenderer(logger);
renderer.Render(canvas, chart, bounds);
```

The chart must contain at least one series with at least one data point. The range comes from the series:

```csharp
var series = new ChartSeries
{
    Name = "CPU",
    YAxisMin = 0,    // defaults to 0 when null
    YAxisMax = 100,  // defaults to 100 when null
    DataPoints = new List<DataPoint> { new DataPoint { Value = 72 } }
};
```

## API

### Constructor

```csharp
public GaugeChartRenderer(ILogger<GaugeChartRenderer> logger)
```

**Parameters**
- `logger` (`ILogger<GaugeChartRenderer>`): Logger for warnings, information, debug output, and errors. Must not be `null`.

**Exceptions**
- `ArgumentNullException`: Thrown when `logger` is `null`.

---

### `public void Render(SKCanvas canvas, Chart chart, SKRect bounds)`

Renders the gauge onto `canvas` within `bounds`. This method is the `IChartRenderer` implementation.

**Parameters**
- `canvas` (`SKCanvas`): The canvas to draw on. If `null`, the method logs a warning and returns.
- `chart` (`Chart`): The chart model. If `null`, or if `Series` is `null` or empty, the method logs a warning and returns.
- `bounds` (`SKRect`): The drawing area within the canvas.

**Behavior**
- Returns silently when the first series has no data points.
- Logs a warning and returns when `YAxisMin` is greater than or equal to `YAxisMax`.
- Any exception thrown during drawing is caught and logged as an error. It is not rethrown.

---

## Implementation Details

### Rendering Process

`Render` performs these steps in order:

1. **Validation.** Checks the canvas, chart, and series. Logs a warning and returns if any of them is missing.
2. **Title space.** Adds `TitleFontSize + 8` when `Configuration.Title` is set, and `SubtitleFontSize + 6` when `Configuration.Subtitle` is set. The gauge area starts below that height.
3. **Value and range.** Reads the first data point's `Value`. The range is `YAxisMin ?? 0` to `YAxisMax ?? 100`. If the minimum is not less than the maximum, it logs a warning and returns.
4. **Normalization.** Computes `(value - min) / (max - min)` and clamps it to `[0, 1]`. The needle uses this clamped value, but the value label shows the raw value.
5. **Geometry.** Sets the center to `(bounds.MidX, bounds.Bottom - 50)` and the radius to `min(width, height) / 2.5`.
6. **Drawing**, in this order:
   1. White background rectangle over the gauge area.
   2. Title and subtitle.
   3. Colored zones.
   4. Gray arc outline.
   5. Needle and tip.
   6. Value label.
   7. Min and max labels.

### Zones

`_drawGaugeZones` draws three stroked arc segments with an 8 px stroke width and semi-transparent colors (alpha 180):

| Zone | Range (fraction of arc) | Color (RGB) |
|------|-------------------------|-------------|
| Danger | 0% – 33% | (220, 80, 80) red |
| Warning | 33% – 66% | (255, 200, 80) yellow |
| Good | 66% – 100% | (100, 200, 100) green |

The zone boundaries are fixed at 0.33 and 0.66 of the arc. They do not depend on the series' data or configuration.

### Needle

The needle angle is `-180 + normalizedValue × 180` degrees. The shaft is a 4 px dark slate gray line from the center to a point at 80% of the radius (`NeedleLengthRatio`). A red filled circle with radius `NeedleWidth × 1.5` marks the tip.

### Value Label

The value is formatted with a format string chosen from the range size:

| Range (`max - min`) | Format | Example |
|---------------------|--------|---------|
| < 1 | `F2` | `0.42` |
| ≥ 1 and < 10 | `F1` | `4.2` |
| ≥ 10 | `F0` | `72` |

The text is 24 px bold Arial, drawn horizontally centered below the center point at `radius × 0.3`.

### Min and Max Labels

Min and max values are formatted with `F0`, in 12 px dim gray. The min label sits 10 px left of the arc's left end, and the max label sits 10 px right of its right end. Both are drawn at the center's y-coordinate.

### Title and Subtitle

Both are drawn horizontally centered in `bounds`, starting at `bounds.Top + 4`, using `Configuration.TextColor`. The title is bold. The subtitle is drawn `TitleFontSize + 2` pixels below the title, or at the same position as the title if no title is set.

## Limitations

- **Not compiled.** See the build note above.
- **First series and first point only.** Other series and data points are ignored.
- **Fixed zones.** Zone colors and thresholds are hard-coded and cannot be configured.
- **Fixed geometry.** The center offset (`50` px), radius divisor (`2.5`), and label offsets are constants. Very small bounds may place the gauge partly outside the visible area.
- **Hard-coded label formatting.** The value label size and font family are fixed, and labels are not scaled to the bounds.
- **Unused parameters.** `_drawGaugeArc` accepts `minValue` and `maxValue`, but does not use them.
- **Errors are swallowed.** Because exceptions are caught and logged, a failed render can leave a partially drawn canvas without an exception reaching the caller.

## Dependencies

- `IChartRenderer` (`Rendering/IChartRenderer.cs`)
- `Chart`, `ChartSeries`, and `DataPoint` (`Models/`)
- `ChartConstants` (`Constants/ChartConstants.cs`) for font sizes
- `Microsoft.Extensions.Logging` for `ILogger<T>`
- SkiaSharp for drawing

## See Also

- [PieChartRenderer](PieChartRenderer.md)
- [BarChartRenderer](BarChartRenderer.md)
- [LineChartRenderer](LineChartRenderer.md)
