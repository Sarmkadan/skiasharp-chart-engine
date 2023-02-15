# CreateChartRequest

The `CreateChartRequest` class serves as a comprehensive data transfer object used to initiate the generation of a chart within the `skiasharp-chart-engine`. It encapsulates all necessary parameters, including identification, layout dimensions, data series, and rendering configuration, required for the engine to produce the requested visual output.

## API

| Member | Type | Description |
| :--- | :--- | :--- |
| `Title` | `string` | **Required.** The display title for the chart. |
| `ChartType` | `ChartType` | Specifies the type of chart to be rendered (e.g., Bar, Line, Pie). |
| `Configuration` | `ChartConfiguration?` | Optional configuration overrides or specific settings for the chart styling and behavior. |
| `Series` | `List<ChartSeries>?` | Optional collection of data series to be plotted on the chart. |
| `IsValid` | `bool` | Indicates whether the request object contains all required fields and passes basic validation rules. |
| `ChartId` | `string` | **Required.** Unique identifier for the specific chart instance. |
| `Width` | `int` | The requested width of the output image in pixels. |
| `Height` | `int` | The requested height of the output image in pixels. |
| `Dpi` | `float` | The dots-per-inch (DPI) setting for the rendering output. |
| `ChartIds` | `List<string>` | **Required.** A collection of identifiers associated with this request, used for batch operations or multi-chart references. |
| `RenderSettings` | `RenderChartRequest?` | Optional specific rendering settings to apply during the chart generation process. |

## Usage

### Simple Chart Request

```csharp
var request = new CreateChartRequest
{
    ChartId = "sales-chart-001",
    ChartIds = new List<string> { "sales-chart-001" },
    Title = "Annual Sales 2026",
    ChartType = ChartType.Bar,
    Width = 800,
    Height = 600,
    Dpi = 96f
};
```

### Detailed Chart Request

```csharp
var request = new CreateChartRequest
{
    ChartId = "detailed-trend-001",
    ChartIds = new List<string> { "trend-group-a", "detailed-trend-001" },
    Title = "Market Trends",
    ChartType = ChartType.Line,
    Width = 1200,
    Height = 800,
    Dpi = 144f,
    Configuration = new ChartConfiguration { Theme = "Dark" },
    Series = new List<ChartSeries> { /* populated data */ },
    RenderSettings = new RenderChartRequest { AntiAlias = true }
};
```

## Notes

- **Validation:** The `IsValid` property should be checked after instantiating and populating the object but before passing it to any processing methods to ensure all required fields (`Title`, `ChartId`, `ChartIds`) are correctly set.
- **Thread Safety:** `CreateChartRequest` is a plain data object and is not thread-safe. Instances should not be shared across threads during modification. If shared access is required, external synchronization mechanisms must be implemented.
- **Initialization:** As this class contains `required` properties, it must be initialized using object initializer syntax or a constructor that satisfies all required member constraints to compile successfully.
