# CacheKeyBenchmarks

Utility class providing deterministic key generation methods for caching chart rendering artifacts in the SkiaSharp Chart Engine. These methods construct composite keys from chart configuration parameters to enable efficient cache lookups and avoid redundant rendering operations.

## API

### `BuildRenderKey`

Constructs a composite key representing the entire chart rendering configuration.

- **Parameters**:
  - `chartType`: The type of chart being rendered (e.g., "line", "bar").
  - `series`: Collection of series configurations.
  - `axes`: Collection of axis configurations.
  - `chartOptions`: Global chart rendering options.
- **Returns**: A string key uniquely identifying the rendered output for the given configuration.
- **Throws**: `ArgumentNullException` if any input parameter is `null`.

### `BuildSeriesKey`

Generates a key representing the series configuration within a chart.

- **Parameters**:
  - `series`: The series configuration object.
  - `chartType`: The type of chart containing the series.
- **Returns**: A string key uniquely identifying the series rendering.
- **Throws**: `ArgumentNullException` if `series` or `chartType` is `null`.

### `BuildAxisKey`

Creates a key for an axis configuration.

- **Parameters**:
  - `axis`: The axis configuration object.
  - `isXAxis`: Boolean indicating whether the axis is an X-axis.
- **Returns**: A string key uniquely identifying the axis rendering.
- **Throws**: `ArgumentNullException` if `axis` is `null`.

### `BuildConfigurationKey_Line`

Produces a key for line chart-specific configuration.

- **Parameters**:
  - `series`: The line series configuration.
  - `axes`: Collection of axis configurations.
  - `chartOptions`: Global chart rendering options.
- **Returns**: A string key uniquely identifying the line chart rendering configuration.
- **Throws**: `ArgumentNullException` if any input parameter is `null`.

### `BuildConfigurationKey_Bar`

Produces a key for bar chart-specific configuration.

- **Parameters**:
  - `series`: The bar series configuration.
  - `axes`: Collection of axis configurations.
  - `chartOptions`: Global chart rendering options.
- **Returns**: A string key uniquely identifying the bar chart rendering configuration.
- **Throws**: `ArgumentNullException` if any input parameter is `null`.

### `BuildCompositeKey`

Combines multiple keys into a single composite key using a consistent delimiter.

- **Parameters**:
  - `keys`: Array of string keys to combine.
- **Returns**: A single string key formed by joining the input keys with `|`.
- **Throws**: `ArgumentNullException` if `keys` is `null` or contains a `null` element.

## Usage
