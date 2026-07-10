# ChartInteractionServiceTests
The `ChartInteractionServiceTests` class is designed to test the functionality of the `ChartInteractionService` class, which is responsible for handling user interactions with charts, such as clicking and hovering over data points. This test class ensures that the service behaves correctly in various scenarios, including null charts, data point clicks, hover misses, and selection toggling.

## API
* `public ChartInteractionServiceTests()`: The constructor for the `ChartInteractionServiceTests` class.
* `public void ProcessInteraction_WithNullChart_ThrowsArgumentNullException()`: Tests that an `ArgumentNullException` is thrown when the `ProcessInteraction` method is called with a null chart.
* `public void ProcessInteraction_ClickOnDataPoint_RaisesClickedEvent()`: Verifies that the `Clicked` event is raised when a data point is clicked.
* `public void ProcessInteraction_HoverMiss_ReturnsNoHit()`: Checks that the `ProcessInteraction` method returns no hit when the user hovers over a non-data point area.
* `public void ToggleSelection_HitDataPoint_SelectsAndRaisesEvent()`: Tests that selecting a data point raises the corresponding event and updates the selection.
* `public void ToggleSelection_SamePointTwice_DeselectionRemovesPoint()`: Verifies that deselecting a data point removes it from the selection.
* `public void ClearSelection_AfterSelect_EmptiesSelection()`: Ensures that clearing the selection after selecting a data point empties the selection.
* `public async Task ProcessInteractionAsync_WithCancellation_ThrowsOperationCancelled()`: Tests that the `ProcessInteractionAsync` method throws an `OperationCanceledException` when cancellation is requested.

## Usage
```csharp
// Example 1: Testing chart interaction with a valid chart
var chart = new Chart();
var service = new ChartInteractionService(chart);
var test = new ChartInteractionServiceTests();
test.ProcessInteraction_ClickOnDataPoint_RaisesClickedEvent();

// Example 2: Testing chart interaction with a null chart
var nullChart = (Chart)null;
var serviceNull = new ChartInteractionService(nullChart);
var testNull = new ChartInteractionServiceTests();
try
{
    testNull.ProcessInteraction_WithNullChart_ThrowsArgumentNullException();
}
catch (ArgumentNullException ex)
{
    Console.WriteLine("ArgumentNullException caught as expected.");
}
```

## Notes
The `ChartInteractionServiceTests` class is designed to be thread-safe, as it does not maintain any internal state that could be accessed concurrently. However, the `ProcessInteractionAsync` method is cancellable, which means that it can be interrupted by an `OperationCanceledException` if the cancellation token is cancelled. Additionally, the `ToggleSelection` methods assume that the data points are unique and can be selected or deselected independently. If the chart contains duplicate data points, the selection behavior may not be as expected.
