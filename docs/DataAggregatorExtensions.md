# DataAggregatorExtensions

Static extension methods that provide common aggregation and filtering operations on sequences of `DataPoint` objects. These helpers simplify preprocessing steps before chart rendering, such as threshold filtering, statistical summarization, normalization, and ranking.

## API

### FilterByMinValue
```csharp
public static List<DataPoint> FilterByMinValue(this IEnumerable<DataPoint> source, double minValue)
```
**Purpose**  
Returns a new list containing only the points whose `Value` is greater than or equal to `minValue`.

**Parameters**  
- `source`: The sequence of `DataPoint` objects to filter.  
- `minValue`: The inclusive lower bound for the `Value` property.

**Return value**  
A `List<DataPoint>` with the filtered points. If no points satisfy the condition, an empty list is returned.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  

### FilterByMaxValue
```csharp
public static List<DataPoint> FilterByMaxValue(this IEnumerable<DataPoint> source, double maxValue)
```
**Purpose**  
Returns a new list containing only the points whose `Value` is less than or equal to `maxValue`.

**Parameters**  
- `source`: The sequence of `DataPoint` objects to filter.  
- `maxValue`: The inclusive upper bound for the `Value` property.

**Return value**  
A `List<DataPoint>` with the filtered points. If no points satisfy the condition, an empty list is returned.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  

### FilterByRange
```csharp
public static List<DataPoint> FilterByRange(this IEnumerable<DataPoint> source, double minValue, double maxValue)
```
**Purpose**  
Returns a new list containing only the points whose `Value` falls within the inclusive range `[minValue, maxValue]`.

**Parameters**  
- `source`: The sequence of `DataPoint` objects to filter.  
- `minValue`: The inclusive lower bound.  
- `maxValue`: The inclusive upper bound. Must be greater than or equal to `minValue`.

**Return value**  
A `List<DataPoint>` with the points that satisfy the range condition. Returns an empty list if none match.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  
- `ArgumentOutOfRangeException` if `minValue > maxValue`.  

### AggregateWithStatistics
```csharp
public static DataStatistics AggregateWithStatistics(this IEnumerable<DataPoint> source)
```
**Purpose**  
Computes basic statistical measures for the `Value` property of the points in `source`.

**Parameters**  
- `source`: The sequence of `DataPoint` objects to analyze.

**Return value**  
A `DataStatistics` instance containing `Count`, `Min`, `Max`, `Mean`, and `StandardDeviation`. If `source` is empty, `Count` is zero and the other properties are set to `double.NaN`.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  

### NormalizeValues
```csharp
public static List<DataPoint> NormalizeValues(this IEnumerable<DataPoint> source)
```
**Purpose**  
Scales the `Value` of each point to the range `[0, 1]` based on the minimum and maximum values found in `source`. The original points are not modified; new `DataPoint` instances are returned with normalized values.

**Parameters**  
- `source`: The sequence of `DataPoint` objects to normalize.

**Return value**  
A `List<DataPoint>` where each point's `Value` is `(value - min) / (max - min)`. If all values are identical (or the sequence contains a single element), all normalized values are set to `0.0`. If the source is empty, an empty list is returned.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  

### GetTopN
```csharp
public static List<DataPoint> GetTopN(this IEnumerable<DataPoint> source, int count)
```
**Purpose**  
Returns the `count` points with the highest `Value` values, sorted in descending order.

**Parameters**  
- `source`: The sequence of `DataPoint` objects to evaluate.  
- `count`: The number of top points to return. Must be non‑negative.

**Return value**  
A `List<DataPoint>` containing the top `count` points. If `count` exceeds the number of points, all points are returned sorted descending. If `count` is zero, an empty list is returned.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  
- `ArgumentOutOfRangeException` if `count` is negative.  

### GetBottomN
```csharp
public static List<DataPoint> GetBottomN(this IEnumerable<DataPoint> source, int count)
```
**Purpose**  
Returns the `count` points with the lowest `Value` values, sorted in ascending order.

**Parameters**  
- `source`: The sequence of `DataPoint` objects to evaluate.  
- `count`: The number of bottom points to return. Must be non‑negative.

**Return value**  
A `List<DataPoint>` containing the bottom `count` points. If `count` exceeds the number of points, all points are returned sorted ascending. If `count` is zero, an empty list is returned.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  
- `ArgumentOutOfRangeException` if `count` is negative.  

## Usage

### Example 1: Filtering and normalizing a dataset before rendering
```csharp
using SkiasharpChartEngine.Extensions;
using System.Collections.Generic;
using System.Linq;

// Assume `rawPoints` is populated elsewhere` is an IEnumerable<DataPoint>
IEnumerable<DataPoint> rawPoints = GetRawData();

// Keep only points with values between 10 and 90, then normalize to 0‑1 range
var processed = rawPoints
    .FilterByRange(10, 90)
    .NormalizeValues();

// `processed` can now be bound to a chart series
chart.Series[0].Points.AddRange(processed);
```

### Example 2: Computing statistics and highlighting extremes
```csharp
using SkiasharpChartEngine.Extensions;
using System.Linq;

// `sensorReadings` is an IEnumerable<DataPoint> from a data source
IEnumerable<DataPoint> sensorReadings = LoadSensorReadings();

// Obtain overall statistics
DataStatistics stats = sensorReadings.AggregateWithStatistics();

// Identify the three highest and three lowest readings for annotation
var topThree = sensorReadings.GetTopN(3);
var bottomThree = sensorReadings.GetBottomN(3);

// Use `stats` for tooltip info and `topThree`/`bottomThree` for visual markers
```

## Notes
- All extension methods operate on the input sequence without modifying it; they return new `List<DataPoint>` instances containing copies or newly created points where applicable.  
- If the source sequence is `null`, each method throws `ArgumentNullException` to fail fast.  
- The methods are thread‑safe with respect to their internal logic because they do not rely on mutable static state; however, the caller must ensure that the supplied `IEnumerable<DataPoint>` is not modified concurrently during enumeration.  
- For `NormalizeValues`, when all values are identical (resulting in a zero denominator), the method returns a list where every normalized value is `0.0` to avoid division‑by‑zero exceptions.  
- `AggregateWithStatistics` returns `double.NaN` for `Mean` and `StandardDeviation` when the source is empty; callers should check `Count` before relying on those values.  
- The ordering produced by `GetTopN` and `GetBottomN` is stable with respect to the original sequence only insofar as the underlying `OrderBy`/`OrderByDescending` implementation is stable; if stable ordering is required, apply `ThenBy` on a secondary key before calling these methods.
