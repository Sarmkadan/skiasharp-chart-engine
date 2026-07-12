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

## ApiResponse

`ApiResponse` is a generic class used to represent a response from the API. It contains information about the status of the response, including the HTTP status code, success flag, and any error message. It also includes a generic type `T` to represent the data returned in the response.

```csharp
using SkiasharpChartEngine.API.Responses;

public class ApiResponseExample
{
    public static void Main(string[] args)
    {
        var response = ApiResponse<int>.Ok(42);
        Console.WriteLine($"Status Code: {response.StatusCode}");
        Console.WriteLine($"Success: {response.Success}");
        Console.WriteLine($"Data: {response.Data}");
        Console.WriteLine($"Message: {response.Message}");
        Console.WriteLine($"Timestamp: {response.Timestamp}");
        Console.WriteLine($"TraceId: {response.TraceId}");
    }
}
```

## DataController

`DataController` is a REST API controller that handles data validation, transformation, and aggregation operations for chart data. It provides endpoints for calculating statistics, validating data points, aggregating data, filtering by value range, and resampling data series.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharpChartEngine.API.Controllers;
using SkiaSharpChartEngine.API.Responses;
using SkiaSharpChartEngine.Models;

public class DataControllerExample
{
    public static async Task Main(string[] args)
    {
        // Initialize controller (typically injected via DI in real applications)
        var dataController = new DataController(
            new ChartDataService(), // Mock service for demonstration
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<DataController>()
        );

        // Example data points
        var dataPoints = new List<DataPoint>
        {
            new DataPoint { X = 1, Value = 10 },
            new DataPoint { X = 2, Value = 25 },
            new DataPoint { X = 3, Value = 30 },
            new DataPoint { X = 4, Value = 45 },
            new DataPoint { X = 5, Value = 50 }
        };

        // Validate data points
        var validationResult = dataController.ValidateDataPoints(dataPoints);
        Console.WriteLine($"Validation: {validationResult.Success}, Count: {validationResult.Data?.count}");

        // Get data statistics
        var statsResult = await dataController.GetDataStatisticsAsync("series-123");
        Console.WriteLine($"Stats: Min={statsResult.Data?.minValue}, Max={statsResult.Data?.maxValue}, Avg={statsResult.Data?.average}");

        // Filter by value range (15 to 40)
        var filteredResult = dataController.FilterByRange(dataPoints, 15, 40);
        Console.WriteLine($"Filtered count: {filteredResult.Data?.Count}");

        // Aggregate data (every 2 points)
        var aggregatedResult = await dataController.AggregateDataAsync(dataPoints, "average", 2);
        Console.WriteLine($"Aggregated count: {aggregatedResult.Data?.Count}");

        // Resample to 3 points
        var resampledResult = await dataController.ResampleDataAsync(dataPoints, 3);
        Console.WriteLine($"Resampled count: {resampledResult.Data?.Count}");
    }
}
```

// ... (rest of the file remains the same)
```