# IChartEventSubscriber
The `IChartEventSubscriber` type is designed to handle events related to charts, providing information about the chart, the event itself, and any relevant metadata. It serves as a foundation for building event-driven systems that interact with charts, allowing for the creation of custom event handlers and subscribers.

## API
* `EventId`: A unique identifier for the event.
* `Timestamp`: The date and time when the event occurred.
* `SourceName`: The name of the source that triggered the event, or `null` if not applicable.
* `Metadata`: A dictionary containing additional information about the event.
* `GetEventName`: A virtual property that returns the name of the event.
* `ChartId`: A required identifier for the chart associated with the event.
* `Title`: The title of the chart, or `null` if not set.
* `ChartType`: The type of chart.
* `SeriesCount`: The number of series in the chart.
* `DataPointCount`: The number of data points in the chart.
* `ModifiedFields`: An array of field names that were modified, or `null` if not applicable.
* `PreviousUpdateTime`: The date and time of the previous update, or `null` if not applicable.
* `DeletedBy`: The identifier of the entity that deleted the chart, or `null` if not applicable.
* `Width` and `Height`: The dimensions of the chart.

## Usage
```csharp
// Example 1: Creating a basic event subscriber
public class MyEventSubscriber : IChartEventSubscriber
{
    public string EventId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? SourceName { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public string GetEventName => "MyEvent";
    public required string ChartId { get; set; }
    public string? Title { get; set; }
    public ChartType ChartType { get; set; }
    public int SeriesCount { get; set; }
    public int DataPointCount { get; set; }

    public void HandleEvent()
    {
        Console.WriteLine($"Event {EventId} occurred at {Timestamp} for chart {ChartId}");
    }
}

// Example 2: Using the event subscriber to track chart updates
public class ChartUpdater
{
    private readonly IChartEventSubscriber _eventSubscriber;

    public ChartUpdater(IChartEventSubscriber eventSubscriber)
    {
        _eventSubscriber = eventSubscriber;
    }

    public void UpdateChart()
    {
        // Update chart logic here
        _eventSubscriber.ModifiedFields = new[] { "Series1", "Series2" };
        _eventSubscriber.PreviousUpdateTime = DateTime.Now;
        Console.WriteLine("Chart updated");
    }
}
```

## Notes
When implementing `IChartEventSubscriber`, consider the following edge cases:
* `ChartId` is required, so ensure it is always set to a valid identifier.
* `Metadata` can be `null`, so check for `null` before accessing its contents.
* `GetEventName` is virtual, so override it to provide a custom event name if needed.
* `ModifiedFields` and `PreviousUpdateTime` can be `null`, so check for `null` before using them.
* The `IChartEventSubscriber` type does not provide any thread-safety guarantees, so ensure that any multithreaded access to its members is properly synchronized.
