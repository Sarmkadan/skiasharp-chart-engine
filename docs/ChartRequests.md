# Chart request models

`API/Requests/ChartRequests.cs` defines the request objects used to create, update, and render charts. Each request exposes an `IsValid(out string? errorMessage)` method except where noted otherwise. Validation is explicit: assigning properties or deserializing JSON does not call `IsValid` automatically.

The JSON examples below use the camel-case property names normally used by ASP.NET Core. Enum values are shown as their numeric values, which is the default `System.Text.Json` representation unless the host configures a string-enum converter. For `ChartType`, `0` is `LineChart`, `1` is `BarChart`, `2` is `PieChart`, and `3` is `HeatmapChart`.

## `CreateChartRequest`

Represents a request to create a chart.

| Property | C# type | Required | Description |
| --- | --- | --- | --- |
| `Title` | `string` | Yes | Chart title. The C# `required` modifier requires initialization by C# callers, while `IsValid` performs the runtime content checks. |
| `ChartType` | `ChartType` | No | Chart type. If omitted, the enum default is `LineChart` (`0`). |
| `Configuration` | `ChartConfiguration?` | No | Display and rendering configuration. |
| `Series` | `List<ChartSeries>?` | No | Chart data series. |

Validation rules, evaluated in this order:

1. `Title` must not be `null`, empty, or whitespace-only. Failure message: `Chart title is required and cannot be empty`.
2. `Title` must contain no more than 200 characters. Failure message: `Chart title must not exceed 200 characters`.

`IsValid` does not validate `ChartType`, `Configuration`, individual series, or data points.

```json
{
  "title": "Quarterly revenue",
  "chartType": 0,
  "configuration": {
    "width": 1200,
    "height": 700,
    "title": "Revenue by quarter",
    "xAxisLabel": "Quarter",
    "yAxisLabel": "Revenue (USD)",
    "showLegend": true
  },
  "series": [
    {
      "name": "2026",
      "color": "#1F77B4",
      "seriesType": 0,
      "dataPoints": [
        { "x": 1, "y": 125000, "label": "Q1" },
        { "x": 2, "y": 142000, "label": "Q2" }
      ]
    }
  ]
}
```

## `UpdateChartRequest`

Represents a partial chart update. All properties are optional, but at least one usable update field must be supplied.

| Property | C# type | Required | Description |
| --- | --- | --- | --- |
| `Title` | `string?` | No | Replacement chart title. |
| `Configuration` | `ChartConfiguration?` | No | Replacement configuration. |
| `Series` | `List<ChartSeries>?` | No | Replacement data series. |

Validation rules, evaluated in this order:

1. The request must contain a non-empty `Title`, a non-null `Configuration`, or a non-null `Series` list containing at least one item. Failure message: `At least one field must be provided for update`.
2. A non-empty `Title` must contain no more than 200 characters. Failure message: `Chart title must not exceed 200 characters`.

An empty string is treated as an absent title. A whitespace-only title is treated as present and passes validation when its length is at most 200. An empty `Series` list alone is not an update. `IsValid` does not validate the contents of `Configuration` or `Series`.

```json
{
  "title": "Revised quarterly revenue",
  "configuration": {
    "width": 1400,
    "height": 800,
    "title": "Revenue by quarter",
    "showLegend": false
  }
}
```

## `RenderChartRequest`

Represents a request to render one saved chart.

| Property | C# type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `ChartId` | `string` | Yes | None | Identifier of the chart to render. The C# `required` modifier requires initialization by C# callers. |
| `Width` | `int` | No | `800` | Output width in pixels. |
| `Height` | `int` | No | `600` | Output height in pixels. |
| `Dpi` | `float` | No | `96` | Output resolution in dots per inch. |

Validation rules, evaluated in this order:

1. `ChartId` must not be `null`, empty, or whitespace-only. Failure message: `Chart ID is required`.
2. `Width` must be from 100 through 4000 pixels, inclusive. Failure message: `Width must be between 100 and 4000 pixels`.
3. `Height` must be from 100 through 4000 pixels, inclusive. Failure message: `Height must be between 100 and 4000 pixels`.
4. `Dpi` must be from 72 through 600, inclusive. Failure message: `DPI must be between 72 and 600`.

```json
{
  "chartId": "chart-7f32",
  "width": 1600,
  "height": 900,
  "dpi": 144
}
```

## `BatchRenderRequest`

Represents a request to render several saved charts using common settings.

| Property | C# type | Required | Description |
| --- | --- | --- | --- |
| `ChartIds` | `List<string>` | Yes | Chart identifiers to render. The C# `required` modifier requires initialization by C# callers. |
| `RenderSettings` | `RenderChartRequest?` | No | Common width, height, and DPI settings. The nested `ChartId` is not used to select the batch items; `ChartIds` supplies them. |

Validation rules, evaluated in this order:

1. `ChartIds` must be non-null and contain at least one item. Failure message: `At least one chart ID must be provided`.
2. `ChartIds` may contain no more than 100 items. Failure message: `Maximum 100 charts can be rendered in a single batch`.

`IsValid` checks only the list count. It does not reject null, empty, whitespace-only, or duplicate values inside `ChartIds`, and it does not call `RenderSettings.IsValid`. When `RenderSettings` is omitted, the export controller uses width `800`, height `600`, and DPI `96`.

```json
{
  "chartIds": [
    "chart-7f32",
    "chart-a184"
  ],
  "renderSettings": {
    "chartId": "unused-for-batch-selection",
    "width": 1920,
    "height": 1080,
    "dpi": 144
  }
}
```

## Calling validation

All four `IsValid` methods return `true` and set `errorMessage` to `null` when validation succeeds. On failure, they return `false` and set `errorMessage` to the first failed rule's message.

```csharp
if (!request.IsValid(out var errorMessage))
{
    // Return or report errorMessage.
}
```
