# ColorSchemeManager

The `ColorSchemeManager` class provides a centralized mechanism for managing, retrieving, and manipulating color palettes within the SkiaSharp chart engine. It allows developers to register custom color schemes, access predefined palettes by name, and programmatically generate harmonious color variations—such as complementary, analogous, and triadic colors—based on a base color or existing scheme, facilitating consistent and aesthetic visual styling across chart components.

## API

*   **`ColorSchemeManager()`**: Initializes a new instance of the `ColorSchemeManager` class.
*   **`void RegisterScheme(string name, ColorScheme scheme)`**: Registers a new `ColorScheme` with the specified name for future retrieval.
*   **`ColorScheme GetScheme(string name)`**: Retrieves a registered `ColorScheme` by its name. Throws `KeyNotFoundException` if no scheme with the provided name is found.
*   **`SKColor GetColor(string schemeName, int index)`**: Retrieves an `SKColor` from a specific registered `ColorScheme` at the given index. Throws `ArgumentOutOfRangeException` if the index is out of bounds, or `KeyNotFoundException` if the scheme does not exist.
*   **`SKColor GetComplementary(SKColor color)`**: Calculates and returns the complementary `SKColor` for the provided input color.
*   **`SKColor[] GetAnalogous(SKColor color)`**: Generates and returns an array of `SKColor` values representing analogous colors based on the provided input color.
*   **`SKColor[] GetTriadic(SKColor color)`**: Generates and returns an array of `SKColor` values representing triadic colors based on the provided input color.
*   **`IEnumerable<string> ListAvailableSchemes`**: Gets an `IEnumerable<string>` containing the names of all currently registered color schemes.
*   **`string Name`**: Gets or sets the name associated with this `ColorSchemeManager` instance.
*   **`string Description`**: Gets or sets a description for this `ColorSchemeManager` instance.
*   **`SKColor[] Colors`**: Gets or sets the primary array of `SKColor` values defining the base palette.

## Usage

**Example 1: Registering and retrieving a custom color scheme**

```csharp
var manager = new ColorSchemeManager();
var customPalette = new ColorScheme(new SKColor[] { SKColors.LightBlue, SKColors.DarkBlue });

// Register the scheme
manager.RegisterScheme("CorporatePalette", customPalette);

// Retrieve and use the scheme
if (manager.ListAvailableSchemes.Contains("CorporatePalette"))
{
    var scheme = manager.GetScheme("CorporatePalette");
    SKColor primary = manager.GetColor("CorporatePalette", 0);
}
```

**Example 2: Generating harmonic colors from a base color**

```csharp
var manager = new ColorSchemeManager();
SKColor baseColor = SKColors.ForestGreen;

// Generate harmonic variations
SKColor complementary = manager.GetComplementary(baseColor);
SKColor[] analogous = manager.GetAnalogous(baseColor);
SKColor[] triadic = manager.GetTriadic(baseColor);
```

## Notes

*   **Thread Safety**: The `ColorSchemeManager` class is not inherently thread-safe. When multiple threads access and modify the same instance (e.g., registering new schemes while simultaneously retrieving them), external synchronization must be implemented to ensure data consistency.
*   **Error Handling**: Methods that retrieve schemes (`GetScheme`, `GetColor`) will throw a `KeyNotFoundException` if the specified scheme name has not been registered. Always validate the existence of a scheme using `ListAvailableSchemes` or handle the exception appropriately when accessing potentially missing schemes.
*   **Color Operations**: The `GetComplementary`, `GetAnalogous`, and `GetTriadic` methods rely on color space conversions. Ensure valid `SKColor` inputs are provided to avoid unexpected output.
