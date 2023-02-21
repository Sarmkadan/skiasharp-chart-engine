# ChartInteractionEventArgs
The `ChartInteractionEventArgs` class provides information about user interactions with a chart, such as mouse movements, clicks, or selections. It is used to notify the application of changes in the chart's state, allowing for custom handling of these events. This class is a crucial part of the `skiasharp-chart-engine` project, enabling developers to create interactive and responsive charts.

## API
* `public ChartInteractionType InteractionType`: Gets the type of interaction that occurred, such as a mouse click or hover.
* `public float PointerX` and `public float PointerY`: Get the x and y coordinates of the pointer (e.g., mouse cursor) at the time of the interaction.
* `public ChartRegion Region`: Gets the region of the chart where the interaction occurred.
* `public DataPoint? HitDataPoint`: Gets the data point that was hit (if any) during the interaction.
* `public ChartSeries? HitSeries`: Gets the series that was hit (if any) during the interaction.
* `public int SeriesIndex`: Gets the index of the series that was hit (if any) during the interaction.
* `public string TooltipText`: Gets the text to be displayed in a tooltip (if any) during the interaction.
* `public DateTime Timestamp`: Gets the timestamp of when the interaction occurred.
* `public Dictionary<string, object> Metadata`: Gets a dictionary of metadata associated with the interaction.
* `public IReadOnlyDictionary<string, IReadOnlyList<DataPoint>> SelectedPoints`: Gets a dictionary of selected points, keyed by series name.

## Usage
```csharp
// Example 1: Handling a chart click event
void OnChartClicked(ChartInteractionEventArgs e)
{
    if (e.InteractionType == ChartInteractionType.Click)
    {
        Console.WriteLine($"Clicked at ({e.PointerX}, {e.PointerY})");
        if (e.HitDataPoint.HasValue)
        {
            Console.WriteLine($"Hit data point: {e.HitDataPoint.Value}");
        }
    }
}

// Example 2: Displaying a tooltip on hover
void OnChartHovered(ChartInteractionEventArgs e)
{
    if (e.InteractionType == ChartInteractionType.Hover)
    {
        Console.WriteLine($"Hovering at ({e.PointerX}, {e.PointerY})");
        if (!string.IsNullOrEmpty(e.TooltipText))
        {
            Console.WriteLine($"Tooltip text: {e.TooltipText}");
        }
    }
}
```

## Notes
When handling chart interactions, be aware that the `HitDataPoint` and `HitSeries` properties may be null if no data point or series was hit. Additionally, the `SelectedPoints` dictionary may be empty if no points are selected. The `Metadata` dictionary can be used to store custom data associated with the interaction. This class is not thread-safe, and interactions should be handled on the same thread that created the chart. The `Timestamp` property can be used to track the timing of interactions, but its accuracy may depend on the system clock and other factors.
