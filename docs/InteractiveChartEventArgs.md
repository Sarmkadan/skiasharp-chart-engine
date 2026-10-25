# InteractiveChartEventArgs

`Events/InteractiveChartEventArgs.cs` is the event-model file for chart user input. It declares three public types:

| Type | Kind | Purpose |
|------|------|---------|
| `ChartInteractionType` | `enum` | Kind of pointer or keyboard-driven gesture that occurred on a chart. |
| `ChartInteractionEventArgs` | `sealed class : EventArgs` | Payload raised for each user interaction, raised by `IChartInteractionService`. |
| `ChartSelectionChangedEventArgs` | `sealed class : EventArgs` | Payload describing the full set of currently selected data points. |

The file name does not match any type it declares. Use the type names above when referencing it in code.

## ChartInteractionType

| Member | Value | Meaning |
|--------|-------|---------|
| `Click` | 0 | Pointer clicked on a chart region. |
| `Hover` | 1 | Pointer is hovering over a chart region. |
| `Select` | 2 | A data point or series was selected. |
| `Deselect` | 3 | A data point or series was deselected. |
| `ContextMenu` | 4 | Right-click / context-menu gesture. |
| `DoubleClick` | 5 | Double-click gesture. |

## ChartInteractionEventArgs

Immutable after construction: every property uses `init` except `Timestamp`, which is set once at creation.

| Property | Type | Default | Purpose |
|----------|------|---------|---------|
| `InteractionType` | `ChartInteractionType` | `default` (`Click`) | The gesture that occurred. |
| `PointerX` | `float` | `0` | Canvas X coordinate in pixels where the interaction occurred. |
| `PointerY` | `float` | `0` | Canvas Y coordinate in pixels where the interaction occurred. |
| `Region` | `ChartRegion` | `default` | The logical chart region that was hit. |
| `HitDataPoint` | `DataPoint?` | `null` | The data point under the pointer, or `null` if the pointer missed all series. |
| `HitSeries` | `ChartSeries?` | `null` | The series that owns `HitDataPoint`, or `null` on a miss. |
| `SeriesIndex` | `int` | `-1` | Zero-based index of the series within the chart, or `-1` on a miss. |
| `TooltipText` | `string` | `string.Empty` | Formatted tooltip text produced by the hit-test, or an empty string on a miss. |
| `Timestamp` | `DateTime` | `DateTime.UtcNow` at construction | UTC time the event object was created. Read-only. |
| `Metadata` | `Dictionary<string, object>` | new empty dictionary | Arbitrary caller-supplied data. |

## ChartSelectionChangedEventArgs

| Property | Type | Default | Purpose |
|----------|------|---------|---------|
| `SelectedPoints` | `IReadOnlyDictionary<string, IReadOnlyList<DataPoint>>` | empty dictionary | Currently selected data points, keyed by series name. |
| `IsEmpty` | `bool` | computed | `true` when no series has any selected point. |
| `TotalSelected` | `int` | computed | Total number of selected data points across all series. |
| `Timestamp` | `DateTime` | `DateTime.UtcNow` at construction | UTC time the event object was created. Read-only. |

`IsEmpty` and `TotalSelected` are computed from `SelectedPoints` on each access, so they reflect the dictionary as it was passed in.

## Usage

```csharp
// Handle a click and inspect what was hit
void OnChartClicked(ChartInteractionEventArgs e)
{
    if (e.InteractionType != ChartInteractionType.Click)
    {
        return;
    }

    Console.WriteLine($"Clicked at ({e.PointerX}, {e.PointerY}) in {e.Region}");

    if (e.HitDataPoint is { } point && e.HitSeries is not null)
    {
        Console.WriteLine($"Series {e.SeriesIndex} ({e.HitSeries.Name}) hit at {point}");
    }
}

// Show tooltip text on hover, only when a point was hit
void OnChartHovered(ChartInteractionEventArgs e)
{
    if (e.InteractionType == ChartInteractionType.Hover && e.SeriesIndex >= 0)
    {
        Console.WriteLine(e.TooltipText);
    }
}

// React to selection changes
void OnSelectionChanged(ChartSelectionChangedEventArgs e)
{
    if (e.IsEmpty)
    {
        return;
    }

    Console.WriteLine($"{e.TotalSelected} point(s) selected");
}
```

## Notes

- On a miss, `HitDataPoint` and `HitSeries` are `null`, `SeriesIndex` is `-1`, and `TooltipText` is empty. Check `SeriesIndex >= 0` or `HitSeries is not null` before using hit data.
- `Metadata` is not read or populated by `ChartInteractionService`. It is a slot for values the caller attaches before or after the event is created.
- `Timestamp` is taken when the object is constructed, not when the gesture happened at the OS level.
- `ChartSelectionChangedEventArgs` does not hold a reference to the chart. It carries a snapshot of the selection passed in when the event was created.
