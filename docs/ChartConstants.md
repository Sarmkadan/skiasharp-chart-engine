# ChartConstants

`ChartConstants` defines shared defaults and limits for chart configuration, rendering, caching, exporting, and animation. It is declared in the `SkiaSharpChartEngine.Constants` namespace.

## Dimensions

| Constant | Value | Controls |
| --- | ---: | --- |
| `DefaultChartWidth` | `800` | Default chart canvas width, in pixels, used by `ChartConfiguration` and `ChartEngineOptions`. |
| `DefaultChartHeight` | `600` | Default chart canvas height, in pixels, used by `ChartConfiguration` and `ChartEngineOptions`. |
| `MinimumChartWidth` | `200` | Smallest accepted chart width, in pixels. Configuration and request validation reject smaller widths. |
| `MinimumChartHeight` | `150` | Smallest accepted chart height, in pixels. Configuration and request validation reject smaller heights. |
| `MaximumChartWidth` | `4000` | Largest accepted chart width, in pixels, enforced by `ChartConfiguration` and request validation. |
| `MaximumChartHeight` | `4000` | Largest accepted chart height, in pixels, enforced by `ChartConfiguration` and request validation. |

## Margins and padding

| Constant | Value | Controls |
| --- | ---: | --- |
| `DefaultMarginTop` | `40` | Default space above the plot area, in pixels. It is also used by interactivity calculations to locate the plot area. |
| `DefaultMarginBottom` | `60` | Default space below the plot area, in pixels. It is also used by interactivity calculations to locate the plot area. |
| `DefaultMarginLeft` | `80` | Default space to the left of the plot area, in pixels. It is also used by interactivity calculations to locate the plot area. |
| `DefaultMarginRight` | `40` | Default space to the right of the plot area, in pixels. It is also used by interactivity calculations to locate the plot area. |
| `DefaultPadding` | `10` | Standard internal padding, in pixels, available to chart layout code. No production component currently references this constant directly. |

## Font sizes

Font-size values are passed to SkiaSharp as text sizes.

| Constant | Value | Controls |
| --- | ---: | --- |
| `TitleFontSize` | `24` | Chart title text size and the vertical space reserved for a title. |
| `SubtitleFontSize` | `16` | Chart subtitle text size and the vertical space reserved for a subtitle. |
| `LegendFontSize` | `12` | Legend entry text size. The general rendering service also uses it for legend text measurement and drawing. |
| `AxisLabelFontSize` | `11` | Axis title text size in the general chart rendering service. |
| `TickLabelFontSize` | `10` | Intended default size for axis tick labels. No production component currently references this constant directly. |

## Colors

Color values use hexadecimal RGB notation; when parsed for rendering they are fully opaque.

| Constant | Value | Controls |
| --- | --- | --- |
| `DefaultBackgroundColor` | `#FFFFFF` | Default chart canvas background color (white). |
| `DefaultGridColor` | `#E0E0E0` | Default grid-line color (light gray). |
| `DefaultAxisColor` | `#000000` | Default axis-line color (black). It is also the initial color assigned to a new `DataPoint`. |
| `DefaultTextColor` | `#333333` | Default chart text color (dark gray), including the fallback used by legend rendering. |

## Performance limits

| Constant | Value | Controls |
| --- | ---: | --- |
| `MaxDataPoints` | `100000` | Maximum number of data points accepted in one series by `ChartDataService`; it is also the default per-series limit in `ChartEngineOptions`. |
| `MaxSeries` | `50` | Maximum number of series that can be added to a `Chart`; it is also the default per-chart limit in `ChartEngineOptions`. |
| `CacheSize` | `100` | Default maximum number of entries held by `RenderCacheService` and the default cache-size option. This is an entry count, not a byte limit. |

## Export settings

| Constant | Value | Controls |
| --- | ---: | --- |
| `DefaultExportDPI` | `96` | Default export resolution in dots per inch for `ChartConfiguration` and `ExportOptions`. |
| `DefaultExportQuality` | `0.95f` | Default export quality factor for `ChartConfiguration` and `ExportOptions`, equivalent to 95 percent on a zero-to-one scale. |

## Animation defaults

| Constant | Value | Controls |
| --- | ---: | --- |
| `DefaultAnimationDurationMs` | `500` | Default chart animation duration, in milliseconds, used by `ChartConfiguration`. |
| `DefaultAnimationFramerate` | `60` | Intended default animation frame rate, in frames per second. No production component currently references this constant directly. |

## Usage

Constants can be referenced directly when creating configuration objects:

```csharp
using SkiaSharpChartEngine.Constants;
using SkiaSharpChartEngine.Models;

var configuration = new ChartConfiguration
{
    Width = ChartConstants.DefaultChartWidth,
    Height = ChartConstants.DefaultChartHeight,
    BackgroundColor = ChartConstants.DefaultBackgroundColor,
    AnimationDurationMs = ChartConstants.DefaultAnimationDurationMs
};
```

These are compile-time constants. Changing a value in `ChartConstants` changes the compiled default for consumers that reference it, but does not override values explicitly supplied through configuration.
