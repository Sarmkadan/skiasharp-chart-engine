## TemplateController

`TemplateController` is a REST API controller that handles template management operations. It provides endpoints for retrieving, creating, updating, and deleting chart templates.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharpChartEngine.API.Controllers;
using SkiaSharpChartEngine.API.Responses;
using SkiaSharpChartEngine.Models;

public class TemplateControllerExample
{
    public static async Task Main(string[] args)
    {
        // Initialize controller (typically injected via DI in real applications)
        var templateController = new TemplateController(
            new ChartDataService(), // Mock service for demonstration
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<TemplateController>()
        );

        // Get all templates
        var templatesResponse = await templateController.GetAllTemplatesAsync();
        Console.WriteLine($"Templates count: {templatesResponse.Data?.Count}");

        // Get a template by ID
        var templateResponse = await templateController.GetTemplateByIdAsync("template-123");
        Console.WriteLine($"Template ID: {templateResponse.Data?.Id}");
        Console.WriteLine($"Template Name: {templateResponse.Data?.Name}");

        // Get templates by type
        var templatesByTypeResponse = await templateController.GetTemplatesByTypeAsync("line");
        Console.WriteLine($"Templates count by type: {templatesByTypeResponse.Data?.Count}");

        // Create a new template
        var createdTemplateResponse = await templateController.CreateTemplateAsync(
            new ChartTemplate { Name = "My Template", Type = "line" }
        );
        Console.WriteLine($"Created template ID: {createdTemplateResponse.Data?.Id}");

        // Delete a template
        var deleted = await templateController.DeleteTemplateAsync("template-123");
        Console.WriteLine($"Template deleted: {deleted}");
    }
}
```

## ChartEngineException

`ChartEngineException` is a custom exception class that represents an error that occurred during chart engine operations. It provides a way to handle and propagate errors in a meaningful way.

```csharp
try
{
    // Code that may throw an exception
}
catch (ChartEngineException ex)
{
    Console.WriteLine($"Error code: {ex.ErrorCode}");
    Console.WriteLine($"Error message: {ex.Message}");
}

// Example of throwing a ChartEngineException
throw new ChartEngineException("Invalid chart data", new InvalidChartDataException("Invalid data points"));
```

## AnimationFrameGenerator

`AnimationFrameGenerator` is a class responsible for generating animation frames. It provides methods for generating frames and data frames, as well as accessing the current frame number, progress, and values.

```csharp
using System.Collections.Generic;
using SkiaSharpChartEngine.Rendering;

