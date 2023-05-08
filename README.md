// README.md
## Configuration

The Skiasharp Chart Engine supports the following configuration settings:

* `CacheEnabled`: Enable or disable caching.
* `CacheDurationSeconds`: Cache expiration time in seconds.
* `MaxConcurrentRenders`: Maximum number of chart renders that may run concurrently.
* `DefaultChartWidth`: Default chart width.
* `DefaultChartHeight`: Default chart height.
* `DefaultBackgroundColor`: Default chart background color.
* `UseAntiAliasing`: Enable or disable anti-aliasing.
* `MaxDataPointsPerSeries`: Maximum number of data points per series.
* `MaxSeriesPerChart`: Maximum number of series per chart.
* `ValidateDataOnLoad`: Enable or disable data validation on load.

These settings can be configured in the `appsettings.json` file.

## Example Configuration

```json
{
  "SkiasharpChartEngine": {
    "CacheEnabled": "true",
    "CacheDurationSeconds": 3600,
    "MaxConcurrentRenders": 10,
    "DefaultChartWidth": 800,
    "DefaultChartHeight": 600,
    "DefaultBackgroundColor": "#FFFFFF",
    "UseAntiAliasing": true,
    "MaxDataPointsPerSeries": 1000,
    "MaxSeriesPerChart": 10,
    "ValidateDataOnLoad": true
  }
}
```

## ChartEngineExtensions

`ChartEngineExtensions` provides a set of convenience extension methods that simplify common chart‑engine workflows such as rendering a chart, saving the result to a file, creating a chart instance, and rendering directly to a memory stream. The methods are available in both synchronous and asynchronous variants, allowing you to choose the most appropriate approach for your application.

### Usage example

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using SkiasharpChartEngine.Models;   // RenderResult, Chart
using SkiasharpChartEngine.Extensions; // ChartEngineExtensions

public class ChartDemo
{
    public static async Task Main(string[] args)
    {
        // 1️⃣ Render a chart and save it to a file (async)
        RenderResult asyncResult = await ChartEngineExtensions
            .RenderAndSaveChartAsync("chart1.png", width: 800, height: 600);

        // 2️⃣ Render a chart and save it to a file (sync)
        RenderResult syncResult = ChartEngineExtensions
            .RenderAndSaveChart("chart2.png", width: 800, height: 600);

        // 3️⃣ Create a chart instance and persist it (async)
        Chart asyncChart = await ChartEngineExtensions
            .CreateAndSaveChartAsync("myChart", width: 800, height: 600);

        // 4️⃣ Create a chart instance and persist it (sync)
        Chart syncChart = ChartEngineExtensions
            .CreateAndSaveChart("myChartSync", width: 800, height: 600);

        // 5️⃣ Render a chart directly to a memory stream (async)
        await using MemoryStream asyncStream = await ChartEngineExtensions
            .RenderToStreamAsync(width: 800, height: 600);
        // e.g., send asyncStream to a client or save it later

        // 6️⃣ Render a chart directly to a memory stream (sync)
        using MemoryStream syncStream = ChartEngineExtensions
            .RenderToStream(width: 800, height: 600);
        // e.g., write syncStream to a file
        File.WriteAllBytes("chart3.png", syncStream.ToArray());

        // 7️⃣ Quick one‑off render that returns a RenderResult (async)
        RenderResult quickResult = await ChartEngineExtensions
            .QuickRenderAsync(width: 800, height: 600);

        Console.WriteLine("All chart operations completed successfully.");
    }
}
```

## ApiResponseExtensions

`ApiResponseExtensions` provides a set of extension methods for working with API responses. It allows you to add a trace ID to a response, add an error to a response, convert a list of items to a standard response, and convert a list of items to a paginated response. 

Here's an example of how to use `ApiResponseExtensions`:

```csharp
using System;
using System.Collections.Generic;
using SkiasharpChartEngine.API.Responses;

