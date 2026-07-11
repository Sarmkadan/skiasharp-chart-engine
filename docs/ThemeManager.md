# ThemeManager

The `ThemeManager` serves as the central registry and controller for managing visual styles within the `skiasharp-chart-engine`. It facilitates the registration of custom chart themes, enables runtime switching between active themes, and exposes the current theme's aesthetic configuration, including color schemes, typography, and line rendering properties.

## API

### Constructor
*   **`ThemeManager()`**: Initializes a new instance of the `ThemeManager` class.

### Methods
*   **`void RegisterTheme(ChartTheme theme)`**: Registers a new `ChartTheme` object for use within the manager.
*   **`ChartTheme GetTheme(string name)`**: Retrieves the `ChartTheme` associated with the specified name. Returns `null` if no theme exists with that name.
*   **`void SetCurrentTheme(string name)`**: Sets the theme identified by the given `name` as the active theme. Throws an `ArgumentException` if the specified theme is not found in the registry.
*   **`ChartTheme GetCurrentTheme()`**: Returns the currently active `ChartTheme`.
*   **`IEnumerable<string> GetAvailableThemes()`**: Returns an enumeration of the names of all registered themes.

### Properties
The following properties reflect the configuration of the currently active theme:

*   **`string Name`**: The name of the active theme.
*   **`SKColor BackgroundColor`**: The background color defined for the chart area.
*   **`SKColor ForegroundColor`**: The primary foreground color used for chart elements.
*   **`SKColor GridColor`**: The color used for grid lines.
*   **`SKColor AxisColor`**: The color used for axis lines and labels.
*   **`SKColor TextColor`**: The primary color used for text elements.
*   **`SKColor[] SeriesColors`**: An array of colors assigned to data series.
*   **`float FontSize`**: The font size used for chart labels and text.
*   **`float LineWidth`**: The default width used for rendered lines.

## Usage

### Registering and Switching Themes
```csharp
var manager = new ThemeManager();

// Create and register a custom theme
var darkTheme = new ChartTheme("Dark", SKColors.Black, SKColors.White, ...);
manager.RegisterTheme(darkTheme);

// Set the active theme
manager.SetCurrentTheme("Dark");
```

### Accessing Current Theme Properties
```csharp
var manager = new ThemeManager();
manager.SetCurrentTheme("Dark");

// Access styling properties directly from the manager
SKColor background = manager.BackgroundColor;
float fontSize = manager.FontSize;

Console.WriteLine($"Current theme: {manager.Name}");
```

## Notes

*   **Thread Safety**: The `ThemeManager` is not inherently thread-safe. Modifications to the theme registry or updates to the active theme should be synchronized externally if accessed by multiple threads simultaneously.
*   **Exception Handling**: The `SetCurrentTheme` method will throw an `ArgumentException` if the requested theme name has not been previously registered. Ensure themes are registered before attempting to set them as current.
*   **Property State**: The properties `BackgroundColor` through `LineWidth` are dynamically updated to reflect the configuration of the currently active theme. Accessing these properties while no theme has been set may result in default or null values, depending on the internal implementation of the active `ChartTheme` reference.
