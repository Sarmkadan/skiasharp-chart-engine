# DataPointExtensions

`DataPointExtensions` provides a set of static utility methods for performing geometric and statistical operations on `DataPoint` instances and collections of `DataPoint`. These methods simplify common tasks such as measuring proximity, applying affine transformations (offset and scale), computing bounding boxes, and calculating coordinate averages.

## API

### GetDistance

```csharp
public static double GetDistance(this DataPoint point1, DataPoint point2)
```

Calculates the Euclidean distance between two points.

**Parameters:**
- `point1` — The first `DataPoint` (the extension method target).
- `point2` — The second `DataPoint`.

**Return Value:** A `double` representing the straight-line distance between `point1` and `point2`.

**Throws:** Does not throw under normal circumstances. Both arguments are value types or non-null references; if a null `DataPoint` is supplied, a `NullReferenceException` will occur at the call site.

---

### IsNear

```csharp
public static bool IsNear(this DataPoint point, DataPoint other, double tolerance)
```

Determines whether two points are within a specified distance of each other.

**Parameters:**
- `point` — The reference `DataPoint`.
- `other` — The `DataPoint` to compare against.
- `tolerance` — The maximum allowable distance for the points to be considered “near.”

**Return Value:** `true` if the Euclidean distance between `point` and `other` is less than or equal to `tolerance`; otherwise `false`.

**Throws:** Does not throw. A negative `tolerance` will always return `false` because distance is non-negative.

---

### Offset (single point)

```csharp
public static DataPoint Offset(this DataPoint point, double dx, double dy)
```

Translates a single point by a given delta.

**Parameters:**
- `point` — The original `DataPoint`.
- `dx` — The horizontal offset to apply.
- `dy` — The vertical offset to apply.

**Return Value:** A new `DataPoint` whose X and Y coordinates are `point.X + dx` and `point.Y + dy`.

**Throws:** Does not throw.

---

### Scale (single point)

```csharp
public static DataPoint Scale(this DataPoint point, double scaleX, double scaleY)
```

Scales the coordinates of a single point by given factors.

**Parameters:**
- `point` — The original `DataPoint`.
- `scaleX` — The multiplier for the X coordinate.
- `scaleY` — The multiplier for the Y coordinate.

**Return Value:** A new `DataPoint` with coordinates `(point.X * scaleX, point.Y * scaleY)`.

**Throws:** Does not throw.

---

### Offset (list)

```csharp
public static List<DataPoint> Offset(this List<DataPoint> points, double dx, double dy)
```

Applies a uniform translation to every point in a list.

**Parameters:**
- `points` — The source list of `DataPoint` instances.
- `dx` — The horizontal offset applied to each point.
- `dy` — The vertical offset applied to each point.

**Return Value:** A new `List<DataPoint>` where each element is the result of offsetting the corresponding source point by `(dx, dy)`. The original list is not modified.

**Throws:** `ArgumentNullException` if `points` is `null`.

---

### Scale (list)

```csharp
public static List<DataPoint> Scale(this List<DataPoint> points, double scaleX, double scaleY)
```

Applies uniform scaling to every point in a list.

**Parameters:**
- `points` — The source list of `DataPoint` instances.
- `scaleX` — The X-axis scaling factor applied to each point.
- `scaleY` — The Y-axis scaling factor applied to each point.

**Return Value:** A new `List<DataPoint>` where each element has coordinates `(source.X * scaleX, source.Y * scaleY)`. The original list is not modified.

**Throws:** `ArgumentNullException` if `points` is `null`.

---

### GetBounds

```csharp
public static (double minX, double maxX, double minY, double maxY) GetBounds(this IEnumerable<DataPoint> points)
```

Computes the axis-aligned bounding box that encloses all provided points.

**Parameters:**
- `points` — A collection of `DataPoint` instances.

**Return Value:** A tuple `(minX, maxX, minY, maxY)` representing the minimum and maximum X and Y coordinates found in the collection.

**Throws:** `ArgumentNullException` if `points` is `null`. Behavior for an empty collection is implementation-specific (typically returns a zeroed or degenerate bounding box).

---

### GetAverageX

```csharp
public static double GetAverageX(this IEnumerable<DataPoint> points)
```

Calculates the arithmetic mean of the X coordinates in a collection of points.

**Parameters:**
- `points` — A collection of `DataPoint` instances.

**Return Value:** The average X value as a `double`.

**Throws:** `ArgumentNullException` if `points` is `null`. `InvalidOperationException` (or `DivideByZeroException`) if the collection is empty.

---

### GetAverageY

```csharp
public static double GetAverageY(this IEnumerable<DataPoint> points)
```

Calculates the arithmetic mean of the Y coordinates in a collection of points.

**Parameters:**
- `points` — A collection of `DataPoint` instances.

**Return Value:** The average Y value as a `double`.

**Throws:** `ArgumentNullException` if `points` is `null`. `InvalidOperationException` (or `DivideByZeroException`) if the collection is empty.

## Usage

### Example 1: Hit-testing and transforming a point

```csharp
var cursor = new DataPoint(102, 205);
var target = new DataPoint(100, 200);

// Check if the cursor is within 5 units of the target
if (cursor.IsNear(target, tolerance: 5.0))
{
    // Snap the cursor position by offsetting it toward the target
    double dx = target.X - cursor.X;
    double dy = target.Y - cursor.Y;
    var snapped = cursor.Offset(dx, dy);
    Console.WriteLine($"Snapped to ({snapped.X}, {snapped.Y})");
}
```

### Example 2: Normalizing and analyzing a dataset

```csharp
List<DataPoint> rawPoints = GetSensorReadings();

// Compute the bounding box of the raw data
var bounds = rawPoints.GetBounds();
double width = bounds.maxX - bounds.minX;
double height = bounds.maxY - bounds.minY;

// Scale all points to fit within a unit square, then center around origin
var normalized = rawPoints
    .Scale(1.0 / width, 1.0 / height)
    .Offset(-0.5, -0.5);

double avgX = normalized.GetAverageX();
double avgY = normalized.GetAverageY();
Console.WriteLine($"Normalized centroid: ({avgX:F3}, {avgY:F3})");
```

## Notes

- **Empty collections:** `GetBounds` on an empty collection may return a tuple with default values (e.g., `(0,0,0,0)`). `GetAverageX` and `GetAverageY` will throw when the collection is empty. Always guard against empty sequences before calling averaging methods.
- **Null arguments:** All methods that accept `IEnumerable<DataPoint>` or `List<DataPoint>` throw `ArgumentNullException` when passed `null`. The single-point overloads operate on value types or non-null references; passing a null `DataPoint` where a struct is expected will not compile, and where a class is expected will cause a `NullReferenceException`.
- **Immutability:** `Offset` and `Scale` (both single-point and list overloads) always produce new instances or new lists. The original data is never modified in place.
- **Thread safety:** All methods are static and operate on immutable inputs or produce new output collections without shared state. They are safe to call concurrently from multiple threads provided the input collections are not being mutated during the call.
- **Floating-point precision:** `IsNear` uses a `double` tolerance; very small or very large coordinate values may be subject to floating-point rounding. For exact equality checks, prefer a tolerance of `0.0` only when coordinates are known to be integral or exactly representable.
- **Scale factors:** Negative scale factors will mirror points across the respective axis. A scale factor of zero will collapse all coordinates along that axis to zero.
