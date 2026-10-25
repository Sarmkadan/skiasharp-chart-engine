# IChartInteractionService

`IChartInteractionService` defines the contract for handling pointer interactions (click, hover, select) on `Chart` instances in the `skiasharp-chart-engine`. A caller passes canvas coordinates to the service, the service hit-tests them against the chart, and the outcome is returned as a `ChartInteractionEventArgs` and published through events. The service also keeps per-chart selection state, so callers can toggle, read, and clear the selected data points.

The engine's default implementation, `ChartInteractionService`, depends on `IInteractivityService` for hit-testing and keeps selections in memory, keyed by chart id. The interface itself does not prescribe how hit-testing or selection storage works.

**Namespace:** `SkiaSharpChartEngine.Services`
**Declaration:** `public interface IChartInteractionService`
**Source:** `Services/IChartInteractionService.cs`

## Events

### `event EventHandler<ChartInteractionEventArgs> Clicked`
Raised when a click interaction is processed. The implementation raises it for `ChartInteractionType.Click`, `DoubleClick`, and `ContextMenu`.

### `event EventHandler<ChartInteractionEventArgs> Hovered`
Raised when a hover interaction is processed (`ChartInteractionType.Hover`).

### `event EventHandler<ChartSelectionChangedEventArgs> SelectionChanged`
Raised when the set of selected data points changes. This happens after a successful toggle and after a selection is cleared.

## API

### `ChartInteractionEventArgs ProcessInteraction(Chart chart, ChartInteractionType interactionType, float pointerX, float pointerY, float canvasWidth, float canvasHeight, TooltipOptions? tooltipOptions = null, ViewportState? viewport = null)`
Processes a pointer interaction at the given canvas coordinates, hit-tests it against the chart, and raises the matching event.
- **Parameters:**
  - `chart` - The target chart.
  - `interactionType` - The `ChartInteractionType` to process.
  - `pointerX`, `pointerY` - Canvas coordinates of the pointer, in pixels.
  - `canvasWidth`, `canvasHeight` - Total canvas size, in pixels.
  - `tooltipOptions` - Optional tooltip configuration used during hit-testing.
  - `viewport` - Optional active viewport.
- **Returns:** A `ChartInteractionEventArgs` describing the outcome: the interaction type, pointer position, the `ChartRegion` hit, the hit `DataPoint` and `ChartSeries` (when a data point was hit), the series index, and the tooltip text.

### `Task<ChartInteractionEventArgs> ProcessInteractionAsync(Chart chart, ChartInteractionType interactionType, float pointerX, float pointerY, float canvasWidth, float canvasHeight, TooltipOptions? tooltipOptions = null, ViewportState? viewport = null, CancellationToken cancellationToken = default)`
Asynchronous form of `ProcessInteraction`. It performs the same work and raises the same events.
- **Parameters:** As for `ProcessInteraction`, plus `cancellationToken`, which is checked before processing begins.
- **Returns:** A `Task<ChartInteractionEventArgs>` that completes with the same result `ProcessInteraction` would return.

### `bool ToggleSelection(Chart chart, float pointerX, float pointerY, float canvasWidth, float canvasHeight, ViewportState? viewport = null)`
Toggles the selection of the data point nearest to the given coordinates. If that point is already selected it is removed; otherwise it is added. Selection is tracked per series name.
- **Parameters:** `chart`, `pointerX`, `pointerY`, `canvasWidth`, `canvasHeight`, and `viewport` as described above.
- **Returns:** `true` when a point was toggled; `false` when the pointer missed every selectable point. A miss leaves the selection unchanged.

### `void ClearSelection(Chart chart)`
Removes all selected data points for the chart and raises `SelectionChanged`. Clearing a chart with no selection still raises the event, with an empty result.

### `IReadOnlyDictionary<string, IReadOnlyList<DataPoint>> GetSelection(Chart chart)`
Returns the current selection for the chart.
- **Returns:** A dictionary keyed by series name, where each value is the list of selected `DataPoint`s in that series. If nothing is selected for the chart, the dictionary is empty.

## Behavior Notes

- Methods are called with a `Chart` and pointer coordinates in canvas space. The interface does not validate coordinates; hit-testing is delegated to the implementation.
- The `ProcessInteraction` family raises events based on `interactionType`. `Click`, `DoubleClick`, and `ContextMenu` raise `Clicked`; `Hover` raises `Hovered`. `Select` is routed to the selection logic (see the note below).
- A miss (no data point under the pointer) is not an error. `ProcessInteraction` still returns args with no hit data, and `ToggleSelection` returns `false`.
- Selection state is scoped to a chart id. `GetSelection` returns a snapshot; changing the returned collections does not alter the stored selection.
- The default implementation's `Select` handling calls `ToggleSelection` with a canvas size of `0, 0` and no viewport, because `ProcessInteraction` does not forward them. Hit-testing for `Select` therefore may not match the layout used for `Click` and `Hover`. Callers that need accurate selection should call `ToggleSelection` directly with the real canvas dimensions and viewport.
- Implementations that replace the default must be thread-safe if the same chart is used from multiple threads. The default implementation guards its selection store with a lock.

## Usage

### Click, Hover, and Selection
```csharp
using SkiaSharpChartEngine.Events;
using SkiaSharpChartEngine.Services;

public sealed class ChartPointerHandler
{
    private readonly IChartInteractionService _interaction;

    public ChartPointerHandler(IChartInteractionService interaction)
    {
        _interaction = interaction ?? throw new ArgumentNullException(nameof(interaction));
        _interaction.Clicked += (_, args) => Console.WriteLine($"Click in {args.Region}");
        _interaction.SelectionChanged += (_, args) => Console.WriteLine($"Selected: {args.TotalSelected}");
    }

    public void OnPointerDown(Chart chart, float x, float y, float width, float height)
    {
        _interaction.ProcessInteraction(chart, ChartInteractionType.Click, x, y, width, height);
    }

    public void OnPointerMove(Chart chart, float x, float y, float width, float height)
    {
        _interaction.ProcessInteraction(chart, ChartInteractionType.Hover, x, y, width, height);
    }

    public void OnShiftClick(Chart chart, float x, float y, float width, float height)
    {
        _interaction.ToggleSelection(chart, x, y, width, height);
    }
}
```

### Reading and Clearing the Selection
```csharp
IReadOnlyDictionary<string, IReadOnlyList<DataPoint>> selected = _interaction.GetSelection(chart);

foreach (var (seriesName, points) in selected)
{
    Console.WriteLine($"{seriesName}: {points.Count} point(s)");
}

_interaction.ClearSelection(chart);
```

### Asynchronous Processing
```csharp
var args = await _interaction.ProcessInteractionAsync(
    chart,
    ChartInteractionType.Hover,
    x, y,
    width, height,
    cancellationToken: cancellationToken);

if (args.HitDataPoint is not null)
{
    Console.WriteLine(args.TooltipText);
}
```

## Related

- `ChartInteractionService` - Default implementation.
- `ChartInteractionEventArgs` and `ChartSelectionChangedEventArgs` - Event payloads (`Events/InteractiveChartEventArgs.cs`).
- `ChartInteractionType` - Interaction kinds accepted by `ProcessInteraction`.
- `IInteractivityService` - Hit-testing dependency used by the default implementation.
