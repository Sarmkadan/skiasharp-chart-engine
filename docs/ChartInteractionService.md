# ChartInteractionService

The `ChartInteractionService` provides the core mechanism for handling user input and managing interactive states within the SkiaSharp chart engine. It acts as the central coordinator for processing mouse, touch, or keyboard events, translating raw input into structured interaction arguments, and maintaining the current selection state of data points across the chart visualization.

## API

### `ChartInteractionService`
The primary constructor initializes a new instance of the service. It sets up the internal state required to track selections and process incoming interaction events. No parameters are required for instantiation.

### `ProcessInteraction`
```csharp
public ChartInteractionEventArgs ProcessInteraction
```
This member represents the synchronous event handler or delegate invoked when an interaction occurs. It processes the input immediately on the calling thread and returns a `ChartInteractionEventArgs` object containing the details of the interaction, such as the target element, coordinates, and interaction type. If the input is invalid or cannot be mapped to a chart element, it may return an event args object indicating no target was hit; it does not typically throw exceptions for standard miss-hits but may throw if the internal state is corrupted.

### `ProcessInteractionAsync`
```csharp
public async Task<ChartInteractionEventArgs> ProcessInteractionAsync
```
An asynchronous variant of the interaction processor. This method is designed for scenarios where interaction processing might involve awaiting external data lookups or non-blocking UI updates. It returns a `Task<ChartInteractionEventArgs>` that resolves to the interaction details once processing is complete. This method ensures the UI thread remains responsive during complex hit-testing operations.

### `ToggleSelection`
```csharp
public bool ToggleSelection
```
A property or method accessor (depending on implementation context implied by usage) that toggles the selection state of the currently targeted data point. When invoked, it switches the selection status of the specific element involved in the last processed interaction. It returns a `bool` indicating the new selection state (`true` if selected, `false` if deselected). It returns `false` if no valid target exists for the toggle operation.

### `ClearSelection`
```csharp
public void ClearSelection
```
Removes all active selections from the chart. This method resets the internal selection dictionary to an empty state and triggers any necessary redraw events to reflect the change visually. It returns `void` and does not throw exceptions under normal circumstances.

### `GetSelection`
```csharp
public IReadOnlyDictionary<string, IReadOnlyList<DataPoint>> GetSelection
```
Retrieves the current collection of selected data points, organized by their series identifier. The return type is an `IReadOnlyDictionary` where the key is the series name (`string`) and the value is a read-only list of `DataPoint` objects. This ensures consumers can inspect the selection without modifying the internal state. If no points are selected, it returns an empty dictionary.

## Usage

### Example 1: Handling Mouse Clicks and Toggling Selection
This example demonstrates how to wire up a mouse click event to the service, process the interaction, and toggle the selection of the clicked point.

```csharp
// Initialize the service
var interactionService = new ChartInteractionService();

// Simulate an incoming mouse event
void OnChartMouseClick(object sender, MouseEventArgs e)
{
    // Process the interaction synchronously
    var eventArgs = interactionService.ProcessInteraction;
    
    // Assume eventArgs is populated with hit-test data from the service logic
    if (eventArgs.HasTarget)
    {
        // Toggle the selection state of the hit element
        bool isSelected = interactionService.ToggleSelection;
        
        if (isSelected)
        {
            Console.WriteLine($"Point selected: {eventArgs.TargetPoint}");
        }
        else
        {
            Console.WriteLine($"Point deselected: {eventArgs.TargetPoint}");
        }
    }
}
```

### Example 2: Retrieving and Displaying Selected Data
This example shows how to retrieve the current selection state after multiple interactions and iterate over the selected data points by series.

```csharp
// After several user interactions
void DisplayCurrentSelection(ChartInteractionService service)
{
    var selectedData = service.GetSelection;

    if (selectedData.Count == 0)
    {
        Console.WriteLine("No data points currently selected.");
        return;
    }

    foreach (var seriesEntry in selectedData)
    {
        string seriesName = seriesEntry.Key;
        IReadOnlyList<DataPoint> points = seriesEntry.Value;

        Console.WriteLine($"Series '{seriesName}' has {points.Count} selected points:");
        foreach (var point in points)
        {
            Console.WriteLine($" - X: {point.X}, Y: {point.Y}");
        }
    }

    // User decides to reset
    service.ClearSelection();
}
```

## Notes

*   **Thread Safety**: The `GetSelection` method returns read-only interfaces (`IReadOnlyDictionary`, `IReadOnlyList`), which protects the internal collection from direct external modification. However, the service itself is not guaranteed to be thread-safe for concurrent write operations. Calls to `ToggleSelection`, `ClearSelection`, and `ProcessInteraction` should be marshaled to the same thread (typically the UI thread) to prevent race conditions during state updates.
*   **Asynchronous Consistency**: When using `ProcessInteractionAsync`, ensure that subsequent calls to `ToggleSelection` or `GetSelection` await the task completion if they depend on the specific interaction result being fully committed to the internal state.
*   **Empty States**: `GetSelection` will never return `null`; it returns an empty dictionary when no selections are active. Similarly, `ToggleSelection` will return `false` if the interaction context does not contain a valid target to toggle.
*   **Data Integrity**: The keys in the dictionary returned by `GetSelection` correspond to the series names defined in the chart model. If a series is removed from the chart while points from that series are selected, those points may be automatically purged from the selection state during the next `ClearSelection` or rendering cycle.
