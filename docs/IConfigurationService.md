# IConfigurationService

`IConfigurationService` defines the contract for managing named `ChartConfiguration` objects and creating starter configurations from chart-type templates in the `skiasharp-chart-engine`. It covers retrieving a default configuration, saving, loading, deleting, and listing named configurations, creating a configuration from a `ChartType` template, and checking whether a named configuration exists.

The engine registers `ConfigurationService` as a singleton in `Configuration/ServiceCollectionExtensions.cs`, both in the full registration and in `AddSkiaSharpChartEngineMinimal`. `ChartEngine` resolves `IConfigurationService` from the service provider and exposes its default and template operations through its own facade. `TemplateController` also depends on it. To use a different storage mechanism, implement this interface and register your implementation in its place.

**Namespace:** `SkiaSharpChartEngine.Services`
**Declaration:** `public interface IConfigurationService`
**Source:** `Services/IConfigurationService.cs`

## API

### Defaults and templates

### `ChartConfiguration GetDefaultConfiguration()`
Returns a new, unnamed configuration with the default property values of `ChartConfiguration`. Each call returns a fresh instance, so changes to the result do not affect later calls.
- **Returns:** A new `ChartConfiguration` instance.

### `ChartConfiguration CreateConfigurationFromTemplate(ChartType chartType)`
Creates a new configuration pre-populated for the given chart type. The built-in templates are:

| `ChartType` | Title | Notable settings |
|---|---|---|
| `LineChart` | Line Chart | Linear X and Y axes, grid and legend on, animation enabled |
| `BarChart` | Bar Chart | Categorical X axis, linear Y axis, grid and legend on, `MarginBottom = 80` |
| `PieChart` | Pie Chart | Legend on, grid and axis labels off, 600 x 600 size |
| `HeatmapChart` | Heatmap | Categorical X and Y axes, grid and legend on |
| `AreaChart` | Area Chart | Linear X and Y axes, grid and legend on, animation enabled |
| `ScatterChart` | Scatter Plot | Linear X and Y axes, grid and legend on |
| `ColumnChart` | Column Chart | Categorical X axis, linear Y axis, grid and legend on |

Any other `ChartType` value falls back to `GetDefaultConfiguration()`.
- **Parameters:** `chartType` - The chart type to create a template for.
- **Returns:** A new `ChartConfiguration` instance. It is not stored by the service. Use `SaveConfiguration` to persist it under a name.

### Named configurations

### `ChartConfiguration GetConfiguration(string configName)`
Returns a copy of the named configuration.
- **Parameters:** `configName` - The name the configuration was saved under.
- **Returns:** A clone of the stored `ChartConfiguration`. Changes to the returned object do not affect the stored copy.
- **Exceptions:**
  - `ArgumentNullException` - `configName` is `null`, empty, or whitespace.
  - `ResourceNotFoundException` - No configuration with that name exists.

### `void SaveConfiguration(string configName, ChartConfiguration configuration)`
Validates the configuration and stores a copy of it under the given name. If the name already exists, the stored configuration is replaced. This includes the built-in names listed below.
- **Parameters:**
  - `configName` - The name to store the configuration under.
  - `configuration` - The configuration to store. The service stores a clone, so later changes to the original object have no effect.
- **Exceptions:**
  - `ArgumentNullException` - `configName` is `null`, empty, or whitespace, or `configuration` is `null`.
  - Any exception thrown by `ChartConfiguration.Validate()` if the configuration is invalid. In that case nothing is stored.

### `void DeleteConfiguration(string configName)`
Removes the named configuration. If no configuration has that name, the call does nothing.
- **Parameters:** `configName` - The name of the configuration to remove.
- **Exceptions:** `ArgumentNullException` - `configName` is `null`, empty, or whitespace.

### `IEnumerable<string> ListConfigurations()`
Returns the names of all stored configurations, sorted with the default string comparer.
- **Returns:** A list of configuration names. The list is empty if none are stored. The built-in names are always present unless they have been deleted.

### `bool ConfigurationExists(string configName)`
Checks whether a configuration with the given name is stored.
- **Parameters:** `configName` - The name to check.
- **Returns:** `true` if a configuration with that name is stored; otherwise `false`. Returns `false` for `null`, empty, or whitespace names and does not throw.

## Built-in configurations

`ConfigurationService` stores three configurations when it is constructed:

| Name | Created from |
|---|---|
| `default_line` | `CreateConfigurationFromTemplate(ChartType.LineChart)` |
| `default_bar` | `CreateConfigurationFromTemplate(ChartType.BarChart)` |
| `default_pie` | `CreateConfigurationFromTemplate(ChartType.PieChart)` |

These names can be overwritten with `SaveConfiguration` and removed with `DeleteConfiguration`.

## Usage

```csharp
IConfigurationService configs = serviceProvider.GetRequiredService<IConfigurationService>();

// Start from a template, customize it, and store it under a name.
ChartConfiguration config = configs.CreateConfigurationFromTemplate(ChartType.LineChart);
config.Title = "Monthly Revenue";
configs.SaveConfiguration("revenue_line", config);

// Load a copy later. Modifying it does not change the stored version.
if (configs.ConfigurationExists("revenue_line"))
{
    ChartConfiguration loaded = configs.GetConfiguration("revenue_line");
}

foreach (string name in configs.ListConfigurations())
{
    Console.WriteLine(name);
}

configs.DeleteConfiguration("revenue_line");
```

## Notes

- Stored configurations are kept in memory in a `Dictionary<string, ChartConfiguration>`. They are lost when the process ends. Use a different implementation if you need persistence.
- The stored dictionary is not synchronized. `ConfigurationService` does not lock around reads and writes, so concurrent calls that modify configurations must be coordinated by the caller.
- Both `SaveConfiguration` and `GetConfiguration` work on copies, so callers never share a live instance with the store.
