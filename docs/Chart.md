# Chart

A `Chart` object in the SkiaSharp Chart Engine represents a single chart configuration, including its type, metadata, and one or more series of data points. It serves as the primary container for chart definitions, enabling serialization, templating, and rendering operations.

## API

### `public ChartType Type`

The type of chart (e.g., bar, line, pie) that determines how data is visualized. This property is immutable after construction and defines the rendering behavior of the chart.

### `public DateTime CreatedAt`

The timestamp indicating when the chart was first created. This value is set automatically upon instantiation and cannot be modified afterward.

### `public DateTime? ModifiedAt`

The timestamp indicating the last time the chart was modified, or `null` if it has never been modified. This value is updated automatically whenever a series is added, removed, or modified.

### `public string? CreatedBy`

The identifier (e.g., username or system account) of the entity that created the chart, or `null` if unknown. This value is set once during construction and remains immutable.

### `public bool IsTemplate`

Indicates whether the chart is a template that can be reused as a base for new charts. Templates do not contain data and are intended for structural reuse.

### `public Dictionary<string, object>? Tags`

A collection of key-value pairs used to annotate the chart for filtering, categorization, or metadata purposes. The dictionary may be `null` if no tags are assigned.

### `public Chart()`

Initializes a new instance of the `Chart` class with default values. The `Type` property must be set afterward before use.

### `public Chart(ChartType type)`

Initializes a new instance of the `Chart` class with the specified chart type. The `CreatedAt` timestamp is set to the current UTC time.

### `public Chart(ChartType type, Dictionary<string, object>? tags)`

Initializes a new instance of the `Chart` class with the specified chart type and tags. The `CreatedAt` timestamp is set to the current UTC time.

### `public void AddSeries(ChartSeries series)`

Adds a new series to the chart. The series must not be `null`; otherwise, an `ArgumentNullException` is thrown. If a series with the same name already exists, it will be replaced.

**Parameters:**
- `series`: The `ChartSeries` instance to add.

**Throws:**
- `ArgumentNullException`: If `series` is `null`.

### `public void RemoveSeries(ChartSeries series)`

Removes the specified series from the chart. If the series does not exist, the operation has no effect. The `series` parameter must not be `null`; otherwise, an `ArgumentNullException` is thrown.

**Parameters:**
- `series`: The `ChartSeries` instance to remove.

**Throws:**
- `ArgumentNullException`: If `series` is `null`.

### `public void RemoveSeriesByName(string name)`

Removes the series with the specified name from the chart. If no series with the given name exists, the operation has no effect. The `name` parameter must not be `null` or whitespace; otherwise, an `ArgumentException` is thrown.

**Parameters:**
- `name`: The name of the series to remove.

**Throws:**
- `ArgumentException`: If `name` is `null` or whitespace.

### `public ChartSeries? GetSeriesByName(string name)`

Retrieves the series with the specified name, or `null` if no such series exists. The `name` parameter must not be `null` or whitespace; otherwise, an `ArgumentException` is thrown.

**Parameters:**
- `name`: The name of the series to retrieve.

**Returns:**
- The `ChartSeries` instance if found; otherwise, `null`.

**Throws:**
- `ArgumentException`: If `name` is `null` or whitespace.

### `public int GetSeriesCount()`

Returns the total number of series currently associated with the chart.

**Returns:**
- The count of series.

### `public int GetTotalDataPoints()`

Returns the sum of data points across all series in the chart. If no series are present, returns `0`.

**Returns:**
- The total number of data points.

### `public void ClearAllSeries()`

Removes all series from the chart. After this operation, `GetSeriesCount()` will return `0`.

### `public (double minX, double maxX, double minY, double maxY) GetDataBounds()`

Calculates and returns the minimum and maximum X and Y values across all data points in all series. If no data points exist, returns `(0, 0, 0, 0)`.

**Returns:**
- A tuple containing `(minX, maxX, minY, maxY)`.

### `public bool ValidateForRendering()`

Validates whether the chart is in a state suitable for rendering. Returns `true` if the chart has at least one series with at least one data point and a valid `Type`; otherwise, returns `false`.

**Returns:**
- `true` if the chart can be rendered; otherwise, `false`.

### `public Chart Clone()`

Creates a deep copy of the chart, including all series and their data points. The new chart will have the same `Type`, `CreatedAt`, `CreatedBy`, `IsTemplate`, and `Tags` as the original, but with a new `ModifiedAt` timestamp set to the current UTC time.

**Returns:**
- A new `Chart` instance that is a deep copy of the original.

### `public override string ToString()`

Returns a human-readable string representation of the chart, including its type, series count, and data point count.

**Returns:**
- A `string` describing the chart.

## Usage

### Example 1: Creating and Populating a Chart