public class ApiResponseDemo
{
    public static void Main(string[] args)
    {
        // Create a list of items
        List<string> items = new List<string> { "Item1", "Item2", "Item3" };

        // Convert the list to a standard response
        ApiResponse<List<string>> standardResponse = items.ToStandardResponse();

        // Convert the list to a paginated response
        PaginatedResponse<string> paginatedResponse = items.ToPaginatedResponse();

        // Add a trace ID to the response
        ApiResponse<List<string>> responseWithTraceId = standardResponse.WithTraceId("myTraceId");

        // Add an error to the response
        ApiResponse<List<string>> responseWithError = standardResponse.WithError("myError");
    }
}
```

## TooltipOptionsExtensions

`TooltipOptionsExtensions` provides a set of extension methods for customizing tooltip options. It allows you to easily switch between light, dark, and high contrast themes, adjust the size, clone existing options, and more.

### Usage example

```csharp
using System;
using SkiasharpChartEngine.Models;

public class TooltipDemo
{
    public static void Main(string[] args)
    {
        // Create a new tooltip options instance with a light theme
        TooltipOptions lightTooltip = TooltipOptionsExtensions.WithLightTheme();

        // Create a new tooltip options instance with a dark theme
        TooltipOptions darkTooltip = TooltipOptionsExtensions.WithDarkTheme();

        // Create a new tooltip options instance with a high contrast theme
        TooltipOptions highContrastTooltip = TooltipOptionsExtensions.WithHighContrast();

        // Create a new tooltip options instance with a large size
        TooltipOptions largeTooltip = TooltipOptionsExtensions.WithLargeSize();

        // Create a new tooltip options instance with a small size
        TooltipOptions smallTooltip = TooltipOptionsExtensions.WithSmallSize();

        // Clone an existing tooltip options instance
        TooltipOptions clonedTooltip = lightTooltip.Clone();

        // Check if a tooltip should be shown
        bool shouldShowTooltip = TooltipOptionsExtensions.ShouldShow(clonedTooltip);

        // Customize the background color of a tooltip
        TooltipOptions customizedTooltip = TooltipOptionsExtensions.WithBackgroundColor(clonedTooltip, "#CCCCCC");
    }
}
```

## ErrorHandlingMiddlewareExtensions

`ErrorHandlingMiddlewareExtensions` supplies a collection of helper methods for configuring error‑handling middleware with different logging strategies and for processing exceptions into standardized error responses. You can choose a logger (console, custom, or none), capture logs for testing, and map exceptions to appropriate HTTP status codes.

### Usage example

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiasharpChartEngine.Middleware;

public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        // Add the error‑handling middleware using the built‑in console logger
        var errorMiddleware = ErrorHandlingMiddlewareExtensions.WithConsoleLogger();
        app.UseMiddleware<ErrorHandlingMiddleware>(errorMiddleware);

        // Example of manually handling an exception inside the pipeline
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                // Create a standardized error response from the exception
                var errorResponse = await ErrorHandlingMiddlewareExtensions.CreateErrorResponseAsync(ex);
                context.Response.StatusCode = errorResponse.StatusCode;
                await context.Response.WriteAsync(errorResponse.Message);
            }
        });
    }
}

// Stand‑alone usage of the static helpers
public class ErrorHandlingDemo
{
    public static async Task DemoAsync()
    {
        var exception = new InvalidOperationException("Invalid operation");

        // Map the exception to an HTTP status code and message
        var (statusCode, message) = ErrorHandlingMiddlewareExtensions.MapException(exception);
        Console.WriteLine($"Mapped status: {statusCode}, message: {message}");

        // Process the exception (e.g., log it) using the default middleware configuration
        await ErrorHandlingMiddlewareExtensions.ProcessExceptionAsync(exception);

        // Create an error response that can be returned from an API endpoint
        var errorResponse = await ErrorHandlingMiddlewareExtensions.CreateErrorResponseAsync(exception);
        Console.WriteLine($"Error response: {errorResponse.StatusCode} - {errorResponse.Message}");

        // Capture logs for testing purposes
        using var provider = new ErrorHandlingMiddlewareExtensions.CapturingLoggerProvider();
        var logger = provider.CreateLogger("Demo");
        logger.LogError("A captured error message");
    }
}
```

## DataAggregatorTestsExtensions

`DataAggregatorTestsExtensions` provides a fluent set of test helpers for the `DataAggregatorTests` type.  
It lets you configure a test instance with specific data points, then assert that aggregation produces the expected count, values, statistics, grouping behavior, and proper handling of null data points. The class also includes factory methods for creating sequential or random `DataPoint` collections used in tests.

### Usage example

