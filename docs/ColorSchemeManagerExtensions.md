# ColorSchemeManagerExtensions

Provides extension methods for `ColorSchemeManager` in SkiaSharp.Chart.Engine. The helpers add alpha-aware color lookup, gradient generation, scheme existence checks, and fallback scheme resolution on top of the manager's basic `GetColor`, `GetScheme`, and `ListAvailableSchemes` members, so callers can work with color schemes without repeating that logic.

Namespace: `SkiaSharpChartEngine.Utilities`

## API

### `GetColorWithAlpha`
Gets a color from a scheme and optionally replaces its alpha channel.

- **Parameters**
  - `this ColorSchemeManager manager`: The manager to read from.
  - `string schemeName`: Name of the color scheme.
  - `int index`: Index of the color in the scheme. The lookup wraps with modulo, so any index maps to a valid color.
  - `byte? alpha = null`: Alpha value (0-255). If `null`, the color's original alpha is kept.

- **Return Value**
  An `SKColor` with the same RGB components as the scheme color. The alpha is either the supplied value or the original.

- **Exceptions**
  - `ArgumentNullException` if `manager` or `schemeName` is `null`.
  - `ArgumentException` if `schemeName` is empty.

- **Notes**
  An unknown scheme name falls back to the `default` scheme (with a logged warning) because `GetColor` resolves schemes through `GetScheme`.

---

### `GetGradientColors`
Builds a linear gradient between two colors of a scheme.

- **Parameters**
  - `this ColorSchemeManager manager`: The manager to read from.
  - `string schemeName`: Name of the color scheme.
  - `int startIndex`: Index of the first color.
  - `int endIndex`: Index of the last color.
  - `int steps = 10`: Number of colors in the result, including both endpoints. Must be at least 2.

- **Return Value**
  An array of `steps` colors. The first element is the start color and the last is the end color. Each channel (red, green, blue, and alpha) is interpolated linearly. Values are truncated to `byte`, not rounded.

- **Exceptions**
  - `ArgumentException` if `schemeName` is empty.
  - `ArgumentOutOfRangeException` if `steps` is less than 2.

- **Notes**
  The method does not check `manager` for `null`. Passing `null` throws `NullReferenceException` rather than `ArgumentNullException`.

---

### `HasScheme`
Checks whether a scheme with the given name is registered.

- **Parameters**
  - `this ColorSchemeManager manager`: The manager to query.
  - `string schemeName`: Name of the scheme to look up.

- **Return Value**
  `true` if a matching scheme exists; otherwise, `false`. A `null`, empty, or whitespace name returns `false`. The comparison ignores case, matching the manager's own lookup.

- **Exceptions**
  None thrown by the method itself. A `null` `manager` causes a `NullReferenceException`.

---

### `GetSchemeOrFallback`
Returns the requested scheme, or a fallback scheme when the requested one is not registered.

- **Parameters**
  - `this ColorSchemeManager manager`: The manager to read from.
  - `string schemeName`: Name of the preferred scheme.
  - `string fallbackSchemeName = "default"`: Name of the scheme to use if `schemeName` is not registered.

- **Return Value**
  The `ColorScheme` registered under `schemeName`, or the scheme for `fallbackSchemeName` when that name is not registered.

- **Exceptions**
  - `ArgumentNullException` if `schemeName` is `null`.
  - `ArgumentException` if `schemeName` or `fallbackSchemeName` is empty.

- **Notes**
  If `fallbackSchemeName` is also missing, the manager's `GetScheme` logs a warning and returns its `default` scheme. Passing a `null` `manager` causes a `NullReferenceException`.

## Usage

### Example 1: Semi-transparent series color

```csharp
var manager = serviceProvider.GetRequiredService<ColorSchemeManager>();

// Keep the scheme's color at index 2, but make it 50% opaque.
SKColor fill = manager.GetColorWithAlpha("material", 2, 128);

// Keep the original alpha.
SKColor line = manager.GetColorWithAlpha("material", 2);
```

### Example 2: Gradient for a heatmap legend

```csharp
// Ten colors from the first to the last color of the viridis scheme.
SKColor[] legend = manager.GetGradientColors("viridis", 0, 5, steps: 10);
```

### Example 3: Safe scheme selection

```csharp
// Use the requested scheme if it is registered; otherwise use the default scheme.
ColorScheme scheme = manager.GetSchemeOrFallback(userSelectedScheme);

if (manager.HasScheme(userSelectedScheme))
{
    // Scheme is registered; no fallback was needed.
}
```

## Related

- [ColorSchemeManager](ColorSchemeManager.md): Stores the color schemes these extensions operate on.
