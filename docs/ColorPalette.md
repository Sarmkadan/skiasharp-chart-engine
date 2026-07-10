# ColorPalette

A `ColorPalette` is a utility class used to manage and retrieve a sequence of named colors. It is primarily intended for charting libraries—such as `skiasharp-chart-engine`—where consistent, reusable color schemes are required for rendering data visualizations. The class supports color cycling, lookup by index, and predefined palette generation.

## API

### `public string Name`

Gets the name of the color palette.

- **Type**: `string`
- **Access**: Read-only
- **Remarks**: Set via the constructor. Cannot be modified after instantiation.

---

### `public ColorPalette`

Initializes a new instance of the `ColorPalette` class with a given name and a sequence of color values.

- **Signature**: `public ColorPalette(string name, IEnumerable<string> colors)`
- **Parameters**:
  - `name` – A non-null, non-empty string identifying the palette.
  - `colors` – An enumerable of color strings in a recognized format (e.g., hex, named, RGB).
- **Throws**:
  - `ArgumentNullException` if `name` is `null`.
  - `ArgumentNullException` if `colors` is `null`.
  - `ArgumentException` if `name` is empty or whitespace.
  - `ArgumentException` if any color string is `null` or empty.
- **Remarks**: The constructor internally converts the input sequence into a list for indexed access.

---

### `public void AddColor(string color)`

Appends a new color to the end of the palette.

- **Signature**: `public void AddColor(string color)`
- **Parameters**:
  - `color` – A non-null, non-empty string representing a color.
- **Throws**:
  - `ArgumentNullException` if `color` is `null`.
  - `ArgumentException` if `color` is empty or whitespace.
- **Remarks**: The new color becomes the last in the sequence and will be returned by subsequent calls to `GetNextColor`.

---

### `public string GetColorAtIndex(int index)`

Retrieves the color at the specified zero-based index.

- **Signature**: `public string GetColorAtIndex(int index)`
- **Parameters**:
  - `index` – The zero-based position of the desired color.
- **Returns**: The color string at the given index.
- **Throws**:
  - `ArgumentOutOfRangeException` if `index` is less than zero or greater than or equal to `GetColorCount()`.
- **Remarks**: Colors are stored in insertion order; this method provides direct access without advancing the internal pointer used by `GetNextColor`.

---

### `public string GetNextColor()`

Retrieves the next color in the sequence, advancing the internal pointer.

- **Signature**: `public string GetNextColor()`
- **Returns**: The next color string in the sequence.
- **Throws**:
  - `InvalidOperationException` if the palette contains no colors (`GetColorCount() == 0`).
- **Remarks**: The internal pointer wraps around to the start upon reaching the end. This method is intended for sequential color cycling in rendering loops.

---

### `public int GetColorCount()`

Returns the total number of colors in the palette.

- **Signature**: `public int GetColorCount()`
- **Returns**: An integer representing the count of colors.
- **Remarks**: Always non-negative. Used to validate indices and detect empty palettes.

---
### `public override string ToString()`

Returns a string representation of the palette.

- **Signature**: `public override string ToString()`
- **Returns**: A string in the format `"Name (Count)"`, where `Name` is the palette name and `Count` is the number of colors.
- **Remarks**: Useful for debugging and logging.

---
### `public static ColorPalette CreateDefaultPalette()`

Creates a default color palette with a standard set of colors.

- **Signature**: `public static ColorPalette CreateDefaultPalette()`
- **Returns**: A new `ColorPalette` instance named `"Default"` containing a predefined sequence of colors.
- **Remarks**: The exact color sequence is implementation-defined and may change between versions.

---
### `public static ColorPalette CreateVibrantPalette()`

Creates a vibrant color palette optimized for high contrast and visibility.

- **Signature**: `public static ColorPalette CreateVibrantPalette()`
- **Returns**: A new `ColorPalette` instance named `"Vibrant"` with a vibrant color sequence.
- **Remarks**: Suitable for charts requiring strong visual differentiation.

---
### `public static ColorPalette CreatePastelPalette()`

Creates a pastel color palette with soft, muted tones.

- **Signature**: `public static ColorPalette CreatePastelPalette()`
- **Returns**: A new `ColorPalette` instance named `"Pastel"` with a pastel color sequence.
- **Remarks**: Ideal for dashboards or reports emphasizing readability over contrast.

---
### `public static ColorPalette CreateMonochromePalette()`

Creates a monochrome grayscale palette.

- **Signature**: `public static ColorPalette CreateMonochromePalette()`
- **Returns**: A new `ColorPalette` instance named `"Monochrome"` with shades of gray.
- **Remarks**: Useful for accessible visualizations or when color is not essential.

---
### `public static ColorPalette CreateOceanPalette()`

Creates a palette inspired by oceanic hues—blues and teals.

- **Signature**: `public static ColorPalette CreateOceanPalette()`
- **Returns**: A new `ColorPalette` instance named `"Ocean"` with blue-toned colors.
- **Remarks**: Commonly used in data visualizations related to water, weather, or temperature.

## Usage

### Example 1: Creating and Using a Custom Palette
