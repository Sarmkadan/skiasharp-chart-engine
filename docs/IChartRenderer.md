# IChartRenderer

Common contract for all chart renderers in SkiaSharpChartEngine. Each renderer draws one chart type onto a SkiaSharp canvas through a single `Render` method.

> **Build status:** `Rendering/**/*.cs` is excluded from compilation in `skiasharp-chart-engine.csproj` (`<Compile Remove="Rendering/**/*.cs" />`). `IChartRenderer` and every renderer listed below are therefore not part of the built assembly today.

## Overview

`IChartRenderer` lives in the `SkiaSharpChartEngine.Rendering` namespace, in `Rendering/IChartRenderer.cs`. It has one member, `Render`. Renderers are stateless with respect to the chart: they receive the chart model on each call and draw into the canvas they are given.

## Interface

```csharp
public interface IChartRenderer
{
    void Render(SKCanvas canvas, Chart chart, SKRect bounds);
}
```

### Methods

| Method | Description |
|--------|-------------|
| `Render(SKCanvas canvas, Chart chart, SKRect bounds)` | Renders the chart onto `canvas` within `bounds`. |

### Parameters

- `canvas` (`SKCanvas`): the SkiaSharp canvas to draw on.
- `chart` (`Chart`): the chart model holding the series, data points and configuration (see `Models/Chart.cs`).
- `bounds` (`SKRect`): the drawing area within the canvas. Implementations lay out titles, legends and plot areas inside this rectangle.

### Return Value

None. Rendering is void; output goes to `canvas`.

## Implementations

The following classes in `Rendering/` implement `IChartRenderer` directly:

| Class | Chart type | Documentation |
|-------|------------|---------------|
| `AreaChartRenderer` | Area chart | [AreaChartRenderer.md](AreaChartRenderer.md) |
| `BarChartRenderer` | Bar chart | [BarChartRenderer.md](BarChartRenderer.md) |
| `BubbleChartRenderer` | Bubble chart | [BubbleChartRenderer.md](BubbleChartRenderer.md) |
| `CandlestickChartRenderer` | Candlestick (OHLC) chart | [CandlestickChartRenderer.md](CandlestickChartRenderer.md) |
| `GaugeChartRenderer` | Gauge chart | [GaugeChartRenderer.md](GaugeChartRenderer.md) |
| `HeatmapRenderer` | Heatmap | [HeatmapRenderer.md](HeatmapRenderer.md) |
| `LineChartRenderer` | Line chart | [LineChartRenderer.md](LineChartRenderer.md) |
| `PieChartRenderer` | Pie chart | [PieChartRenderer.md](PieChartRenderer.md) |
| `RadarChartRenderer` | Radar (spider) chart | [RadarChartRenderer.md](RadarChartRenderer.md) |
| `ScatterPlotRenderer` | Scatter plot | [ScatterPlotRenderer.md](ScatterPlotRenderer.md) |

`PieChartRenderer` declares the interface, but its public `Render` takes an extra optional `float innerRadiusRatio` parameter and no three-argument overload exists. Under the C# rules for implicit interface implementation, this does not satisfy `IChartRenderer.Render`. The file is excluded from the build, so this does not break `dotnet build` today, but it would fail to compile once the exclusion is removed unless a three-argument `Render` overload is added.

### Not an implementation

`ChartRenderer` (`Rendering/ChartRenderer.cs`) uses `IChartRenderer` but does not implement it. It is a dispatcher that keeps a `Dictionary<string, IChartRenderer>` keyed by chart type (`RegisterRenderer`) and calls `Render` on the matching renderer.

Other `Render` methods in `Rendering/` (`LegendRenderer`, `TooltipOverlay`) have their own signatures and do not implement this interface.

## Usage

Resolve a renderer by chart type and draw:

```csharp
IChartRenderer renderer = new LineChartRenderer(logger);
renderer.Render(canvas, chart, new SKRect(0, 0, 800, 600));
```

Register a custom renderer with the dispatcher:

```csharp
chartRenderer.RegisterRenderer("sparkline", new MySparklineRenderer());
```

## Implementing a New Renderer

1. Create the class in `Rendering/` (or a folder that is compiled in the project) with the `SkiaSharpChartEngine.Rendering` namespace.
2. Implement `Render(SKCanvas canvas, Chart chart, SKRect bounds)` with exactly this signature.
3. Return early on `null` arguments or empty series, and keep layout inside `bounds`.
4. Register the renderer with the chart-type key the caller will use.

## Related

- `ChartRenderer` (`Rendering/ChartRenderer.cs`): dispatcher that selects an `IChartRenderer` by chart type.
- [Chart](Chart.md): the chart model passed to `Render`.