public class AnimationFrameGeneratorExample
{
    public static async Task Main(string[] args)
    {
        // Initialize animation frame generator
        var animationFrameGenerator = new AnimationFrameGenerator();

        // Generate frames
        var frames = animationFrameGenerator.GenerateFrames();
        Console.WriteLine($"Generated frames count: {frames.Count}");

        // Generate data frames
        var dataFrames = animationFrameGenerator.GenerateDataFrames();
        Console.WriteLine($"Generated data frames count: {dataFrames.Count}");

        // Get current frame number
        var frameNumber = animationFrameGenerator.FrameNumber;
        Console.WriteLine($"Current frame number: {frameNumber}");

        // Get current progress
        var progress = animationFrameGenerator.Progress;
        Console.WriteLine($"Current progress: {progress}");

        // Get current values
        var values = animationFrameGenerator.Values;
        Console.WriteLine($"Current values count: {values.Count}");
    }
}
```

## ChartEventPublisher

`ChartEventPublisher` implements the publish-subscribe pattern for chart events in the SkiaSharp chart engine. It allows components to subscribe to various chart events (creation, update, deletion, rendering, export, and errors) and notifies all registered subscribers when these events occur. The publisher provides thread-safe subscription management and asynchronous event broadcasting with comprehensive logging.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Events;

public class ChartEventPublisherExample
{
    public static async Task Main(string[] args)
    {
        // Initialize event publisher with logger
        var logger = new NullLogger<ChartEventPublisher>();
        var eventPublisher = new ChartEventPublisher(logger);

        // Check initial subscriber count
        Console.WriteLine($"Initial subscriber count: {eventPublisher.GetSubscriberCount()}");

        // Create a subscriber implementation
        var chartLogger = new ChartEventLoggerSubscriber();

        // Subscribe to events
        eventPublisher.Subscribe(chartLogger);
        Console.WriteLine($"After subscribe - Subscriber count: {eventPublisher.GetSubscriberCount()}");

        // Publish various chart events
        await eventPublisher.PublishChartCreatedAsync(
            new ChartCreatedEvent(Guid.NewGuid(), DateTime.UtcNow, "line-chart-001"));

        await eventPublisher.PublishChartUpdatedAsync(
            new ChartUpdatedEvent(Guid.NewGuid(), DateTime.UtcNow, "line-chart-001", "Updated title"));

        await eventPublisher.PublishChartRenderedAsync(
            new ChartRenderedEvent(Guid.NewGuid(), DateTime.UtcNow, "line-chart-001", 800, 600));

        await eventPublisher.PublishChartExportedAsync(
            new ChartExportedEvent(Guid.NewGuid(), DateTime.UtcNow, "line-chart-001", "chart.png"));

        await eventPublisher.PublishErrorAsync(
            new ChartErrorEvent(Guid.NewGuid(), DateTime.UtcNow, "Failed to render chart", new Exception("Rendering error")));

        // Unsubscribe
        eventPublisher.Unsubscribe(chartLogger);
        Console.WriteLine($"After unsubscribe - Subscriber count: {eventPublisher.GetSubscriberCount()}");
    }
}

// Example subscriber implementation
public class ChartEventLoggerSubscriber : IChartEventSubscriber
{
    public async Task OnChartCreatedAsync(ChartCreatedEvent @event)
    {
        Console.WriteLine($"[ChartCreated] Chart: {@event.ChartId}, Timestamp: {@event.Timestamp}");
        await Task.CompletedTask;
    }

    public async Task OnChartUpdatedAsync(ChartUpdatedEvent @event)
    {
        Console.WriteLine($"[ChartUpdated] Chart: {@event.ChartId}, Title: {@event.NewTitle}");
        await Task.CompletedTask;
    }

    public async Task OnChartDeletedAsync(ChartDeletedEvent @event)
    {
        Console.WriteLine($"[ChartDeleted] Chart: {@event.ChartId}");
        await Task.CompletedTask;
    }

    public async Task OnChartRenderedAsync(ChartRenderedEvent @event)
    {
        Console.WriteLine($"[ChartRendered] Chart: {@event.ChartId}, Size: {@event.Width}x{@event.Height}");
        await Task.CompletedTask;
    }

    public async Task OnChartExportedAsync(ChartExportedEvent @event)
    {
        Console.WriteLine($"[ChartExported] Chart: {@event.ChartId}, File: {@event.FilePath}");
        await Task.CompletedTask;
    }

    public async Task OnErrorAsync(ChartErrorEvent @event)
    {
        Console.WriteLine($"[Error] Chart: {@event.ChartId}, Message: {@event.Message}");
        await Task.CompletedTask;
    }
}
```

## EventDispatcher

`EventDispatcher` is a central event dispatcher implementing the publish-subscribe pattern. It manages event subscriptions and dispatches events to all registered handlers, supporting both synchronous and asynchronous event handling. The dispatcher provides thread-safe subscription management, comprehensive logging, and utilities for inspecting the current subscription state.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Events;

public class EventDispatcherExample
{
    public static async Task Main(string[] args)
    {
        // Initialize dispatcher with logger
        var logger = new NullLogger<EventDispatcher>();
        var dispatcher = new EventDispatcher(logger);

        // Check initial handler count
        Console.WriteLine($"Initial handler count for 'chart.created': {dispatcher.GetHandlerCount("chart.created")}");

        // Create handlers
        var syncHandler = new SyncEventHandler();
        var asyncHandler = new AsyncEventHandler();

        // Subscribe handlers
        dispatcher.Subscribe("chart.created", syncHandler);
        dispatcher.Subscribe("chart.created", asyncHandler);
        dispatcher.Subscribe("chart.updated", syncHandler);

        Console.WriteLine($"After subscribe - 'chart.created' handlers: {dispatcher.GetHandlerCount("chart.created")}");
        Console.WriteLine($"Subscribed event types: {string.Join(", ", dispatcher.GetSubscribedEventTypes())}");

        // Dispatch synchronous event
        dispatcher.Dispatch("chart.created", new { ChartId = "line-chart-001", Timestamp = DateTime.UtcNow });

        // Dispatch asynchronous event
        await dispatcher.DispatchAsync("chart.updated", new { 
            ChartId = "bar-chart-002", 
            Timestamp = DateTime.UtcNow,
            Changes = new[] { "Title", "DataPoints" }
        });

        // Unsubscribe
        dispatcher.Unsubscribe("chart.created", syncHandler);
        Console.WriteLine($"After unsubscribe - 'chart.created' handlers: {dispatcher.GetHandlerCount("chart.created")}");

        // Clear all handlers
        dispatcher.Clear();
        Console.WriteLine($"After clear - 'chart.updated' handlers: {dispatcher.GetHandlerCount("chart.updated")}");
    }
}

