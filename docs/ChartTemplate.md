# ChartTemplate

`ChartTemplate` is a serializable, reusable blueprint for constructing chart instances. It captures a predefined chart type, a base configuration, default series, and optional metadata, allowing consumers to stamp out consistently configured charts without repeating setup logic. The type also supports cloning and custom property bags for extensibility.

## API

### Constructors

**`ChartTemplate()`**  
Parameterless constructor. Initializes `TemplateId` to a new GUID string, sets `CreatedAt` to `DateTime.UtcNow`, and leaves `BaseConfiguration`, `DefaultSeries`, and optional fields at their defaults. Never throws.

**`ChartTemplate(string templateId, ChartType chartType, ChartConfiguration baseConfiguration, List<ChartSeries> defaultSeries, string? createdBy, Dictionary<string, object>? customProperties)`**  
Full constructor. Assigns every property from the supplied arguments. `CreatedAt` is set to `DateTime.UtcNow` regardless of arguments.  
- **Parameters**  
  `templateId`: unique identifier for the template; must not be null or empty.  
  `chartType`: the kind of chart this template produces.  
  `baseConfiguration`: the configuration applied to every chart created from this template; must not be null.  
  `defaultSeries`: the series collection preloaded into new charts; must not be null.  
  `createdBy`: optional creator tag.  
  `customProperties`: optional extensibility dictionary.  
- **Throws** `ArgumentNullException` when `templateId` is null or empty, or when `baseConfiguration` or `defaultSeries` is null.

### Properties

**`public string TemplateId`**  
Unique identifier for the template. Set during construction; consumers may read and write it directly.

**`public ChartType ChartType`**  
The chart type (e.g., Line, Bar, Pie) that this template is bound to. Read/write.

**`public ChartConfiguration BaseConfiguration`**  
The configuration object (axes, legends, styling, etc.) applied as the foundation for every chart created from the template. Read/write.

**`public List<ChartSeries> DefaultSeries`**  
A mutable list of series that will be copied into new charts. Read/write.

**`public DateTime CreatedAt`**  
UTC timestamp set when the template was instantiated. Read-only after construction (set only in constructors).

**`public string? CreatedBy`**  
Optional label identifying who or what process created the template. Nullable; read/write.

**`public Dictionary<string, object>? CustomProperties`**  
An optional dictionary for arbitrary metadata or extension data. Nullable; read/write.

### Methods

**`public Chart CreateChartFromTemplate()`**  
Produces a new `Chart` instance configured from this template.  
- **Behavior**: creates a deep copy of `BaseConfiguration`, clones each series in `DefaultSeries`, assigns the `ChartType`, and returns the fully wired `Chart`.  
- **Returns**: a new `Chart` ready for rendering or further customization.  
- **Throws**: may throw if `BaseConfiguration` or `DefaultSeries` contain objects that fail during deep copy (e.g., non-serializable types in the property graph).

**`public ChartTemplate Clone()`**  
Creates a deep copy of the current template.  
- **Returns**: a new `ChartTemplate` with a freshly generated `TemplateId`, a copied `BaseConfiguration`, a cloned `DefaultSeries` list, and duplicated `CustomProperties` (if present). `CreatedAt` is set to the current UTC time. `CreatedBy` is preserved from the original.  
- **Throws**: may throw for the same deep-copy limitations as `CreateChartFromTemplate`.

**`public override string ToString()`**  
Returns a string representation, typically combining `TemplateId` and `ChartType` for diagnostics and logging. Format is not contractual but stable across calls on the same instance.

## Usage

### Example 1: Defining a template and creating a chart

```csharp
var config = new ChartConfiguration
{
    Title = "Monthly Sales",
    ShowLegend = true
};

var series = new List<ChartSeries>
{
    new ChartSeries { Name = "Revenue", Data = new double[] { 1200, 1500, 900 } }
};

var template = new ChartTemplate(
    templateId: "sales-monthly",
    chartType: ChartType.Line,
    baseConfiguration: config,
    defaultSeries: series,
    createdBy: "ReportingTeam",
    customProperties: new Dictionary<string, object> { ["department"] = "Finance" }
);

Chart chart = template.CreateChartFromTemplate();
// chart now has Line type, the title "Monthly Sales", and the Revenue series.
```

### Example 2: Cloning and modifying a template

```csharp
var original = new ChartTemplate
{
    TemplateId = "orig-1",
    ChartType = ChartType.Bar,
    BaseConfiguration = new ChartConfiguration { Title = "Default" },
    DefaultSeries = new List<ChartSeries>()
};

ChartTemplate clone = original.Clone();
clone.TemplateId = "variant-1";
clone.BaseConfiguration.Title = "Variant View";
clone.DefaultSeries.Add(new ChartSeries { Name = "NewData" });

// original remains unchanged; clone can be used independently.
```

## Notes

- **Mutability**: All properties except `CreatedAt` are read/write. Changes to `BaseConfiguration` or `DefaultSeries` on an existing template affect future calls to `CreateChartFromTemplate` but do not retroactively alter already-created charts.
- **Deep copy limitations**: `CreateChartFromTemplate` and `Clone` rely on deep-copy mechanisms that assume serializable or cloneable objects inside `BaseConfiguration` and `DefaultSeries`. Non-cloneable types (e.g., delegates, streams) will cause runtime exceptions.
- **TemplateId uniqueness**: The parameterless constructor and `Clone` both auto-generate a GUID-based `TemplateId`. When using the full constructor, callers must enforce uniqueness if required by the application.
- **Thread safety**: This type is not thread-safe. Concurrent reads and writes to mutable properties (especially `BaseConfiguration`, `DefaultSeries`, and `CustomProperties`) or concurrent calls to `CreateChartFromTemplate`/`Clone` must be externally synchronized.
- **Null handling**: `CreatedBy` and `CustomProperties` are nullable by design. Code consuming templates should null-check before dereferencing `CustomProperties` or using `CreatedBy` in display paths.
- **ToString output**: The string returned by `ToString` is intended for human diagnostics and logging. It should not be parsed for programmatic decisions.
