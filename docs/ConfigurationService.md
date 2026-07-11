# ConfigurationService

`ConfigurationService` manages the persistence and retrieval of `ChartConfiguration` objects within the skiasharp-chart-engine. It provides methods to store, load, delete, and enumerate named configurations, as well as to create new configurations from predefined templates. The service acts as the central registry for chart styling and layout presets, allowing consumers to save user-defined settings or revert to system defaults.

## API

### ConfigurationService

```csharp
public ConfigurationService()
```

Initializes a new instance of the service. The constructor prepares the underlying storage mechanism and ensures that the default configuration is available.

- **Exceptions**: May throw an `InvalidOperationException` if the storage backend cannot be initialised.

---

### GetDefaultConfiguration

```csharp
public ChartConfiguration GetDefaultConfiguration()
```

Returns the built-in default `ChartConfiguration`. This configuration is immutable from the caller’s perspective and serves as the baseline for all templates and user-defined configurations.

- **Returns**: A `ChartConfiguration` instance populated with engine defaults.
- **Exceptions**: None.

---

### GetConfiguration

```csharp
public ChartConfiguration GetConfiguration(string name)
```

Retrieves a previously saved configuration by its unique name.

- **Parameters**:
  - `name` (string): The case-insensitive name of the configuration to retrieve.
- **Returns**: The matching `ChartConfiguration`.
- **Exceptions**:
  - `ArgumentNullException`: `name` is `null`.
  - `KeyNotFoundException`: No configuration exists with the given `name`.

---

### SaveConfiguration

```csharp
public void SaveConfiguration(string name, ChartConfiguration configuration)
```

Persists a `ChartConfiguration` under the specified name. If a configuration with the same name already exists, it is overwritten.

- **Parameters**:
  - `name` (string): The unique, case-insensitive name to store the configuration under.
  - `configuration` (ChartConfiguration): The configuration object to persist. Must not be `null`.
- **Exceptions**:
  - `ArgumentNullException`: `name` or `configuration` is `null`.
  - `ArgumentException`: `name` is empty or contains only whitespace.

---

### DeleteConfiguration

```csharp
public void DeleteConfiguration(string name)
```

Removes a saved configuration. The default configuration cannot be deleted.

- **Parameters**:
  - `name` (string): The case-insensitive name of the configuration to remove.
- **Exceptions**:
  - `ArgumentNullException`: `name` is `null`.
  - `InvalidOperationException`: Attempted to delete the reserved default configuration.
  - `KeyNotFoundException`: No configuration exists with the given `name`.

---

### ListConfigurations

```csharp
public IEnumerable<string> ListConfigurations()
```

Enumerates the names of all currently saved configurations, excluding the default configuration.

- **Returns**: A lazy enumeration of configuration names. The order is stable but unspecified.
- **Exceptions**: None.

---

### CreateConfigurationFromTemplate

```csharp
public ChartConfiguration CreateConfigurationFromTemplate(string templateName)
```

Creates a new `ChartConfiguration` by cloning a named template. The returned configuration is detached from storage and can be modified freely before being saved with `SaveConfiguration`.

- **Parameters**:
  - `templateName` (string): The case-insensitive name of an existing configuration to use as the template.
- **Returns**: A deep copy of the template `ChartConfiguration`.
- **Exceptions**:
  - `ArgumentNullException`: `templateName` is `null`.
  - `KeyNotFoundException`: No configuration exists with the given `templateName`.

---

### ConfigurationExists

```csharp
public bool ConfigurationExists(string name)
```

Checks whether a configuration with the specified name is currently stored.

- **Parameters**:
  - `name` (string): The case-insensitive name to check.
- **Returns**: `true` if a configuration with the given `name` exists; otherwise `false`.
- **Exceptions**:
  - `ArgumentNullException`: `name` is `null`.

## Usage

### Example 1: Saving and restoring a user configuration

```csharp
var service = new ConfigurationService();

// Obtain the default and customise it
var userConfig = service.GetDefaultConfiguration();
userConfig.Title.Text = "Monthly Sales";
userConfig.Background.Color = SKColors.WhiteSmoke;

// Save under a user-defined name
service.SaveConfiguration("sales-light", userConfig);

// Later, retrieve and apply
if (service.ConfigurationExists("sales-light"))
{
    var restored = service.GetConfiguration("sales-light");
    chart.ApplyConfiguration(restored);
}
```

### Example 2: Creating from a template and managing the registry

```csharp
var service = new ConfigurationService();

// Start with a saved baseline
service.SaveConfiguration("dark-theme", BuildDarkTheme());

// Create a variant without mutating the original
var variant = service.CreateConfigurationFromTemplate("dark-theme");
variant.Axis.LabelColor = SKColors.Gray;

service.SaveConfiguration("dark-theme-muted", variant);

// List all user configurations
foreach (var name in service.ListConfigurations())
{
    Console.WriteLine($"Available: {name}");
}

// Clean up obsolete configurations
service.DeleteConfiguration("dark-theme");
```

## Notes

- **Case insensitivity**: All configuration names are treated case-insensitively. Saving “MyChart” and “mychart” refers to the same entry.
- **Default configuration protection**: The default configuration is reserved and cannot be deleted or overwritten through `SaveConfiguration`. Attempts to do so will raise an exception.
- **Template cloning**: `CreateConfigurationFromTemplate` performs a deep copy. Modifications to the returned object do not affect the source template or any other stored configuration.
- **Enumeration stability**: `ListConfigurations` returns names in an unspecified but consistent order for the lifetime of the service instance. Callers should not rely on alphabetical or insertion order.
- **Thread safety**: Instance methods of `ConfigurationService` are not thread-safe. Concurrent calls to `SaveConfiguration`, `DeleteConfiguration`, or `GetConfiguration` from multiple threads must be synchronised externally to avoid race conditions on the underlying storage.
- **Empty names**: Supplying a whitespace-only string for `name` parameters results in an `ArgumentException`. Always validate or trim user input before calling the API.