// Synchronous event handler implementation
public class SyncEventHandler : IEventHandler
{
    public void Handle(string eventType, object eventData)
    {
        Console.WriteLine($"[Sync] Received {eventType}: {System.Text.Json.JsonSerializer.Serialize(eventData)}");
    }
}

// Asynchronous event handler implementation
public class AsyncEventHandler : IAsyncEventHandler
{
    public async Task HandleAsync(string eventType, object eventData)
    {
        Console.WriteLine($"[Async] Processing {eventType}...");
        await Task.Delay(100); // Simulate async work
        Console.WriteLine($"[Async] Completed {eventType}");
    }
    
    public void Handle(string eventType, object eventData)
    {
        Console.WriteLine($"[Async] Sync fallback for {eventType}");
    }
}
```

## IChartEventSubscriber

`IChartEventSubscriber` is an interface that defines the contract for receiving chart events from the `ChartEventPublisher`. It provides a standardized way for components to react to various chart lifecycle events such as creation, updates, deletions, rendering, exports, and errors. Implementations of this interface can be registered with the event publisher to receive asynchronous notifications about chart events.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Events;

public class CustomChartEventSubscriber : IChartEventSubscriber
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string? SourceName { get; set; } = "CustomChartMonitor";
    public Dictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

    public virtual string GetEventName()
    {
        return "CustomChartEvent";
    }

    public Task OnChartCreatedAsync(ChartCreatedEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart created: {@event.ChartId}");
        Console.WriteLine($"  Event ID: {EventId}");
        Console.WriteLine($"  Timestamp: {Timestamp}");
        Console.WriteLine($"  Source: {SourceName}");
        
        Metadata["chartType"] = @event.ChartId.Contains("line") ? "line" : "other";
        Metadata["eventSource"] = SourceName;
        
        return Task.CompletedTask;
    }

    public Task OnChartUpdatedAsync(ChartUpdatedEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart updated: {@event.ChartId}");
        Console.WriteLine($"  New title: {@event.NewTitle}");
        Console.WriteLine($"  Modified fields: {string.Join(", ", @event.ModifiedFields ?? Array.Empty<string>())}");
        return Task.CompletedTask;
    }

    public Task OnChartDeletedAsync(ChartDeletedEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart deleted: {@event.ChartId}");
        Console.WriteLine($"  Deleted by: {@event.DeletedBy}");
        return Task.CompletedTask;
    }

    public Task OnChartRenderedAsync(ChartRenderedEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart rendered: {@event.ChartId}");
        Console.WriteLine($"  Dimensions: {@event.Width}x{@event.Height}");
        Console.WriteLine($"  Previous update: {@event.PreviousUpdateTime?.ToString("o") ?? "never"}");
        return Task.CompletedTask;
    }

    public Task OnChartExportedAsync(ChartExportedEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart exported: {@event.ChartId}");
        Console.WriteLine($"  File path: {@event.FilePath}");
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(ChartErrorEvent @event)
    {
        Console.WriteLine($"[CustomSubscriber] Chart error: {@event.ChartId}");
        Console.WriteLine($"  Error message: {@event.Message}");
        return Task.CompletedTask;
    }
}

// Usage example
public class Program
{
    public static async Task Main(string[] args)
    {
        // Create a custom subscriber
        var subscriber = new CustomChartEventSubscriber();
        
        Console.WriteLine($"Subscriber created with EventId: {subscriber.EventId}");
        Console.WriteLine($"Supported events: ChartCreated, ChartUpdated, ChartDeleted, ChartRendered, ChartExported, Error");
        
        // Simulate receiving events
        var createdEvent = new ChartCreatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "line-chart-001"
        );
        
        await subscriber.OnChartCreatedAsync(createdEvent);
        
        var updatedEvent = new ChartUpdatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "bar-chart-002",
            "Updated Bar Chart",
            new[] { "Title", "DataPoints" },
            DateTime.UtcNow.AddMinutes(-5)
        );
        
        await subscriber.OnChartUpdatedAsync(updatedEvent);
        
        var renderedEvent = new ChartRenderedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "pie-chart-003",
            1024,
            768
        );
        
        await subscriber.OnChartRenderedAsync(renderedEvent);
    }
}
```

