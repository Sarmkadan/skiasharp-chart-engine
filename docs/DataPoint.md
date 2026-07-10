# DataPoint

Represents a single point of data in a chart, encapsulating its visual state, optional styling hints, metadata, and temporal information. The type is intended to be lightweight and mutable, allowing charting logic to adjust appearance or attach custom data without allocating new objects for every update.

## API

### DataPointState State
Gets or sets the logical state of the point (e.g., normal, highlighted, selected). The value influences rendering decisions such as color or size. Assigning a value outside the defined enum range does not throw but may produce undefined visual results.

### double? CustomRadius
Gets or sets an optional radius override for the point when rendered as a shape. A null value indicates that the chart should use its default radius logic. Setting a negative value may cause an `ArgumentOutOfRangeException` if the implementation validates the radius.

### Dictionary<string, object>? Metadata
Gets or sets a user‑defined key/value store for attaching arbitrary data to the point. The dictionary can be null; assigning a non‑null dictionary stores the reference directly—mutations to the supplied dictionary after assignment will affect the point’s metadata. No exceptions are thrown for normal assignment.

### DateTime? Timestamp
Gets or sets an optional timestamp associated with the point. A null value indicates that no explicit time is attached. Assigning a value does not throw.

### DataPoint()
Initializes a new instance of the `DataPoint` class with all properties set to their default null/zero states.

### DataPoint(…)
Overloaded constructors provide alternative ways to instantiate a `DataPoint` with initial values for one or more of its properties. The exact parameter lists are defined in the source; each constructor assigns the supplied arguments to the corresponding members and leaves unspecified members at their defaults.

### DataPoint(…)
See the description above for the overloaded constructors.

### DataPoint(…)
See the description above for the overloaded constructors.

### public override string ToString()
Returns a string that represents the current `DataPoint`. The output includes the type name and the values of `State`, `CustomRadius`, `Timestamp`, and a count of metadata entries. This method does not accept parameters and does not throw exceptions under normal operation.

### public DataPoint Clone()
Creates a shallow copy of the current `DataPoint`. The returned instance has the same property values; reference‑type members such as `Metadata` point to the same object as the original. Modifying the cloned point’s value‑type properties does not affect the original, but changes to shared reference‑type members will be visible in both instances. This method does not throw exceptions.

## Usage

```csharp
// Create a point with a custom radius and attach metadata.
var point = new DataPoint
{
    CustomRadius = 6.5,
    Timestamp = DateTime.UtcNow,
    Metadata = new Dictionary<string, object>
    {
        ["source"] = "sensor‑42",
        ["unit"] = "°C"
    }
};

point.State = DataPointState.Highlighted;

// Obtain a readable representation for logging.
Console.WriteLine(point.ToString());
// Output: DataPoint { State=Highlighted, CustomRadius=6.5, Timestamp=2025-09-26T14:32:10Z, MetadataCount=2 }

// Clone the point to preserve the original while experimenting with visual changes.
var experiment = point.Clone();
experiment.CustomRadius = null; // revert to default radius
experiment.State = DataPointState.Normal;

// The original point remains unchanged.
Debug.Assert(point.CustomRadius == 6.5);
Debug.Assert(point.State == DataPointState.Highlighted);
```

```csharp
// Build a series of points from a CSV line and add ad‑hoc metadata.
string[] fields = line.Split(',');
var pt = new DataPoint
{
    Timestamp = DateTime.Parse(fields[0]),
    CustomRadius = double.TryParse(fields[1], out var r) ? (double?)r : null
};

// Store the raw line for later debugging without creating a separate class.
pt.Metadata = new Dictionary<string, object> { ["raw"] = line };

// Use the point in a rendering pipeline; the metadata travels with it.
renderer.AddPoint(pt);
```

## Notes

- All properties are mutable; concurrent writes to the same `DataPoint` instance from multiple threads are not synchronized and may lead to race conditions. If thread‑safe access is required, external locking or immutable snapshots (via `Clone`) should be employed.
- The `Metadata` property stores a reference to the supplied dictionary. If the caller mutates the dictionary after assignment, those changes are visible through the point. To avoid unintended sharing, pass a new dictionary or a copy when assigning.
- Setting `CustomRadius` to a negative value is not prohibited by the type definition but may be rejected by the charting library’s validation logic, resulting in an `ArgumentOutOfRangeException`. Consumers should validate values if negative radii are not meaningful for their chart type of their visual representation.
- The `State` property accepts any underlying integral value of the `DataPointState` enum. Assigning a value that does not correspond to a named enum member will not throw but may produce undefined rendering behavior; it is advisable to restrict assignments to the defined enum members.
- `ToString` is primarily intended for debugging and logging; its exact format may change between versions and should not be parsed for programmatic logic.
- The `Clone` method performs a shallow copy; therefore, reference‑type members like `Metadata` are shared between the original and the clone. If independent metadata is required, clone the dictionary manually after calling `Clone`.