```csharp
using System.Collections.Generic;
using SkiasharpChartEngine.Models;               // DataPoint
using SkiasharpChartEngine.Tests;                // DataAggregatorTests, DataAggregatorTestsExtensions

public class DataAggregatorTestsDemo
{
    public void Run()
    {
        // Create a test instance and configure it with sequential data points
        var test = new DataAggregatorTests()
            .WithDataPoints(DataAggregatorTestsExtensions.CreateSequentialDataPoints(1, 10));

        // Verify that the aggregator reports the correct count
        test.ShouldAggregateToCount(expectedCount: 10);

        // Verify that the aggregated values match the original sequence
        test.ShouldAggregateToValues(expectedValues: new List<double> { 1,2,3,4,5,6,7,8,9,10 });

        // Verify calculated statistics (e.g., average)
        test.ShouldCalculateStatistics(expectedAverage: 5.5);

        // Verify grouping by label works as expected
        test.ShouldGroupByLabel(expectedGroupCount: 1);

        // Verify that the aggregator gracefully handles null data points
        var nullTest = new DataAggregatorTests()
            .WithDataPoints(DataAggregatorTestsExtensions.CreateRandomDataPoints(5));
        nullTest.ShouldHandleNullDataPoints();

        // You can also chain the WithDataPoints overload that accepts a collection directly
        var anotherTest = new DataAggregatorTests()
            .WithDataPoints(new List<DataPoint>
            {
                new DataPoint { X = 0, Y = 0 },
                new DataPoint { X = 1, Y = 1 }
            });
        anotherTest.ShouldAggregateToCount(2);
    }
}
```

These extensions make test code concise and expressive while ensuring that the data aggregation logic is thoroughly validated.

## ChartInteractionServiceTestsExtensions

`ChartInteractionServiceTestsExtensions` provides a collection of fluent extension methods for testing chart interaction scenarios with the `ChartInteractionService`. It includes factory methods for creating charts and tooltip hit results, as well as assertion helpers for validating interaction results. These extensions simplify testing of chart interactivity features like tooltip display, hit detection, and user interaction handling.

### Usage example

```csharp
using System;
using Moq;
using SkiasharpChartEngine.Models;
using SkiasharpChartEngine.Services;
using SkiasharpChartEngine.Tests;
using Xunit;

public class ChartInteractionTestsDemo
{
    public void Run()
    {
        // 1️⃣ Create a chart with multiple series and data points for testing
        var chart = new ChartInteractionServiceTests().CreateChart(
            id: "test-chart",
            seriesCount: 3,
            dataPointsPerSeries: 5
        );

        // Verify the chart was created correctly
        Assert.Equal(3, chart.Series.Count);
        Assert.Equal(5, chart.Series[0].DataPoints.Count);

        // 2️⃣ Create a tooltip hit result at a specific data point
        var hitResult = new ChartInteractionServiceTests().MakeHit(
            chart: chart,
            seriesIndex: 1,
            dataPointIndex: 2
        );

        // Verify the hit result properties
        Assert.True(hitResult.IsHit);
        Assert.Equal(2, hitResult.SeriesIndex);
        Assert.Equal(3.0, hitResult.DataPoint.X);
        Assert.Equal(30.0, hitResult.DataPoint.Y);

        // 3️⃣ Set up a mock interactivity service to return a specific hit result
        var mockService = new Mock<IInteractivityService>();
        mockService.SetupHitTest(chart, hitResult);

        // 4️⃣ Set up a mock interactivity service to return a miss result
        mockService.SetupHitMiss(chart);

        // 5️⃣ Verify that an interaction result is a valid hit
        var interactionArgs = new ChartInteractionEventArgs
        {
            HitDataPoint = hitResult.DataPoint,
            HitSeries = chart.Series[1],
            Region = ChartRegion.PlotArea,
            SeriesIndex = 1
        };
        new ChartInteractionServiceTests().ShouldBeValidHit(
            interactionArgs,
            hitResult.DataPoint,
            chart.Series[1]
        );

        // 6️⃣ Verify that an interaction result is a miss
        var missArgs = new ChartInteractionEventArgs
        {
            HitDataPoint = null,
            HitSeries = null,
            Region = ChartRegion.Outside,
            SeriesIndex = -1
        };
        new ChartInteractionServiceTests().ShouldBeMiss(missArgs);
    }
}
```

These extensions make chart interaction tests concise and readable while ensuring comprehensive coverage of interactivity scenarios.