## StringFormatBenchmarks

`StringFormatBenchmarks` is a benchmark suite that measures the performance of common string formatting operations used throughout the SkiaSharp chart engine. It focuses on hot paths for string formatting including camelCase/snake_case conversion, CSV generation, string repetition, and number formatting with units.

The benchmarks cover:
- **CamelCaseToTitleCase**: Converts camelCase strings to Title Case using `string.Create`
- **SnakeCaseToTitleCase**: Converts snake_case strings to Title Case using `string.Create`
- **ToCsvLine**: Generates CSV lines from arrays of values using pooled `StringBuilder`
- **Repeat**: Repeats strings multiple times efficiently
- **FormatNumberWithUnits**: Formats numbers with appropriate units (thousands, millions, billions)
- **FormatPercentage**: Formats numbers as percentages with specified decimal places

## MathHelperBenchmarks

`MathHelperBenchmarks` is a benchmark suite that measures the performance of mathematical operations used throughout the SkiaSharp chart engine. It focuses on hot paths for statistical calculations including min/max finding, averaging, and standard deviation computation. The benchmarks compare traditional `IEnumerable<T>`-based approaches against zero-allocation `ReadOnlySpan<T>`-based implementations to demonstrate performance improvements.

The benchmarks cover:
- **GetMinMax**: Finds minimum and maximum values in a dataset
- **Average**: Computes the arithmetic mean of values
- **StandardDeviation**: Calculates the standard deviation (measure of data dispersion)

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Benchmarks;
using SkiaSharpChartEngine.Utilities;

public class MathHelperBenchmarksExample
{
    public static void Main()
    {
        // Create sample data for analysis
        var random = new Random(42);
        var dataPoints = new float[1000];
        for (int i = 0; i < dataPoints.Length; i++)
        {
            dataPoints[i] = (float)(random.NextDouble() * 1000.0);
        }

        // Get min/max values using ReadOnlySpan (recommended for performance)
        var (min, max) = MathHelper.GetMinMax(dataPoints.AsSpan());
        Console.WriteLine($"Min: {min:F2}, Max: {max:F2}");
        // Example output: Min: 0.02, Max: 999.98

        // Calculate average using ReadOnlySpan
        float average = MathHelper.Average(dataPoints.AsSpan());
        Console.WriteLine($"Average: {average:F2}");
        // Example output: Average: 499.50

        // Calculate standard deviation using ReadOnlySpan
        float stdDev = MathHelper.StandardDeviation(dataPoints.AsSpan());
        Console.WriteLine($"Standard Deviation: {stdDev:F2}");
        // Example output: Standard Deviation: 288.67

        // For comparison: using IEnumerable (less efficient)
        var dataEnumerable = (IEnumerable<float>)dataPoints;
        var (minEnum, maxEnum) = MathHelper.GetMinMax(dataEnumerable);
        float averageEnum = MathHelper.Average(dataEnumerable);
        float stdDevEnum = MathHelper.StandardDeviation(dataEnumerable);
        
        Console.WriteLine($"\nIEnumerable results - Min: {minEnum:F2}, Max: {maxEnum:F2}, Avg: {averageEnum:F2}, StdDev: {stdDevEnum:F2}");
    }
}
```

```csharp
using System;
using SkiaSharpChartEngine.Benchmarks;
using SkiaSharpChartEngine.Utilities;

