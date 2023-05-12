## DataAggregatorExtensions

`DataAggregatorExtensions` provides static methods for filtering, aggregating, and transforming collections of `DataPoint` objects. It includes utilities for statistical analysis, normalization, and selection of top/bottom data points based on their values.

```csharp
using SkiasharpChartEngine.Utilities;

public class DataAggregatorExtensionsDemo
{
    public static void Main(string[] args)
    {
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { X = 1, Y = 5 },
            new DataPoint { X = 2, Y = 15 },
            new DataPoint { X = 3, Y = 25 },
            new DataPoint { X = 4, Y = 35 },
            new DataPoint { X = 5, Y = 45 }
        };

        // Filter data points by minimum Y value
        var filteredMin = dataPoints.FilterByMinValue(10);

        // Filter data points by maximum Y value
        var filteredMax = dataPoints.FilterByMaxValue(30);

        // Filter data points within a Y value range
        var filteredRange = dataPoints.FilterByRange(10, 30);

        // Calculate statistics for the dataset
        var stats = dataPoints.AggregateWithStatistics();

        // Normalize Y values to a 0-1 scale
        var normalized = dataPoints.NormalizeValues();

        // Get top 3 data points by Y value
        var top3 = dataPoints.GetTopN(3);

        // Get bottom 2 data points by Y value
        var bottom2 = dataPoints.GetBottomN(2);
    }
}
```

// ... (rest of the file remains the same)
