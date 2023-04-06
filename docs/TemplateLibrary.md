# TemplateLibrary

The `TemplateLibrary` class serves as a centralized repository for managing and accessing reusable `ChartTemplate` configurations within the `skiasharp-chart-engine`. It facilitates the storage, retrieval, and instantiation of charts based on pre-defined templates, ensuring consistent visual and data-binding styles across different chart components.

## API

### TemplateLibrary()
Initializes a new instance of the `TemplateLibrary` class.

### ChartTemplate GetTemplate(string templateName)
Retrieves a specific `ChartTemplate` by its name.
- `templateName`: The unique identifier of the template.
- Returns: The requested `ChartTemplate`.
- Throws: `KeyNotFoundException` if no template matches the provided name.

### Dictionary<string, ChartTemplate> GetAllTemplates()
Retrieves all available templates stored in the library.
- Returns: A dictionary where keys are template names and values are `ChartTemplate` objects.

### List<ChartTemplate> GetTemplatesByCategory(string category)
Filters and retrieves all templates belonging to a specific category.
- `category`: The category string to filter by.
- Returns: A list of `ChartTemplate` objects associated with the specified category.

### void AddTemplate(ChartTemplate template)
Registers a new `ChartTemplate` in the library.
- `template`: The `ChartTemplate` to be added.
- Throws: `ArgumentNullException` if the template is null.

### Chart CreateChartFromTemplate(string templateName, object data)
Creates a new `Chart` instance based on an existing template and provides the data context.
- `templateName`: The name of the template to use.
- `data`: The data object to bind to the template.
- Returns: A newly created `Chart` object.
- Throws: `KeyNotFoundException` if the template name does not exist.

### IEnumerable<string> ListTemplateNames()
Returns a list of all template names currently registered in the library.
- Returns: An `IEnumerable<string>` of template names.

### int GetTemplateCount()
Returns the total number of templates currently stored in the library.
- Returns: An integer representing the count.

## Usage

```csharp
// Example 1: Retrieving a template and creating a chart
var library = new TemplateLibrary();
// Assume 'templateName' and 'myData' are pre-defined
try {
    var myChart = library.CreateChartFromTemplate("BarChartTemplate", myData);
    // Use myChart...
} catch (KeyNotFoundException) {
    // Handle case where template does not exist
}
```

```csharp
// Example 2: Adding a new template
var library = new TemplateLibrary();
var newTemplate = new ChartTemplate("MyCustomChart", "Sales");
library.AddTemplate(newTemplate);

var count = library.GetTemplateCount();
Console.WriteLine($"Total templates: {count}");
```

## Notes

- **Thread Safety:** The `TemplateLibrary` implementation is not guaranteed to be thread-safe for concurrent read/write operations. If multiple threads access the library simultaneously, appropriate synchronization primitives (e.g., `lock` statements) must be utilized to prevent race conditions during `AddTemplate` operations.
- **Data Integrity:** The `AddTemplate` method does not automatically verify if a template name is already in use. Overwriting existing template entries is not explicitly prevented; developers should perform existence checks using `ListTemplateNames` if name collisions must be avoided.
- **Resource Management:** `CreateChartFromTemplate` does not automatically dispose of the provided `data` object; callers remain responsible for managing the lifecycle of the data source injected into the resulting chart.