public class StringFormatBenchmarksExample
{
    public static void Main()
    {
        // CamelCaseToTitleCase example
        string camelCase = "salesPerformanceMonthlyDashboard";
        string titleCase = StringFormatHelper.CamelCaseToTitleCase(camelCase);
        Console.WriteLine($"CamelCase: {camelCase}");
        Console.WriteLine($"TitleCase: {titleCase}");
        // Output: TitleCase: Sales Performance Monthly Dashboard

        // SnakeCaseToTitleCase example
        string snakeCase = "sales_performance_monthly_dashboard";
        string titleCase2 = StringFormatHelper.SnakeCaseToTitleCase(snakeCase);
        Console.WriteLine($"SnakeCase: {snakeCase}");
        Console.WriteLine($"TitleCase: {titleCase2}");
        // Output: TitleCase: Sales Performance Monthly Dashboard

        // ToCsvLine example - format data for CSV export
        var csvValues = new object[] { "Monthly Revenue", 1234567.89, "North America", true, null };
        string csvLine = StringFormatHelper.ToCsvLine(csvValues);
        Console.WriteLine($"CSV Line: {csvLine}");
        // Output: CSV Line: "Monthly Revenue",1234567.89,North America,True,

        // Repeat example - create separator lines
        string separator = StringFormatHelper.Repeat("-", 50);
        Console.WriteLine(separator);
        // Output: --------------------------------------------------

        // FormatNumberWithUnits examples
        string formattedBillions = StringFormatHelper.FormatNumberWithUnits(1234567890);
        Console.WriteLine($"Billions: {formattedBillions}");
        // Output: Billions: 1.23B

        string formattedThousands = StringFormatHelper.FormatNumberWithUnits(4567.5);
        Console.WriteLine($"Thousands: {formattedThousands}");
        // Output: Thousands: 4.57K

        // FormatPercentage example
        string percentage = StringFormatHelper.FormatPercentage(73.456, 2);
        Console.WriteLine($"Percentage: {percentage}");
        // Output: Percentage: 73.46%
    }
}
```

## CacheKeyBenchmarks

`CacheKeyBenchmarks` is a benchmark suite that measures the performance of cache key generation operations used throughout the SkiaSharp chart engine. It focuses on hot paths for cache key assembly including SHA-256 hashing, frozen-dictionary lookups, and composite key construction.

The benchmarks cover:
- **BuildRenderKey**: Generates a render cache key for chart images with dimensions and format
- **BuildSeriesKey**: Creates a cache key for chart series based on chart and series names
- **BuildAxisKey**: Builds a cache key for axis configuration with min/max values and tick count
- **BuildConfigurationKey_Line**: Generates configuration keys for line charts using frozen dictionary lookups
- **BuildConfigurationKey_Bar**: Generates configuration keys for bar charts using frozen dictionary lookups
- **BuildCompositeKey**: Assembles composite cache keys from multiple parameters (no LINQ)

```csharp
using System;
using SkiaSharpChartEngine.Benchmarks;
using SkiaSharpChartEngine.Constants;

