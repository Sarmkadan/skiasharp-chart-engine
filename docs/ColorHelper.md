# ColorHelper

The `ColorHelper` class provides a suite of static utility methods for managing color representations, generating default color palettes, and performing basic color manipulations such as lightening, darkening, and format conversion between hexadecimal and RGB strings within the `skiasharp-chart-engine` project.

## API

### GetColorAtIndex(int index)
Retrieves a hexadecimal color string from the default palette corresponding to the specified zero-based index. If the index is greater than or equal to the number of available colors, the index is wrapped using a modulo operation.
*   **Parameters:** `int index` (The zero-based index of the desired color).
*   **Returns:** `string` (The hexadecimal color string).

### GetDefaultColorPalette()
Returns a collection of the predefined hexadecimal color strings used as the default palette for chart series rendering.
*   **Parameters:** None.
*   **Returns:** `List<string>` (A list of hexadecimal color strings).

### HexToRgb(string hex)
Converts a hexadecimal color string (e.g., "#RRGGBB") to an RGB color string format (e.g., "rgb(r, g, b)").
*   **Parameters:** `string hex` (The hex color string to convert).
*   **Returns:** `string` (The equivalent RGB color string).
*   **Throws:** `ArgumentException` if the provided hex string is null, empty, or not in a valid format.

### RgbToHex(string rgb)
Converts an RGB color string (e.g., "rgb(r, g, b)") to a hexadecimal string format (e.g., "#RRGGBB").
*   **Parameters:** `string rgb` (The RGB color string to convert).
*   **Returns:** `string` (The equivalent hexadecimal color string).
*   **Throws:** `ArgumentException` if the provided RGB string is null, empty, or not in a valid format.

### LightenColor(string hex, float factor)
Lightens the provided hexadecimal color by the specified factor.
*   **Parameters:**
    *   `string hex` (The hex color string to lighten).
    *   `float factor` (The intensity of the lightening effect, ranging from 0.0 to 1.0).
*   **Returns:** `string` (The new hexadecimal color string).
*   **Throws:** `ArgumentException` if the hex string is invalid or the factor is out of range.

### DarkenColor(string hex, float factor)
Darkens the provided hexadecimal color by the specified factor.
*   **Parameters:**
    *   `string hex` (The hex color string to darken).
    *   `float factor` (The intensity of the darkening effect, ranging from 0.0 to 1.0).
*   **Returns:** `string` (The new hexadecimal color string).
*   **Throws:** `ArgumentException` if the hex string is invalid or the factor is out of range.

### IsValidHexColor(string hex)
Determines if the provided string is a valid hexadecimal color representation.
*   **Parameters:** `string hex` (The string to validate).
*   **Returns:** `bool` (True if the string is a valid hex color, otherwise false).

## Usage

```csharp
// Retrieve a color for a series based on its index
int seriesIndex = 0;
string seriesColor = ColorHelper.GetColorAtIndex(seriesIndex);

// Create a hover effect by lightening a color
string baseColor = "#FF5733";
string hoverColor = ColorHelper.LightenColor(baseColor, 0.2f);
```

## Notes

*   **Thread Safety:** As all methods within `ColorHelper` are static and operate without modifying shared state, they are thread-safe and can be called concurrently from multiple threads.
*   **Input Validation:** Methods requiring hex strings (`HexToRgb`, `LightenColor`, `DarkenColor`) will throw an `ArgumentException` if the provided input does not strictly conform to valid hexadecimal color formats.
*   **Factor Range:** The `factor` parameter in `LightenColor` and `DarkenColor` is expected to be a value between 0.0 and 1.0. Values outside this range may lead to undefined behavior or exceptions.