public class CacheKeyBenchmarksExample
{
    public static void Main()
    {
        // BuildRenderKey example - generate a key for a 1920x1080 PNG chart
        string renderKey = CacheKeyBuilder.BuildRenderKey(
            chartId: "sales-dashboard-2024",
            width: 1920,
            height: 1080,
            scale: 1.0f,
            format: "PNG"
        );
        Console.WriteLine($"Render key: {renderKey}");
        // Example output: Render key: 9a3b4c5d6e7f8g9h0i1j2k3l4m5n6o7p8

        // BuildSeriesKey example - create a key for a specific series in a chart
        string seriesKey = CacheKeyBuilder.BuildSeriesKey(
            chartId: "quarterly-revenue-chart",
            seriesName: "Q3 2024 Sales Performance"
        );
        Console.WriteLine($"Series key: {seriesKey}");
        // Example output: Series key: 2p3q4r5s6t7u8v9w0x1y2z3a4b5c6d7e8

        // BuildAxisKey example - generate a key for axis configuration
        string axisKey = CacheKeyBuilder.BuildAxisKey(
            minValue: 0f,
            maxValue: 1000f,
            tickCount: 10
        );
        Console.WriteLine($"Axis key: {axisKey}");
        // Example output: Axis key: 3a4b5c6d7e8f9g0h1i2j3k4l5m6n7

        // BuildConfigurationKey for LineChart - get configuration key from ChartType enum
        string lineConfigKey = CacheKeyBuilder.BuildConfigurationKey(ChartType.LineChart);
        Console.WriteLine($"Line chart config key: {lineConfigKey}");
        // Example output: Line chart config key: 4b5c6d7e8f9g0h1i2j3k4l5m6n7o8

        // BuildConfigurationKey for BarChart - get configuration key from ChartType enum
        string barConfigKey = CacheKeyBuilder.BuildConfigurationKey(ChartType.BarChart);
        Console.WriteLine($"Bar chart config key: {barConfigKey}");
        // Example output: Bar chart config key: 5c6d7e8f9g0h1i2j3k4l5m6n7o8p9

        // BuildCompositeKey example - assemble a composite key from multiple parameters
        string compositeKey = CacheKeyBuilder.BuildCompositeKey(
            chartId: "monthly-report-2024",
            width: 800,
            height: 600,
            format: "PNG",
            version: "v2.1.0"
        );
        Console.WriteLine($"Composite key: {compositeKey}");
        // Example output: Composite key: 6d7e8f9g0h1i2j3k4l5m6n7o8p9q0
    }
}
```

## ChartInteractionEventArgs

`ChartInteractionEventArgs` provides the event data for user interactions with chart elements such as clicks, hovers, selections, and context menu gestures. It contains detailed information about the interaction including the pointer position, the chart region affected, and any data points or series that were hit during the interaction.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Events;
using SkiaSharpChartEngine.Models;

public class ChartInteractionHandler
{
    private readonly IChartInteractionService _interactionService;

    public ChartInteractionHandler(IChartInteractionService interactionService)
    {
        _interactionService = interactionService;
        _interactionService.Interaction += OnChartInteraction;
    }

    private void OnChartInteraction(object? sender, ChartInteractionEventArgs e)
    {
        Console.WriteLine($"Interaction at ({e.PointerX}, {e.PointerY})");
        Console.WriteLine($"Type: {e.InteractionType}");
        Console.WriteLine($"Region: {e.Region}");
        Console.WriteLine($"Series: {e.HitSeries?.Name ?? "None"}");
        Console.WriteLine($"DataPoint: {e.HitDataPoint?.Value ?? 0}");
        Console.WriteLine($"Tooltip: {e.TooltipText}");
        Console.WriteLine($"Timestamp: {e.Timestamp:O}");

        // Add custom metadata
        e.Metadata["userId"] = "user-123";
        e.Metadata["sessionId"] = Guid.NewGuid().ToString();

        // Handle selection-based interactions
        if (e.InteractionType == ChartInteractionType.Select && e.HitDataPoint != null)
        {
            Console.WriteLine($"Selected point: Series={e.SeriesIndex}, Value={e.HitDataPoint.Value}");
        }
    }
}

// Example usage with different interaction types
public class InteractionExamples
{
    public static void Main()
    {
        var now = DateTime.UtcNow;
        var metadata = new Dictionary<string, object>
        {
            ["chartId"] = "sales-line-chart",
            ["userRole"] = "analyst"
        };

        // Click interaction
        var clickArgs = new ChartInteractionEventArgs
        {
            InteractionType = ChartInteractionType.Click,
            PointerX = 150.5f,
            PointerY = 200.75f,
            Region = ChartRegion.PlotArea,
            HitSeries = new ChartSeries { Name = "Sales" },
            HitDataPoint = new DataPoint { X = 3, Y = 1500 },
            SeriesIndex = 0,
            TooltipText = "Q3 2024: $1,500",
            Metadata = metadata
        };

        Console.WriteLine("Click interaction:");
        Console.WriteLine($"  Position: ({clickArgs.PointerX}, {clickArgs.PointerY})");
        Console.WriteLine($"  DataPoint: X={clickArgs.HitDataPoint?.X}, Y={clickArgs.HitDataPoint?.Y}");
        Console.WriteLine($"  Tooltip: {clickArgs.TooltipText}");

        // Hover interaction
        var hoverArgs = new ChartInteractionEventArgs
        {
            InteractionType = ChartInteractionType.Hover,
            PointerX = 200.0f,
            PointerY = 180.0f,
            Region = ChartRegion.AxisX,
            HitSeries = null,
            HitDataPoint = null,
            SeriesIndex = -1,
            TooltipText = "",
            Metadata = new Dictionary<string, object>()
        };

        Console.WriteLine("\nHover interaction:");
        Console.WriteLine($"  Position: ({hoverArgs.PointerX}, {hoverArgs.PointerY})");
        Console.WriteLine($"  SeriesIndex: {hoverArgs.SeriesIndex}");
    }
}
```
