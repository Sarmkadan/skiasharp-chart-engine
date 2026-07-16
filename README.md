# SkiaSharp Chart Engine

A .NET chart rendering library (SkiaSharp-based) with an ASP.NET Core Web API host. Usable standalone via `ChartEngine.Create()` or as a service through `AddSkiaSharpChartEngine()`.

## Architecture

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the component breakdown, data flow, design decisions, extension points, and known limitations. The sections below are per-type usage examples.

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

## AsyncDataLoader

`AsyncDataLoader` is a utility class for asynchronously loading chart data from various sources including JSON files, CSV files, and directories. It provides methods for loading individual charts or multiple charts from file systems, with automatic format detection and validation.

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Integration;
using SkiaSharpChartEngine.Models;

public class AsyncDataLoaderExample
{
    public static async Task Main(string[] args)
    {
        // Initialize the data loader with logger
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<AsyncDataLoader>();
        var dataLoader = new AsyncDataLoader(logger);

        // Example 1: Load a single chart from a JSON file
        var jsonChart = await dataLoader.LoadChartFromFileAsync("chart-data.json");
        Console.WriteLine(jsonChart != null
            ? $"Loaded JSON chart: {jsonChart.Title}"
            : "Failed to load JSON chart");

        // Example 2: Load a chart from a CSV file
        var csvChart = await dataLoader.LoadChartFromFileAsync("sales-data.csv");
        Console.WriteLine(csvChart != null
            ? $"Loaded CSV chart with {csvChart.Series.Count} series"
            : "Failed to load CSV chart");

        // Example 3: Load multiple charts from a directory
        var chartsFromDir = await dataLoader.LoadChartsFromDirectoryAsync(
            "./charts/",
            "*.json"
        );
        Console.WriteLine($"Loaded {chartsFromDir.Count} charts from directory");

        // Example 4: Parse CSV data directly
        var csvData = "Quarter,Revenue,Expenses\nQ1,100000,85000\nQ2,125000,92000";
        var dataPoints = dataLoader.ParseCsvData(csvData);
        Console.WriteLine($"Parsed {dataPoints.Count} data points from CSV");

        // Example 5: Check if a file can be loaded
        bool canLoad = dataLoader.CanLoadFile("chart.json");
        Console.WriteLine($"Can load file: {canLoad}");
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

## ChartRenderingPipeline

`ChartRenderingPipeline` is a pipeline processor for chart rendering operations. It executes a sequence of stages sequentially with support for middleware-style interceptors. The pipeline maintains execution context, tracks performance metrics, and provides detailed execution results for each stage.

The pipeline supports:
- Adding custom rendering stages
- Registering interceptors for before/after pipeline execution
- Tracking execution context and data across stages
- Measuring performance of individual stages
- Comprehensive error handling and reporting

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Pipeline;

public class ChartRenderingPipelineExample
{
    public static async Task Main(string[] args)
    {
        // Initialize pipeline with logger
        var logger = new NullLogger<ChartRenderingPipeline>();
        var pipeline = new ChartRenderingPipeline(logger);

        // Create a simple chart with data
        var chart = new Chart("sales-chart");
        var series = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        series.AddDataPoint(1.0, 100000.0);
        series.AddDataPoint(2.0, 125000.0);
        series.AddDataPoint(3.0, 150000.0);
        series.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(series);

        // Add a custom pipeline stage
        pipeline.AddStage(new DataValidationStage());
        
        // Add a pipeline interceptor for logging
        pipeline.AddInterceptor(new LoggingInterceptor());

        // Create pipeline context with initial data
        var context = new PipelineContext();
        context.Set("requestId", Guid.NewGuid());
        context.Set("userId", "user-123");

        // Execute the pipeline
        var result = await pipeline.ExecuteAsync(chart, context);

        // Check execution results
        Console.WriteLine($"Pipeline completed: {result.Success}");
        Console.WriteLine($"Total duration: {result.TotalDurationMs}ms");
        Console.WriteLine($"Successful stages: {result.SuccessfulStages}");
        Console.WriteLine($"Failed stages: {result.FailedStages}");

        if (!result.Success)
        {
            Console.WriteLine($"Error: {result.ErrorMessage}");
        }

        // Access stage-specific results
        foreach (var stageResult in result.StageResults)
        {
            Console.WriteLine($"Stage '{stageResult.StageName}': {stageResult.Success} in {stageResult.DurationMs}ms");
            if (!stageResult.Success && stageResult.Message != null)
            {
                Console.WriteLine($"  Error: {stageResult.Message}");
            }
        }

        // Access context data after execution
        var requestId = context.Get<Guid>("requestId");
        Console.WriteLine($"Request ID from context: {requestId}");
    }
}

// Example pipeline stage implementation
public class DataValidationStage : IPipelineStage
{
    public string Name => "DataValidation";

    public async Task<PipelineStageResult> ExecuteAsync(
        Chart chart, 
        PipelineContext context, 
        System.Threading.CancellationToken cancellationToken)
    {
        // Validate chart has data
        if (chart.Series.Count == 0)
        {
            return PipelineStageResult.Failure("Chart has no series data");
        }

        // Validate each series has data points
        foreach (var series in chart.Series)
        {
            if (series.DataPoints.Count == 0)
            {
                return PipelineStageResult.Failure($"Series '{series.Name}' has no data points");
            }
        }

        // Store validation result in context
        context.Set("validationPassed", true);
        context.Set("dataQualityScore", 95.5);

        return PipelineStageResult.Success("Data validated successfully");
    }
}

// Example pipeline interceptor implementation
public class LoggingInterceptor : IPipelineInterceptor
{
    public async Task OnBeforeAsync(
        PipelineContext context, 
        System.Threading.CancellationToken cancellationToken)
    {
        var requestId = context.Get<Guid>("requestId");
        Console.WriteLine($"[Before] Pipeline starting - Request ID: {requestId}");
        await Task.CompletedTask;
    }

    public async Task OnAfterAsync(
        PipelineResult result, 
        PipelineContext context, 
        System.Threading.CancellationToken cancellationToken)
    {
        var requestId = context.Get<Guid>("requestId");
        var duration = result.TotalDurationMs;
        Console.WriteLine($"[After] Pipeline completed - Request ID: {requestId}, Duration: {duration}ms, Success: {result.Success}");
        await Task.CompletedTask;
    }
}
```

## AnimationFrameGenerator

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

## ReportSection

`ReportSection` represents a single section within a PDF report that can contain an optional heading, descriptive body text, and a chart visualization. Each section supports configurable layout options like image scaling behavior and automatic page breaks, making it easy to create multi-section reports with mixed text and chart content.

```csharp
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Reports;

// Create a simple text-only section
var textSection = new ReportSection
{
    Heading = "Executive Summary",
    BodyText = "This report contains quarterly performance metrics and key insights."
};

// Create a chart section with automatic page break
var chart = new Chart("quarterly-sales");
var series = new ChartSeries("Revenue")
{
    LineWidth = 2.5f,
    Color = "#2E86C1"
};
series.AddDataPoint(1.0, 100000.0);
series.AddDataPoint(2.0, 125000.0);
series.AddDataPoint(3.0, 150000.0);
series.AddDataPoint(4.0, 175000.0);
chart.AddSeries(series);

var chartSection = new ReportSection
{
    Heading = "Quarterly Revenue Analysis",
    BodyText = "Sales performance across all regions for Q2 2024.",
    Chart = chart,
    ImageFit = PdfImageFit.FitWidth,
    PageBreakBefore = true
};

// Create a section with original image size (may clip if too large)
var fullSizeSection = new ReportSection
{
    Heading = "High Resolution Chart",
    Chart = chart,
    ImageFit = PdfImageFit.Original
};
```

## QuickStartExample

`QuickStartExample` provides a collection of practical examples demonstrating common usage patterns for the SkiaSharp chart engine. It showcases how to create various chart types, configure chart properties, render charts to images, export charts to files, validate chart configurations, work with color palettes, and perform asynchronous rendering operations.

```csharp
using System;
using System.Threading.Tasks;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class QuickStartExampleUsage
{
    public static void Main()
    {
        // Example 1: Create a simple line chart
        QuickStartExample.CreateSimpleLineChart();
        
        // Example 2: Create a multi-series chart with custom configuration
        QuickStartExample.CreateMultiSeriesChart();
        
        // Example 3: Export a chart to a file
        QuickStartExample.ExportChartToFile();
        
        // Example 4: Validate a chart before rendering
        QuickStartExample.ValidateChartBeforeRendering();
        
        // Example 5: Generate and visualize different color palettes
        QuickStartExample.GenerateAndVisualizePaletteColors();
        
        // Example 6: Perform asynchronous chart rendering
        QuickStartExample.RenderChartAsync().GetAwaiter().GetResult();
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

## PdfReportGeneratorTests

`PdfReportGeneratorTests` provides a comprehensive suite of unit tests for the `PdfReportGenerator` class, which generates PDF reports containing chart visualizations and formatted text content. The tests validate PDF generation with various section configurations, error handling for null inputs, and proper interaction with the chart rendering service.

The test suite covers:
- **Null argument validation**: Ensuring `GenerateAsync` and `GenerateToFileAsync` throw `ArgumentNullException` for invalid inputs
- **Empty sections handling**: Testing PDF generation with empty section lists
- **Text-only reports**: Validating PDF creation with only text content (headings and body text)
- **Chart integration**: Verifying chart rendering service is called for chart sections and multiple charts produce correct output
- **Error resilience**: Ensuring PDF generation succeeds even when chart rendering fails
- **File system operations**: Testing successful file writing with `GenerateToFileAsync`

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Reports;
using SkiaSharpChartEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class PdfReportGeneratorTestsExample
{
    public static async Task Main()
    {
        // Initialize PDF report generator with dependencies
        var logger = new NullLogger<PdfReportGenerator>();
        var renderingService = new ChartRenderingService(
            logger,
            new ChartDataService(logger),
            new RenderCacheService()
        );
        var pdfGenerator = new PdfReportGenerator(renderingService, logger);

        // Example 1: Generate a PDF report with text-only sections
        var textSections = new List<ReportSection>
        {
            new ReportSection
            {
                Heading = "Executive Summary",
                BodyText = "This report contains quarterly performance metrics and analysis."
            },
            new ReportSection
            {
                Heading = "Key Findings",
                BodyText = "Revenue increased by 15% compared to last quarter. Customer satisfaction reached record high."
            }
        };

        var pdfBytes = await pdfGenerator.GenerateAsync(textSections);
        Console.WriteLine($"Generated PDF with text sections: {pdfBytes.Length} bytes");

        // Example 2: Generate a PDF report with chart sections
        var chart = new Chart("quarterly-sales");
        var salesSeries = new ChartSeries("Sales Revenue");
        salesSeries.AddDataPoint(1.0, 100000.0);
        salesSeries.AddDataPoint(2.0, 125000.0);
        salesSeries.AddDataPoint(3.0, 150000.0);
        salesSeries.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(salesSeries);

        var chartSections = new List<ReportSection>
        {
            new ReportSection
            {
                Heading = "Quarterly Revenue Analysis",
                BodyText = "Sales performance across all regions.",
                Chart = chart
            },
            new ReportSection
            {
                Heading = "Market Share Distribution",
                BodyText = "Regional breakdown of market share.",
                Chart = CreatePieChart(),
                PageBreakBefore = true
            }
        };

        var chartPdfBytes = await pdfGenerator.GenerateAsync(chartSections);
        Console.WriteLine($"Generated PDF with chart sections: {chartPdfBytes.Length} bytes");

        // Example 3: Generate and save PDF to file
        var outputPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            $"report-{DateTime.Now:yyyyMMdd-HHmmss}.pdf"
        );

        await pdfGenerator.GenerateToFileAsync(outputPath, chartSections);
        Console.WriteLine($"PDF saved to: {outputPath}");
        Console.WriteLine($"File exists: {File.Exists(outputPath)}");

        // Cleanup
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }
    }

    private static Chart CreatePieChart()
    {
        var chart = new Chart("market-share");
        var series = new ChartSeries("Market Share");
        series.AddDataPoint(1.0, 35.0);
        series.AddDataPoint(2.0, 25.0);
        series.AddDataPoint(3.0, 20.0);
        series.AddDataPoint(4.0, 20.0);
        chart.AddSeries(series);
        return chart;
    }
}
```

## MathHelperTests

`MathHelperTests` is a comprehensive unit test suite that validates the mathematical utility methods in the `MathHelper` class. These utilities provide essential chart rendering calculations including data normalization, statistical analysis, and value clamping operations. The tests cover edge cases, boundary conditions, and ensure mathematical correctness for chart visualization scenarios.

The test suite covers:
- **GetMinMax**: Validates minimum and maximum value extraction from collections
- **Normalize**: Tests normalization of values to [0, 1] range with special handling for equal bounds
- **Lerp**: Verifies linear interpolation with boundary clamping
- **StandardDeviation**: Ensures correct statistical calculations for data dispersion
- **GenerateAxisTicks**: Validates axis tick generation for chart rendering
- **Clamp**: Tests value clamping to specified minimum and maximum bounds

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Utilities;

public class MathHelperTestsExample
{
    public static void Main()
    {
        // Example 1: GetMinMax with a collection of values
        var values = new List<double> { 10.5, 25.3, 15.7, 30.1, 8.2 };
        var (min, max) = MathHelper.GetMinMax(values);
        Console.WriteLine($"Min: {min:F2}, Max: {max:F2}");
        // Output: Min: 8.20, Max: 30.10

        // Example 2: Normalize values to [0, 1] range
        var dataPoints = new List<(double X, double Y)>
        {
            (1.0, 100.0),
            (2.0, 200.0),
            (3.0, 150.0)
        };
        
        // Normalize X values
        MathHelper.Normalize(dataPoints, p => p.X);
        Console.WriteLine($"Normalized X - Min: {dataPoints[0].X:F4}, Max: {dataPoints[2].X:F4}");
        // Output: Normalized X - Min: 0.0000, Max: 1.0000
        
        // Normalize Y values
        MathHelper.Normalize(dataPoints, p => p.Y);
        Console.WriteLine($"Normalized Y - Min: {dataPoints[0].Y:F4}, Max: {dataPoints[1].Y:F4}");
        // Output: Normalized Y - Min: 0.0000, Max: 1.0000

        // Example 3: Linear interpolation (Lerp) with boundary clamping
        double start = 10.0;
        double end = 50.0;
        
        // Interpolate at midpoint (t = 0.5)
        double midpoint = MathHelper.Lerp(start, end, 0.5);
        Console.WriteLine($"Lerp midpoint: {midpoint:F2}");
        // Output: Lerp midpoint: 30.00
        
        // Interpolate with t > 1 (clamps to end value)
        double clamped = MathHelper.Lerp(start, end, 1.5);
        Console.WriteLine($"Lerp with t > 1: {clamped:F2}");
        // Output: Lerp with t > 1: 50.00
        
        // Interpolate with t = 0 (returns start value)
        double startValue = MathHelper.Lerp(start, end, 0.0);
        Console.WriteLine($"Lerp with t = 0: {startValue:F2}");
        // Output: Lerp with t = 0: 10.00

        // Example 4: Calculate standard deviation for statistical analysis
        var measurements = new List<double> { 9.8, 10.2, 9.9, 10.1, 10.0 };
        double stdDev = MathHelper.StandardDeviation(measurements);
        Console.WriteLine($"Standard deviation: {stdDev:F4}");
        // Output: Standard deviation: 0.1414

        // Example 5: Generate axis ticks for chart rendering
        var ticks = MathHelper.GenerateAxisTicks(0.0, 100.0, 11);
        Console.WriteLine($"Generated {ticks.Count} axis ticks");
        Console.WriteLine($"First tick: {ticks[0]:F2}, Last tick: {ticks[^1]:F2}");
        // Output: Generated 11 axis ticks
        // Output: First tick: 0.00, Last tick: 100.00

        // Example 6: Clamp values to ensure they stay within valid ranges
        double valueAboveMax = 150.0;
        double clampedValue = MathHelper.Clamp(valueAboveMax, 0.0, 100.0);
        Console.WriteLine($"Clamped value above max: {clampedValue:F2}");
        // Output: Clamped value above max: 100.00
        
        double valueBelowMin = -10.0;
        double clampedValue2 = MathHelper.Clamp(valueBelowMin, 0.0, 100.0);
        Console.WriteLine($"Clamped value below min: {clampedValue2:F2}");
        // Output: Clamped value below min: 0.00
    }
}
```

## HttpChartClient

`HttpChartClient` is an HTTP client for communicating with remote chart services. It provides methods for fetching charts, posting chart data, and downloading rendered chart images from a remote chart service endpoint. The client handles authentication headers, request serialization, and comprehensive error logging.

```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Integration;
using SkiaSharpChartEngine.Models;

public class HttpChartClientExample
{
    public static async Task Main(string[] args)
    {
        // Initialize HttpChartClient with base URL and logger
        var logger = new NullLogger<HttpChartClient>();
        var serializer = new JsonChartSerializer();
        var httpChartClient = new HttpChartClient(
            "https://api.chart-service.example.com",
            logger,
            serializer
        );

        // Set authentication token for API access
        httpChartClient.SetAuthenticationToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");

        // Set custom headers if needed
        httpChartClient.SetDefaultHeaders("X-API-Version", "v2.1");
        httpChartClient.SetDefaultHeaders("X-Request-ID", Guid.NewGuid().ToString());

        try
        {
            // Example 1: Fetch a chart by ID
            var chart = await httpChartClient.GetChartAsync("sales-dashboard-2024");
            Console.WriteLine(chart != null
                ? $"Fetched chart: {chart.Id} - {chart.Title}"
                : "Chart not found");

            // Example 2: Post a new chart to remote service
            var newChart = new Chart("monthly-report-q2")
            {
                Title = "Q2 2024 Monthly Report",
                Description = "Monthly sales performance report"
            };
            newChart.AddSeries(new ChartSeries("Revenue")
            {
                LineWidth = 2.5f,
                Color = "#2E86C1"
            });
            
            bool posted = await httpChartClient.PostChartAsync(newChart);
            Console.WriteLine($"Chart posted successfully: {posted}");

            // Example 3: Download rendered chart image
            var chartImageData = await httpChartClient.GetRenderedChartAsync(
                "sales-dashboard-2024",
                format: "png"
            );
            
            if (chartImageData != null)
            {
                Console.WriteLine($"Downloaded chart image: {chartImageData.Length} bytes");
                // Save to file or process the image data
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chart service error: {ex.Message}");
        }
        finally
        {
            // Cleanup
            httpChartClient.Dispose();
        }
    }
}
```

## ExternalApiClient

`ExternalApiClient` is an HTTP client for integrating with external APIs. It provides a convenient wrapper around `HttpClient` with built-in retry logic, error handling, request/response serialization, and header management. The client supports common HTTP methods (GET, POST, PUT, DELETE) and automatically handles JSON serialization/deserialization.

```csharp
using System;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class ExternalApiClientExample
{
    public static async Task Main(string[] args)
    {
        // Initialize HttpClient and logger
        var httpClient = new System.Net.Http.HttpClient();
        var logger = new NullLogger<ExternalApiClient>();
        
        // Create API client with 3 retries and 1 second base delay
        var apiClient = new ExternalApiClient(httpClient, logger, maxRetries: 3);
        
        // Set authorization header (e.g., Bearer token)
        apiClient.SetAuthorizationHeader("Bearer", "your-access-token-here");
        
        // Add custom headers
        apiClient.AddHeader("X-Custom-Header", "custom-value");
        apiClient.AddHeader("Accept", "application/json");
        
        try
        {
            // Example 1: GET request to fetch data
            var weatherData = await apiClient.GetAsync<WeatherResponse>(
                "https://api.weather.com/v1/current?location=NewYork"
            );
            Console.WriteLine($"Temperature: {weatherData?.Temperature}°C");
            
            // Example 2: POST request to create a resource
            var newChart = new { Name = "Sales Dashboard", Type = "line" };
            var createdChart = await apiClient.PostAsync<Chart>(
                "https://api.example.com/charts",
                newChart
            );
            Console.WriteLine($"Created chart with ID: {createdChart?.Id}");
            
            // Example 3: PUT request to update a resource
            var updateData = new { Title = "Updated Sales Dashboard" };
            var updatedChart = await apiClient.PutAsync<Chart>(
                $"https://api.example.com/charts/{createdChart?.Id}",
                updateData
            );
            Console.WriteLine($"Updated chart: {updatedChart?.Name}");
            
            // Example 4: DELETE request to remove a resource
            bool deleted = await apiClient.DeleteAsync(
                $"https://api.example.com/charts/{createdChart?.Id}"
            );
            Console.WriteLine($"Resource deleted: {deleted}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API request failed: {ex.Message}");
        }
    }
}

// Example response model
public class WeatherResponse
{
    public double Temperature { get; set; }
    public double Humidity { get; set; }
    public string Location { get; set; }
}

// Example chart model
public class Chart
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
}
```

## InteractivityExtensions

`InteractivityExtensions` provides extension methods for adding interactive features to charts, including tooltip hit-testing, zooming, panning, and viewport management. These methods allow you to implement rich user interactions like hover tooltips, zoom/pan gestures, and reset functionality in your chart applications.

The extension methods can be used with or without dependency injection - they include a built-in default service for standalone scenarios while also providing the `AddChartInteractivity()` method to register the service in your DI container.

```csharp
using System;
using SkiaSharp;
using SkiaSharpChartEngine.Extensions;
using SkiaSharpChartEngine.Models;

public class InteractivityExample
{
    public static void Main()
    {
        // Create a chart with sample data
        var chart = new Chart("sales-chart");
        var series = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        series.AddDataPoint(1.0, 100000.0);
        series.AddDataPoint(2.0, 125000.0);
        series.AddDataPoint(3.0, 150000.0);
        series.AddDataPoint(4.0, 175000.0);
        chart.AddSeries(series);

        // Example 1: Get tooltip at specific coordinates
        var tooltipResult = chart.GetTooltipAt(
            pointerX: 200,
            pointerY: 300,
            canvasWidth: 800,
            canvasHeight: 600,
            options: new TooltipOptions { HitRadius = 10 }
        );

        if (tooltipResult.IsHit)
        {
            Console.WriteLine($"Tooltip: {tooltipResult.TooltipText}");
            Console.WriteLine($"Data point: X={tooltipResult.DataPoint?.X}, Y={tooltipResult.DataPoint?.Y}");
        }

        // Example 2: Zoom in at a specific point (2x zoom)
        var currentViewport = chart.ResetViewport();
        var zoomedViewport = chart.ZoomAt(
            current: currentViewport,
            anchorX: 400,
            anchorY: 300,
            canvasWidth: 800,
            canvasHeight: 600,
            factor: 2.0
        );

        Console.WriteLine($"Zoomed viewport: {zoomedViewport.ZoomLevel:P0}");

        // Example 3: Pan the viewport by 50 pixels right
        var pannedViewport = chart.PanBy(
            current: zoomedViewport,
            deltaX: 50,
            deltaY: 0,
            canvasWidth: 800,
            canvasHeight: 600
        );

        Console.WriteLine($"Panned viewport: X offset={pannedViewport.XOffset}");

        // Example 4: Reset viewport to show all data
        var fullViewport = chart.ResetViewport();
        Console.WriteLine("Reset to full viewport");

        // Example 5: Async tooltip hit-testing
        var asyncTooltip = await chart.GetTooltipAtAsync(
            pointerX: 250,
            pointerY: 250,
            canvasWidth: 800,
            canvasHeight: 600
        );

        Console.WriteLine($"Async tooltip hit: {asyncTooltip.IsHit}");

        // Example 6: Using SKPoint overload for MAUI/Blazor coordinates
        var skPoint = new SKPoint(300, 200);
        var tooltipFromPoint = chart.GetTooltipAt(
            point: skPoint,
            canvasWidth: 800,
            canvasHeight: 600
        );
    }
}
```

## StringExtensions

`StringExtensions` provides a comprehensive set of utility extension methods for string manipulation and conversion. It includes methods for converting between different string formats (camelCase, snake_case, PascalCase, kebab-case), parsing and converting numeric values, truncating strings, checking string properties, and various text transformations.

```csharp
using System;
using SkiaSharpChartEngine.Extensions;

public class StringExtensionsExample
{
    public static void Main()
    {
        // Example 1: Convert between different string formats
        string camelCase = "salesPerformanceMonthlyDashboard";
        string kebabCase = camelCase.ToKebabCase();
        Console.WriteLine($"Kebab case: {kebabCase}");
        // Output: Kebab case: sales-performance-monthly-dashboard

        string snakeCase = "quarterly_revenue_report";
        string pascalCase = snakeCase.ToPascalCase();
        Console.WriteLine($"Pascal case: {pascalCase}");
        // Output: Pascal case: QuarterlyRevenueReport

        // Example 2: Parse strings to numeric values with fallback
        string numberString = "42.75";
        double parsedDouble = numberString.ToDoubleOrDefault();
        Console.WriteLine($"Parsed double: {parsedDouble}");
        // Output: Parsed double: 42.75

        string invalidNumber = "not-a-number";
        double defaultValue = invalidNumber.ToDoubleOrDefault(999.99);
        Console.WriteLine($"Invalid number fallback: {defaultValue}");
        // Output: Invalid number fallback: 999.99

        // Example 3: Truncate long strings with ellipsis
        string longText = "This is a very long text that needs to be truncated";
        string truncated = longText.Truncate(20);
        Console.WriteLine($"Truncated: {truncated}");
        // Output: Truncated: This is a very long...

        // Example 4: Check string properties
        string hexColor = "#FF5733";
        bool isValidHex = hexColor.IsValidHexColor();
        Console.WriteLine($"Is valid hex color: {isValidHex}");
        // Output: Is valid hex color: True

        string numericString = "12345";
        bool containsDigits = numericString.ContainsDigit();
        Console.WriteLine($"Contains digits: {containsDigits}");
        // Output: Contains digits: True

        // Example 5: Text transformations
        string mixedCase = "hElLo WoRlD";
        string capitalized = mixedCase.CapitalizeFirstLetter();
        Console.WriteLine($"Capitalized: {capitalized}");
        // Output: Capitalized: HElLo WoRlD

        string whitespaceText = "  Hello   World  ";
        string noWhitespace = whitespaceText.RemoveWhitespace();
        Console.WriteLine($"No whitespace: '{noWhitespace}'");
        // Output: No whitespace: 'HelloWorld'

        // Example 6: Extract numbers from strings
        string alphanumeric = "Product ID: 12345, Version: 2.1.0";
        string numbersOnly = alphanumeric.ExtractNumbers();
        Console.WriteLine($"Extracted numbers: {numbersOnly}");
        // Output: Extracted numbers: 12345210

        // Example 7: Repeat strings for padding or separators
        string separator = "=".Repeat(30);
        Console.WriteLine(separator);
        // Output: ================================

        // Example 8: Check if string is a palindrome
        string palindrome = "madam";
        bool isPalindrome = palindrome.IsPalindrome();
        Console.WriteLine($"Is palindrome: {isPalindrome}");
        // Output: Is palindrome: True

        string notPalindrome = "hello";
        bool isNotPalindrome = notPalindrome.IsPalindrome();
        Console.WriteLine($"Is palindrome: {isNotPalindrome}");
        // Output: Is palindrome: False
    }
}
```

## DataPointExtensions

`DataPointExtensions` provides utility methods for working with `DataPoint` objects, enabling common geometric and statistical operations on chart data points. It includes distance calculations, proximity checks, and transformation methods for both individual points and collections of points.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Extensions;
using SkiaSharpChartEngine.Models;

public class DataPointExtensionsExample
{
    public static void Main()
    {
        // Create sample data points
        var pointA = new DataPoint(1.0, 2.0);
        var pointB = new DataPoint(4.0, 6.0);
        var pointC = new DataPoint(1.0, 2.0);
        
        // Example 1: Calculate Euclidean distance between two points
        double distance = pointA.GetDistance(pointB);
        Console.WriteLine($"Distance between A and B: {distance:F2}");
        // Output: Distance between A and B: 5.00
        
        // Example 2: Check if two points are near each other (within tolerance)
        bool isNear = pointA.IsNear(pointB, 0.1);
        Console.WriteLine($"Points A and B are near: {isNear}");
        // Output: Points A and B are near: False
        
        // Example 3: Offset a single data point by specified X and Y values
        var offsetPoint = pointA.Offset(3.0, 1.0);
        Console.WriteLine($"Offset point: [{offsetPoint.X}, {offsetPoint.Y}]");
        // Output: Offset point: [4, 3]
        
        // Example 4: Scale a single data point by specified factors
        var scaledPoint = pointA.Scale(2.0, 0.5);
        Console.WriteLine($"Scaled point: [{scaledPoint.X:F2}, {scaledPoint.Y:F2}]");
        // Output: Scaled point: [2.00, 1.00]
        
        // Example 5: Offset all points in a collection
        var points = new List<DataPoint> { pointA, pointB, new DataPoint(7.0, 8.0) };
        var offsetPoints = points.Offset(10.0, 5.0);
        Console.WriteLine("Offset points:");
        foreach (var p in offsetPoints)
        {
            Console.WriteLine($"  [{p.X}, {p.Y}]");
        }
        // Output: Offset points:
        //   [11, 7]
        //   [14, 11]
        //   [17, 13]
        
        // Example 6: Scale all points in a collection
        var scaledPoints = points.Scale(0.5, 2.0);
        Console.WriteLine("Scaled points:");
        foreach (var p in scaledPoints)
        {
            Console.WriteLine($"  [{p.X:F2}, {p.Y:F2}]");
        }
        // Output: Scaled points:
        //   [0.50, 4.00]
        //   [2.00, 12.00]
        //   [3.50, 16.00]
        
        // Example 7: Get bounds (min/max X and Y) for a collection of points
        var bounds = points.GetBounds();
        Console.WriteLine($"Bounds: X=[{bounds.minX:F2}, {bounds.maxX:F2}], Y=[{bounds.minY:F2}, {bounds.maxY:F2}]");
        // Output: Bounds: X=[1.00, 7.00], Y=[2.00, 8.00]
        
        // Example 8: Calculate average X coordinate across multiple points
        double avgX = points.GetAverageX();
        Console.WriteLine($"Average X: {avgX:F2}");
        // Output: Average X: 4.00
        
        // Example 9: Calculate average Y coordinate across multiple points
        double avgY = points.GetAverageY();
        Console.WriteLine($"Average Y: {avgY:F2}");
        // Output: Average Y: 5.33
    }
}
```

## ChartExtensions

`ChartExtensions` provides a set of extension methods for enhancing and transforming `Chart` objects. These methods enable common chart operations like applying default configurations, calculating axis bounds, counting data points, checking for empty charts, applying color palettes, normalizing data across series, filtering series, and converting charts to builders for further customization.

The extension methods work with existing chart objects and return either the modified chart (for in-place operations) or a new chart instance (for functional-style operations), allowing for method chaining and immutable-style transformations.

```csharp
using System;
using SkiaSharpChartEngine.Extensions;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class ChartExtensionsExample
{
    public static void Main()
    {
        // Create a chart with sample data
        var chart = new Chart("sales-performance-chart");
        
        var revenueSeries = new ChartSeries("Revenue")
        {
            LineWidth = 2.5f,
            Color = "#2E86C1"
        };
        revenueSeries.AddDataPoint(1.0, 100000.0);
        revenueSeries.AddDataPoint(2.0, 125000.0);
        revenueSeries.AddDataPoint(3.0, 150000.0);
        revenueSeries.AddDataPoint(4.0, 175000.0);
        
        var expensesSeries = new ChartSeries("Expenses")
        {
            LineWidth = 2.5f,
            Color = "#E74C3C"
        };
        expensesSeries.AddDataPoint(1.0, 85000.0);
        expensesSeries.AddDataPoint(2.0, 92000.0);
        expensesSeries.AddDataPoint(3.0, 98000.0);
        expensesSeries.AddDataPoint(4.0, 105000.0);
        
        chart.AddSeries(revenueSeries);
        chart.AddSeries(expensesSeries);

        // Example 1: Apply default configuration
        var configuredChart = chart.WithDefaultConfiguration();
        Console.WriteLine($"Chart configured with type: {configuredChart.Type}");

        // Example 2: Get axis bounds for the chart
        var (minX, maxX, minY, maxY) = chart.GetAxisBounds();
        Console.WriteLine($"Axis bounds - X: [{minX:F2}, {maxX:F2}], Y: [{minY:F2}, {maxY:F2}]");

        // Example 3: Count total data points
        int totalPoints = chart.GetTotalDataPoints();
        Console.WriteLine($"Total data points: {totalPoints}");

        // Example 4: Check if chart is empty
        bool isEmpty = chart.IsEmpty();
        Console.WriteLine($"Chart is empty: {isEmpty}");

        // Example 5: Apply a color palette
        var palette = new ColorPalette(new[] { "#2E86C1", "#E74C3C", "#27AE60", "#F39C12" });
        var chartWithPalette = chart.ApplyPalette(palette);
        Console.WriteLine($"Applied palette with {palette.GetColorCount()} colors");

        // Example 6: Normalize data across all series
        var normalizedChart = chart.NormalizeData(clone: true);
        Console.WriteLine("Data normalized to 0-1 range");

        // Example 7: Filter series (keep only series with high values)
        var highValueChart = chart.FilterSeries(
            series => series.DataPoints.Any(p => p.Y > 100000),
            clone: true
        );
        Console.WriteLine($"Filtered to {highValueChart.Series.Count} high-value series");

        // Example 8: Convert chart to builder for further customization
        var builder = chart.ToBuilder();
        Console.WriteLine($"Chart converted to builder with type: {builder.ChartType}");
    }
}
```

## CollectionExtensions

`CollectionExtensions` provides a set of utility extension methods for working with collections and sequences in a more convenient and expressive way. It includes methods for batching, filtering duplicates, checking for null/empty collections, getting random elements, shuffling, and various statistical operations.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Extensions;

public class CollectionExtensionsExample
{
    public static void Main()
    {
        // Example 1: Batch a large collection into smaller chunks
        var numbers = Enumerable.Range(1, 25);
        var batches = numbers.Batch(5);
        
        Console.WriteLine("Batches:");
        foreach (var batch in batches)
        {
            Console.WriteLine($"  {string.Join(", ", batch)}");
        }
        // Output: Batches: 1, 2, 3, 4, 5
        //         Batches: 6, 7, 8, 9, 10
        //         ...
        
        // Example 2: Check if a collection is null or empty
        List<string>? nullList = null;
        List<string> emptyList = new();
        var listWithItems = new List<string> { "item1", "item2" };
        
        Console.WriteLine($"Null list is null or empty: {nullList.IsNullOrEmpty()}");
        Console.WriteLine($"Empty list is null or empty: {emptyList.IsNullOrEmpty()}");
        Console.WriteLine($"List with items is null or empty: {listWithItems.IsNullOrEmpty()}");
        
        // Example 3: Get first element or default value
        var emptyNumbers = new List<int>();
        int firstOrDefault = emptyNumbers.GetOrDefault(42);
        Console.WriteLine($"First or default for empty list: {firstOrDefault}");
        // Output: First or default for empty list: 42
        
        // Example 4: Shuffle a collection randomly
        var orderedNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var shuffled = orderedNumbers.Shuffle();
        Console.WriteLine($"Original: {string.Join(", ", orderedNumbers)}");
        Console.WriteLine($"Shuffled: {string.Join(", ", shuffled)}");
        
        // Example 5: Get a random element from a collection
        var colors = new List<string> { "Red", "Green", "Blue", "Yellow", "Purple" };
        string randomColor = colors.GetRandom();
        Console.WriteLine($"Random color: {randomColor}");
        
        // Example 6: Find duplicate elements in a collection
        var dataWithDuplicates = new List<int> { 1, 2, 3, 2, 4, 5, 3, 6, 1 };
        var duplicates = dataWithDuplicates.FindDuplicates();
        Console.WriteLine($"Duplicates found: {string.Join(", ", duplicates)}");
        // Output: Duplicates found: 1, 2, 3
        
        // Example 7: DistinctBy - get distinct elements based on a key selector
        var people = new List<Person>
        {
            new Person("Alice", 25),
            new Person("Bob", 30),
            new Person("Alice", 28), // Same name, different age
            new Person("Charlie", 22)
        };
        
        var distinctByName = people.DistinctBy(p => p.Name);
        Console.WriteLine("People with distinct names:");
        foreach (var person in distinctByName)
        {
            Console.WriteLine($"  {person.Name} - {person.Age}");
        }
        
        // Example 8: ContainsAny - check if any element satisfies a condition
        var temperatures = new List<double> { 15.5, 18.2, 22.1, 19.8 };
        bool hasHighTemp = temperatures.ContainsAny(t => t > 20);
        Console.WriteLine($"Contains temperature > 20: {hasHighTemp}");
        
        // Example 9: SumBy - sum a specific property across all elements
        var products = new List<Product>
        {
            new Product("Laptop", 999.99m),
            new Product("Mouse", 29.99m),
            new Product("Keyboard", 79.99m)
        };
        decimal totalPrice = products.SumBy(p => p.Price);
        Console.WriteLine($"Total price: {totalPrice:C}");
        
        // Example 10: AverageOrDefault - compute average with fallback
        var measurements = new List<double> { 10.5, 12.3, 11.7, 13.1 };
        double avg = measurements.AverageOrDefault(x => x, -1);
        Console.WriteLine($"Average: {avg:F2}");
        
        // Example 11: ChunkBy - split collection by predicate
        var logEntries = new List<string> { "INFO", "DEBUG", "INFO", "WARN", "ERROR", "INFO" };
        var logChunks = logEntries.ChunkBy(entry => entry == "INFO");
        Console.WriteLine("Log chunks separated by INFO:");
        foreach (var chunk in logChunks)
        {
            Console.WriteLine($"  {string.Join(", ", chunk)}");
        }
        
        // Example 12: Interleave - merge multiple sequences alternately
        var sequence1 = new List<int> { 1, 2, 3 };
        var sequence2 = new List<int> { 4, 5, 6, 7 };
        var sequence3 = new List<int> { 8, 9 };
        
        var interleaved = CollectionExtensions.Interleave(sequence1, sequence2, sequence3);
        Console.WriteLine($"Interleaved: {string.Join(", ", interleaved)}");
        // Output: Interleaved: 1, 4, 8, 2, 5, 9, 3, 6, 7
    }
}

public class Person
{
    public string Name { get; }
    public int Age { get; }
    
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}

public class Product
{
    public string Name { get; }
    public decimal Price { get; }
    
    public Product(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}
```

## ChartModelsAndValidationTests

`ChartModelsAndValidationTests` is a comprehensive unit test suite that validates the behavior of core chart models and validation logic in the SkiaSharpChartEngine library. This test class ensures the correctness of fundamental components including `DataPoint`, `ChartSeries`, `Chart`, `ChartValidator`, `ColorHelper`, and their associated extension methods.

The tests cover:

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;
using SkiaSharpChartEngine.Extensions;

public class ChartModelsAndValidationTestsExample
{
    public static void Main()
    {
        // Example 1: Test DataPoint validation - setting X to NaN throws ArgumentException
        var point = new DataPoint(1.0, 2.0);
        try
        {
            point.X = double.NaN;
            Console.WriteLine("ERROR: Should have thrown ArgumentException");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"✓ DataPoint validation: {ex.Message}");
        }

        // Example 2: Test DataPoint cloning produces independent copy
        var original = new DataPoint(3.0, 7.5, "Q3", "#FF0000");
        original.Metadata = new Dictionary<string, object> { ["key"] = "value" };
        
        var clone = original.Clone();
        clone.X = 99.0;
        
        Console.WriteLine($"✓ Clone independent: Original.X={original.X}, Clone.X={clone.X}");

        // Example 3: Test ChartSeries data point count and Y-axis range
        var series = new ChartSeries("Revenue");
        series.AddDataPoint(1.0, 100.0);
        series.AddDataPoint(2.0, 150.0);
        series.AddDataPoint(3.0, 120.0);
        
        Console.WriteLine($"✓ Series data points: {series.GetDataPointCount()}");
        var (minY, maxY) = series.GetYAxisRange();
        Console.WriteLine($"✓ Y-axis range: [{minY}, {maxY}]");

        // Example 4: Test Chart validation
        var chart = new Chart("sales-chart");
        var validationResult = ChartValidator.ValidateChart(chart);
        Console.WriteLine($"✓ Chart validation valid: {validationResult.IsValid}");
        
        // Add a series to make it valid
        chart.AddSeries(series);
        validationResult = ChartValidator.ValidateChart(chart);
        Console.WriteLine($"✓ Chart with series valid: {validationResult.IsValid}");

        // Example 5: Test ColorHelper utilities
        string rgbRed = ColorHelper.HexToRgb("#FF0000");
        Console.WriteLine($"✓ Hex to RGB: {rgbRed}");
        
        string hexBlue = ColorHelper.RgbToHex(0, 0, 255);
        Console.WriteLine($"✓ RGB to Hex: {hexBlue}");
        
        bool isValidHex = ColorHelper.IsValidHexColor("#1F77B4");
        Console.WriteLine($"✓ Valid hex color: {isValidHex}");
        
        // Example 6: Test DataPoint extensions - offset and scale
        var dataPoint = new DataPoint(1.0, 2.0);
        var offsetPoint = dataPoint.Offset(3.0, 1.0);
        Console.WriteLine($"✓ Offset point: [{offsetPoint.X}, {offsetPoint.Y}]");
        
        var scaledPoint = dataPoint.Scale(2.0, 0.5);
        Console.WriteLine($"✓ Scaled point: [{scaledPoint.X:F2}, {scaledPoint.Y:F2}]");
        
        // Example 7: Test Euclidean distance calculation
        var pointA = new DataPoint(0.0, 0.0);
        var pointB = new DataPoint(3.0, 4.0);
        double distance = pointA.GetDistance(pointB);
        Console.WriteLine($"✓ Distance between (0,0) and (3,4): {distance:F4}");
    }
}
```

## ChartDataServiceTests

`ChartDataServiceTests` provides a comprehensive suite of unit tests for the `ChartDataService` class, which validates chart configurations, transforms and normalizes chart data, filters data points, and calculates axis ranges. The tests cover validation scenarios (null charts, empty series, invalid line widths), data transformation operations, normalization to [0, 1] intervals, and axis range calculations for both linear and logarithmic scales.

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class ChartDataServiceExample
{
    public static void Main()
    {
        // Initialize ChartDataService with logger
        var logger = new NullLogger<ChartDataService>();
        var chartDataService = new ChartDataService(logger);

        // Example 1: Validate a chart before processing
        var chart = new Chart("sales-performance");
        var revenueSeries = new ChartSeries("Revenue")
        {
            LineWidth = 2.0f,
            Color = "#2E86C1"
        };
        revenueSeries.AddDataPoint(1.0, 1000.0);
        revenueSeries.AddDataPoint(2.0, 1500.0);
        revenueSeries.AddDataPoint(3.0, 1200.0);
        chart.AddSeries(revenueSeries);

        // Validate chart configuration
        try
        {
            chartDataService.ValidateChart(chart);
            Console.WriteLine("✓ Chart validation passed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Chart validation failed: {ex.Message}");
        }

        // Example 2: Calculate axis range for linear scale
        var yValues = new double[] { 1000.0, 1500.0, 1200.0, 800.0 };
        var (minValue, maxValue) = chartDataService.CalculateAxisRange(yValues, AxisScaleType.Linear);
        Console.WriteLine($"Linear axis range: [{minValue}, {maxValue}]");
        // Output: Linear axis range: [800, 1500]

        // Example 3: Calculate axis range for logarithmic scale
        var logValues = new double[] { 0.1, 1.0, 10.0, 100.0 };
        var (logMin, logMax) = chartDataService.CalculateAxisRange(logValues, AxisScaleType.Logarithmic);
        Console.WriteLine($"Logarithmic axis range: [{logMin}, {logMax}]");
        // Output: Logarithmic axis range: [1, 100]

        // Example 4: Filter data points with positive Y values
        var allPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 1000.0),
            new DataPoint(2.0, -200.0),
            new DataPoint(3.0, 1500.0),
            new DataPoint(4.0, -500.0),
            new DataPoint(5.0, 1200.0)
        };
        var filteredPoints = chartDataService.FilterDataPoints(allPoints, p => p.Y > 0);
        Console.WriteLine($"Filtered points count: {filteredPoints.Count}");
        // Output: Filtered points count: 3

        // Example 5: Transform chart data (double all Y values)
        var transformedChart = chartDataService.TransformChartData(chart, 
            p => new DataPoint(p.X, p.Y * 2));
        Console.WriteLine($"Original first point Y: {chart.Series[0].DataPoints[0].Y}");
        Console.WriteLine($"Transformed first point Y: {transformedChart.Series[0].DataPoints[0].Y}");
        // Output: Original first point Y: 1000
        //         Transformed first point Y: 2000

        // Example 6: Normalize data points to [0, 1] range
        var normalizationPoints = new List<DataPoint>
        {
            new DataPoint(0.0, 100.0),
            new DataPoint(50.0, 500.0),
            new DataPoint(100.0, 900.0)
        };
        chartDataService.NormalizeDataPoints(normalizationPoints);
        Console.WriteLine($"Normalized first point: [{normalizationPoints[0].X:F4}, {normalizationPoints[0].Y:F4}]");
        Console.WriteLine($"Normalized last point: [{normalizationPoints[2].X:F4}, {normalizationPoints[2].Y:F4}]");
        // Output: Normalized first point: [0.0000, 0.0000]
        //         Normalized last point: [1.0000, 1.0000]
    }
}
```

## RenderCacheServiceTests

`RenderCacheServiceTests` provides a comprehensive suite of unit tests for the `RenderCacheService` class, which implements a thread-safe, LRU (Least Recently Used) cache for storing rendered chart images. The tests validate null safety, cache operations (get/set/remove/clear), cache eviction policies when maximum size is reached, and statistics tracking. The test suite covers edge cases including null keys, empty keys, non-existent keys, and various cache management scenarios.

```csharp
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Services;

public class RenderCacheServiceTestsExample
{
  public static void Main()
  {
    // Initialize logger and cache service with small size for demonstration
    var logger = new NullLogger<RenderCacheService>();
    var cacheService = new RenderCacheService(logger, maxCacheSize: 5);

    // Example 1: Store and retrieve cached render results
    byte[] chartImageData = new byte[1024]; // Simulated chart image data
    cacheService.Set("chart-2024-q1", chartImageData);
    
    var cachedData = cacheService.Get("chart-2024-q1");
    Console.WriteLine($"Cache contains 'chart-2024-q1': {cachedData != null}");
    // Output: Cache contains 'chart-2024-q1': True

    // Example 2: Handle null keys gracefully
    try
    {
      var nullResult = cacheService.Get(null!);
      Console.WriteLine("✓ Get with null key handled gracefully");
    }
    catch
    {
      Console.WriteLine("✗ Get with null key threw exception");
    }

    // Example 3: Handle empty keys gracefully
    try
    {
      var emptyResult = cacheService.Get(" ");
      Console.WriteLine("✓ Get with empty key handled gracefully");
    }
    catch
    {
      Console.WriteLine("✗ Get with empty key threw exception");
    }

    // Example 4: Check cache contains key
    cacheService.Set("chart-2024-q2", new byte[512]);
    bool containsKey = cacheService.Contains("chart-2024-q2");
    Console.WriteLine($"Cache contains 'chart-2024-q2': {containsKey}");
    // Output: Cache contains 'chart-2024-q2': True

    // Example 5: Remove entry from cache
    cacheService.Remove("chart-2024-q2");
    bool removed = !cacheService.Contains("chart-2024-q2");
    Console.WriteLine($"Entry removed successfully: {removed}");
    // Output: Entry removed successfully: True

    // Example 6: Clear entire cache
    cacheService.Set("chart-1", new byte[256]);
    cacheService.Set("chart-2");
    cacheService.Set("chart-3", new byte[256]);
    
    int sizeBeforeClear = cacheService.GetCacheSize();
    cacheService.Clear();
    int sizeAfterClear = cacheService.GetCacheSize();
    
    Console.WriteLine($"Cache size before clear: {sizeBeforeClear}");
    Console.WriteLine($"Cache size after clear: {sizeAfterClear}");
    // Output: Cache size before clear: 3
    // Output: Cache size after clear: 0

    // Example 7: Cache eviction when maximum size is reached
    for (int i = 1; i <= 6; i++)
    {
      cacheService.Set($"chart-{i}", new byte[256]);
    }
    
    // The cache should have evicted the least recently used entry (chart-1)
    bool hasChart1 = cacheService.Contains("chart-1");
    bool hasChart6 = cacheService.Contains("chart-6");
    
    Console.WriteLine($"Cache contains chart-1 (should be false): {hasChart1}");
    Console.WriteLine($"Cache contains chart-6 (should be true): {hasChart6}");
    // Output: Cache contains chart-1 (should be false): False
    // Output: Cache contains chart-6 (should be true): True

    // Example 8: Get all cached keys
    cacheService.Set("chart-a", new byte[128]);
    cacheService.Set("chart-b", new byte[128]);
    cacheService.Set("chart-c", new byte[128]);
    
    var allKeys = cacheService.GetAllKeys();
    Console.WriteLine($"Total cached entries: {allKeys.Count()}");
    Console.WriteLine($"Keys: {string.Join(", ", allKeys)}");
    // Output: Total cached entries: 3
    // Output: Keys: chart-a, chart-b, chart-c
  }
}
```

## ExportServiceTests

`ExportServiceTests` provides a comprehensive suite of unit tests for the `ExportService` class, which handles chart export functionality including both synchronous and asynchronous export operations. The tests validate null argument validation, unsupported format handling, successful rendering delegation, error handling for infrastructure issues, cancellation support, and constructor dependency validation for the export service.

The test suite covers:
- **Async export methods**: Validating `ExportAsync` with null checks, unsupported formats, successful rendering delegation, failure scenarios, infrastructure errors, and cancellation support
- **Sync export methods**: Testing `Export` with null argument validation and successful rendering delegation
- **Format support**: Verifying `SupportsFormat` returns true for supported formats (PNG, SVG, JPEG, WEBP) and false for unsupported formats
- **Supported formats list**: Ensuring `GetSupportedFormats` returns all four supported formats in ascending order
- **Constructor validation**: Ensuring proper dependency injection validation for rendering service and logger

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class ExportServiceTestsExample
{
    public static void Main()
    {
        // Initialize ExportService with dependencies
        var logger = new NullLogger<ExportService>();
        var renderingService = new ChartRenderingService(
            logger,
            new ChartDataService(logger),
            new RenderCacheService()
        );
        var exportService = new ExportService(renderingService, logger);

        // Create a test chart with data
        var chart = new Chart("sales-chart");
        var series = new ChartSeries("Revenue");
        series.AddDataPoint(1.0, 1000.0);
        series.AddDataPoint(2.0, 1500.0);
        series.AddDataPoint(3.0, 1200.0);
        chart.AddSeries(series);

        // Example 1: Check if PNG format is supported
        bool supportsPng = exportService.SupportsFormat(ExportFormat.PNG);
        Console.WriteLine($"PNG format supported: {supportsPng}");
        // Output: PNG format supported: True

        // Example 2: Get all supported formats
        var supportedFormats = exportService.GetSupportedFormats();
        Console.WriteLine($"Supported formats: {string.Join(", ", supportedFormats)}");
        // Output: Supported formats: JPEG, PNG, SVG, WEBP

        // Example 3: Export chart to file (async)
        var exportOptions = new ExportOptions
        {
            Format = ExportFormat.PNG,
            DirectoryPath = Path.GetTempPath(),
            FileName = "chart-export.png"
        };
        
        var asyncResult = await exportService.ExportAsync(chart, exportOptions);
        Console.WriteLine($"Async export success: {asyncResult.Success}");
        Console.WriteLine($"Error message: {asyncResult.ErrorMessage ?? "None"}");
        
        // Example 4: Export chart to file (sync)
        var syncResult = exportService.Export(chart, exportOptions);
        Console.WriteLine($"Sync export success: {syncResult.Success}");
        
        // Example 5: Export to SVG format
        var svgOptions = new ExportOptions
        {
            Format = ExportFormat.SVG,
            DirectoryPath = Path.GetTempPath(),
            FileName = "chart-export.svg"
        };
        
        var svgResult = await exportService.ExportAsync(chart, svgOptions);
        Console.WriteLine($"SVG export success: {svgResult.Success}");
        
        // Example 6: Handle unsupported format
        try
        {
            var invalidOptions = new ExportOptions { Format = (ExportFormat)999 };
            var invalidResult = exportService.Export(chart, invalidOptions);
            Console.WriteLine("ERROR: Should have thrown UnsupportedExportFormatException");
        }
        catch (UnsupportedExportFormatException ex)
        {
            Console.WriteLine($"✓ Correctly threw UnsupportedExportFormatException: {ex.Message}");
        }
    }
}
```

## ChartInteractionServiceTests

`ChartInteractionServiceTests` provides a comprehensive suite of unit tests for the `ChartInteractionService` class, which handles user interactions with chart elements such as clicks, hovers, selections, and context menu gestures. The tests validate edge cases including null inputs, missed interactions, and proper event raising, ensuring the service correctly processes user interactions and maintains selection state.

## ChartRenderingServiceTests

`ChartRenderingServiceTests` provides a comprehensive suite of unit tests for the `ChartRenderingService` class, which handles synchronous and asynchronous chart rendering to byte arrays and files in various image formats (PNG, SVG, JPEG, WebP). The tests validate null argument validation, caching behavior, file system operations, cancellation support, and constructor dependency validation for the chart rendering service.

The test suite covers:
- **Async rendering methods**: Validating `RenderToByteArrayAsync`, `RenderToFileAsync`, and `RenderWithExportAsync` with null checks, caching behavior, and cancellation support
- **Sync rendering methods**: Testing `RenderToByteArray` and `RenderToFile` with null argument validation
- **Cache pre-warming**: Verifying `PrewarmCache` populates the render cache correctly
- **Constructor validation**: Ensuring proper dependency injection validation for logger, data service, and cache service

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class ChartRenderingServiceExample
{
    public static void Main()
    {
        // Initialize ChartRenderingService with dependencies
        var logger = new NullLogger<ChartRenderingService>();
        var dataService = new ChartDataService(logger);
        var cacheService = new RenderCacheService();
        var renderingService = new ChartRenderingService(logger, dataService, cacheService);

        // Create a simple line chart with data
        var chart = new Chart("sales-chart");
        var series = new ChartSeries("Revenue");
        series.AddDataPoint(1.0, 1000.0);
        series.AddDataPoint(2.0, 1500.0);
        series.AddDataPoint(3.0, 1200.0);
        chart.AddSeries(series);

        // Example 1: Render chart to byte array (async)
        var renderResult = await renderingService.RenderToByteArrayAsync(chart);
        Console.WriteLine($"Async render success: {renderResult.Success}");
        Console.WriteLine($"Render time: {renderResult.RenderTimeMilliseconds}ms");
        Console.WriteLine($"Image data size: {renderResult.ImageData?.Length ?? 0} bytes");

        // Example 2: Render chart to file (async)
        var filePath = Path.Combine(Path.GetTempPath(), "chart-example.png");
        var fileResult = await renderingService.RenderToFileAsync(chart, filePath);
        Console.WriteLine($"Async file render success: {fileResult.Success}");
        Console.WriteLine($"File exists: {File.Exists(filePath)}");

        // Example 3: Export with custom options (async)
        var exportOptions = new ExportOptions
        {
            Format = ExportFormat.SVG,
            DirectoryPath = Path.GetTempPath(),
            FileName = "chart-example.svg"
        };
        var exportResult = await renderingService.RenderWithExportAsync(chart, exportOptions);
        Console.WriteLine($"Export render success: {exportResult.Success}");

        // Example 4: Synchronous rendering
        var syncResult = renderingService.RenderToByteArray(chart);
        Console.WriteLine($"Sync render success: {syncResult.Success}");
        Console.WriteLine($"Sync image data size: {syncResult.ImageData?.Length ?? 0} bytes");

        // Example 5: Pre-warm the cache for faster subsequent renders
        renderingService.PrewarmCache(chart);
        Console.WriteLine("Cache pre-warmed");

        // Cleanup
        if (File.Exists(filePath)) File.Delete(filePath);
        if (File.Exists(exportOptions.FullPath)) File.Delete(exportOptions.FullPath);
    }
}
```

## ChartStreamingServiceTests

`ChartStreamingServiceTests` provides a comprehensive suite of unit tests for the `ChartStreamingService` class, which handles real-time data streaming for charts. The service maintains a buffer of streaming data points, applies windowing to limit data retention, and provides thread-safe snapshot generation for rendering. Tests cover registration, publishing, batch operations, window size enforcement, auto-series creation, and asynchronous flushing operations.

## WebhookHandler

`WebhookHandler` handles webhook subscriptions and deliveries, allowing external services to be notified of chart events. It implements the `IChartEventSubscriber` interface to receive chart events and deliver them via HTTP POST to registered webhook URLs.

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharpChartEngine.Integration;
using SkiaSharpChartEngine.Events;

public class WebhookHandlerExample
{
    public static async Task Main(string[] args)
    {
        // Initialize webhook handler with logger
        var logger = new NullLogger<WebhookHandler>();
        var webhookHandler = new WebhookHandler(logger);

        // Register a webhook for chart created events
        string subscriptionId = webhookHandler.RegisterWebhook(
            "chart.created",
            "https://example.com/webhooks/chart-created",
            new WebhookOptions { Headers = new Dictionary<string, string> { { "Authorization", "Bearer token123" } } }
        );

        Console.WriteLine($"Registered webhook with ID: {subscriptionId}");

        // Get all subscriptions
        List<WebhookSubscription> subscriptions = webhookHandler.GetSubscriptions();
        Console.WriteLine($"Total subscriptions: {subscriptions.Count}");

        // Simulate a chart created event
        var chartCreatedEvent = new ChartCreatedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            "chart-123"
        );

        await webhookHandler.OnChartCreatedAsync(chartCreatedEvent);
        Console.WriteLine("Chart created event processed and webhook delivered");

        // Check subscription details
        var subscription = subscriptions.Find(s => s.Id == subscriptionId);
        if (subscription != null)
        {
            Console.WriteLine($"Subscription status - Active: {subscription.IsActive}, Healthy: {subscription.IsHealthy}");
            Console.WriteLine($"Delivery count: {subscription.DeliveryCount}");
            Console.WriteLine($"Last delivery: {subscription.LastDeliveryAt}");
        }

        // Unregister the webhook
        bool unregistered = webhookHandler.UnregisterWebhook(subscriptionId);
        Console.WriteLine($"Webhook unregistered: {unregistered}");
    }
}
```

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using SkiaSharpChartEngine.Streaming;

public class ChartStreamingServiceExample
{
    public static void Main()
    {
        // Initialize streaming service with rendering service
        var renderingService = new ChartRenderingService();
        var streamingService = new ChartStreamingService(renderingService);

        // Create a chart for streaming
        var chart = new Chart("temperature-stream");
        chart.AddSeries(new ChartSeries("Temperature"));
        chart.AddSeries(new ChartSeries("Humidity"));

        // Register the chart with default options
        streamingService.Register(chart);

        // Publish individual data points
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = 1, Y = 23.5 });
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = 2, Y = 24.1 });
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Humidity", X = 1, Y = 45.0 });

        // Publish batch of points
        var humidityPoints = Enumerable.Range(1, 10).Select(i => 
            new StreamDataPoint { SeriesName = "Humidity", X = i, Y = 35.0 + i * 2.5 });
        streamingService.PublishBatch(chart.Id, humidityPoints);

        // Get current snapshot for rendering
        var snapshot = streamingService.GetSnapshot(chart.Id);
        Console.WriteLine($"Chart {snapshot.Id} has {snapshot.Series.Count} series");
        Console.WriteLine($"Temperature points: {snapshot.GetSeriesByName("Temperature")?.GetDataPointCount()}");
        Console.WriteLine($"Humidity points: {snapshot.GetSeriesByName("Humidity")?.GetDataPointCount()}");

        // Configure window size to limit data retention
        var options = new StreamingChartOptions { WindowSize = 5 };
        streamingService.Register(chart, options);

        // Publish more points - oldest will be dropped when window exceeds size
        for (int i = 1; i <= 10; i++)
        {
            streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = i + 10, Y = 20.0 + i });
        }

        // Get updated snapshot
        var finalSnapshot = streamingService.GetSnapshot(chart.Id);
        Console.WriteLine($"After windowing, Temperature points: {finalSnapshot.GetSeriesByName("Temperature")?.GetDataPointCount()}");

        // Flush buffered points asynchronously
        var flushedCount = await streamingService.FlushAsync(chart.Id);
        Console.WriteLine($"Flushed {flushedCount} buffered points");

        // Unregister when chart is no longer needed
        streamingService.Unregister(chart.Id);
    }
}
```

## RenderResultCacheTests

`RenderResultCacheTests` provides a comprehensive suite of unit tests for the `RenderResultCache` class, which implements a thread-safe, time-based cache for storing chart rendering results. The cache supports configurable maximum size limits, time-to-live (TTL) expiration, LRU (Least Recently Used) eviction policy, and detailed statistics tracking. Tests cover null safety, cache operations (add/get/remove/clear), TTL expiration, size limits, thread safety, and statistics reporting.

```csharp
using System;
using SkiaSharpChartEngine.Caching;
using SkiaSharpChartEngine.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class RenderResultCacheExample
{
    public static void Main()
    {
        // Initialize cache with logger and maximum size (100 MB default)
        var logger = new NullLogger<RenderResultCache>();
        var cache = new RenderResultCache(logger, maxSizeBytes: 104_857_600);

        // Create a test render result
        var renderResult = RenderResult.CreateSuccess(
            "line-chart-001",
            new byte[1024], // Image data
            renderTimeMilliseconds: 42,
            ExportFormat.PNG
        );

        // Example 1: Cache a render result with default TTL (1 hour)
        cache.Cache("render-key-001", renderResult);
        Console.WriteLine("Cached render result successfully");

        // Example 2: Retrieve cached result
        var cachedResult = cache.Get("render-key-001");
        if (cachedResult != null)
        {
            Console.WriteLine($"Retrieved cached result: ChartId={cachedResult.ChartId}, Size={cachedResult.ImageData?.Length} bytes");
        }

        // Example 3: Cache with custom TTL (5 minutes)
        var customTtl = TimeSpan.FromMinutes(5);
        cache.Cache("render-key-002", renderResult, customTtl);
        Console.WriteLine("Cached with 5-minute TTL");

        // Example 4: Remove a cached entry
        bool removed = cache.Remove("render-key-001");
        Console.WriteLine($"Entry removed: {removed}");

        // Example 5: Clear all cached entries
        cache.Clear();
        Console.WriteLine("Cache cleared");

        // Example 6: Get cache statistics
        var stats = cache.GetStatistics();
        Console.WriteLine($"Cache stats - Entries: {stats.TotalEntries}, Size: {stats.TotalSize} bytes, " +
                        $"Hits: {stats.TotalHits}, MaxSize: {stats.MaxSize} bytes");
    }
}
```

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using SkiaSharpChartEngine.Streaming;

public class ChartStreamingServiceExample
{
    public static void Main()
    {
        // Initialize streaming service with rendering service
        var renderingService = new ChartRenderingService();
        var streamingService = new ChartStreamingService(renderingService);

        // Create a chart for streaming
        var chart = new Chart("temperature-stream");
        chart.AddSeries(new ChartSeries("Temperature"));
        chart.AddSeries(new ChartSeries("Humidity"));

        // Register the chart with default options
        streamingService.Register(chart);

        // Publish individual data points
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = 1, Y = 23.5 });
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = 2, Y = 24.1 });
        streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Humidity", X = 1, Y = 45.0 });

        // Publish batch of points
        var humidityPoints = Enumerable.Range(1, 10).Select(i =>
            new StreamDataPoint { SeriesName = "Humidity", X = i, Y = 35.0 + i * 2.5 });
        streamingService.PublishBatch(chart.Id, humidityPoints);

        // Get current snapshot for rendering
        var snapshot = streamingService.GetSnapshot(chart.Id);
        Console.WriteLine($"Chart {snapshot.Id} has {snapshot.Series.Count} series");
        Console.WriteLine($"Temperature points: {snapshot.GetSeriesByName("Temperature")?.GetDataPointCount()}");
        Console.WriteLine($"Humidity points: {snapshot.GetSeriesByName("Humidity")?.GetDataPointCount()}");

        // Configure window size to limit data retention
        var options = new StreamingChartOptions { WindowSize = 5 };
        streamingService.Register(chart, options);

        // Publish more points - oldest will be dropped when window exceeds size
        for (int i = 1; i <= 10; i++)
        {
            streamingService.Publish(chart.Id, new StreamDataPoint { SeriesName = "Temperature", X = i + 10, Y = 20.0 + i });
        }

        // Get updated snapshot
        var finalSnapshot = streamingService.GetSnapshot(chart.Id);
        Console.WriteLine($"After windowing, Temperature points: {finalSnapshot.GetSeriesByName("Temperature")?.GetDataPointCount()}");

        // Flush buffered points asynchronously
        var flushedCount = await streamingService.FlushAsync(chart.Id);
        Console.WriteLine($"Flushed {flushedCount} buffered points");

        // Unregister when chart is no longer needed
        streamingService.Unregister(chart.Id);
    }
}
```

```csharp
using System;
using System.Threading.Tasks;
using Xunit;
using SkiaSharpChartEngine;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class ChartInteractionServiceTestsExample
{
    public static void Main()
    {
        // Initialize the service with mock dependencies
        var interactivityService = new Mock<IInteractivityService>();
        var logger = new NullLogger<ChartInteractionService>();
        var interactionService = new ChartInteractionService(interactivityService.Object, logger);

        // Create a test chart with a single series
        var chart = new Chart("test-chart");
        var series = new ChartSeries("Sales Data");
        series.AddDataPoint(1.0, 100.0);
        series.AddDataPoint(2.0, 150.0);
        series.AddDataPoint(3.0, 200.0);
        chart.AddSeries(series);

        // Test 1: ProcessInteraction with null chart throws ArgumentNullException
        try
        {
            interactionService.ProcessInteraction(null!, ChartInteractionType.Click, 0, 0, 800, 600);
            Console.WriteLine("ERROR: Should have thrown ArgumentNullException");
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"✓ Correctly threw ArgumentNullException: {ex.ParamName}");
        }

        // Test 2: ProcessInteraction_ClickOnDataPoint_RaisesClickedEvent
        var hitResult = new TooltipHitResult
        {
            IsHit = true,
            DataPoint = series.DataPoints[0],
            Series = series,
            SeriesIndex = 0,
            Region = ChartRegion.PlotArea,
            TooltipText = "x=1, y=100"
        };
        interactivityService.Setup(s => s.HitTest(chart, It.IsAny<float>(), It.IsAny<float>(),
            It.IsAny<float>(), It.IsAny<float>(), null, null)).Returns(hitResult);

        ChartInteractionEventArgs? clickEventArgs = null;
        interactionService.Clicked += (_, e) => clickEventArgs = e;

        var clickResult = interactionService.ProcessInteraction(
            chart, ChartInteractionType.Click, 100, 200, 800, 600);

        Console.WriteLine($"✓ Click interaction processed: Type={clickResult.InteractionType}, " +
            $"HasDataPoint={clickResult.HitDataPoint != null}");

        // Test 3: ProcessInteraction_HoverMiss_ReturnsNoHit
        interactivityService.Setup(s => s.HitTest(chart, It.IsAny<float>(), It.IsAny<float>(),
            It.IsAny<float>(), It.IsAny<float>(), null, null)).Returns(TooltipHitResult.Miss);

        var hoverResult = interactionService.ProcessInteraction(
            chart, ChartInteractionType.Hover, 0, 0, 800, 600);

        Console.WriteLine($"✓ Hover miss handled correctly: SeriesIndex={hoverResult.SeriesIndex}");

        // Test 4: ToggleSelection_HitDataPoint_SelectsAndRaisesEvent
        interactivityService.Setup(s => s.HitTest(chart, It.IsAny<float>(), It.IsAny<float>(),
            It.IsAny<float>(), It.IsAny<float>(), null, null)).Returns(hitResult);

        ChartSelectionChangedEventArgs? selectionArgs = null;
        interactionService.SelectionChanged += (_, e) => selectionArgs = e;

        var selected = interactionService.ToggleSelection(chart, 100, 200, 800, 600);
        Console.WriteLine($"✓ Selection toggled: Success={selected}, " +
            $"TotalSelected={selectionArgs?.TotalSelected}");

        // Test 5: ToggleSelection_SamePointTwice_DeselectionRemovesPoint
        interactionService.ToggleSelection(chart, 100, 200, 800, 600); // Deselect
        var selection = interactionService.GetSelection(chart);
        var totalPoints = selection.Values.Sum(pts => pts.Count);
        Console.WriteLine($"✓ Double toggle deselects: TotalPoints={totalPoints}");

        // Test 6: ClearSelection_AfterSelect_EmptiesSelection
        interactionService.ClearSelection(chart);
        var clearedSelection = interactionService.GetSelection(chart);
        Console.WriteLine($"✓ Selection cleared: IsEmpty={clearedSelection.IsEmpty}");

        // Test 7: ProcessInteractionAsync_WithCancellation_ThrowsOperationCancelled
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        try
        {
            await interactionService.ProcessInteractionAsync(
                chart, ChartInteractionType.Hover, 0, 0, 800, 600, cts.Token);
            Console.WriteLine("ERROR: Should have thrown OperationCanceledException");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("✓ Async cancellation handled correctly");
        }
    }
}
```


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

## DataAggregatorTests

`DataAggregatorTests` provides a comprehensive suite of unit tests for the `DataAggregator` class, which handles data aggregation operations including bucket-based aggregation, interval-based grouping, and statistical calculations. The tests validate edge cases, parameter validation, and correct computation across different aggregation types (Average, Sum, Min, Max, Median) to ensure accurate data summarization for chart rendering scenarios.

The test suite covers:

- **Bucket-based aggregation**: Validating `AggregateByCount` with null/empty inputs, zero/negative bucket counts, various aggregation types, and edge cases where bucket count exceeds data point count
- **Interval-based grouping**: Testing `AggregateByInterval` with null inputs, label-based grouping, and null label handling (groups as "unknown")
- **Statistical calculations**: Verifying `CalculateStatistics` returns null for null/empty inputs and computes Sum, Average, Min, Max, Median, Range, and Standard Deviation correctly
- **Error handling**: Ensuring proper exception throwing for invalid parameters and fallback behavior for unsupported aggregation types

```csharp
using System;
using System.Collections.Generic;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Utilities;

public class DataAggregatorTestsExample
{
    public static void Main()
    {
        // Initialize DataAggregator with logger
        var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<DataAggregator>();
        var aggregator = new DataAggregator(logger);

        // Example 1: Aggregate data points by count into buckets using average aggregation
        var dataPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 10.0),
            new DataPoint(2.0, 20.0),
            new DataPoint(3.0, 30.0),
            new DataPoint(4.0, 40.0)
        };

        var aggregated = aggregator.AggregateByCount(dataPoints, 2, AggregationType.Average);
        Console.WriteLine($"Aggregated {aggregated.Count} buckets");
        Console.WriteLine($"Bucket 1 average: {aggregated[0].Value:F2}");  // 15.00
        Console.WriteLine($"Bucket 2 average: {aggregated[1].Value:F2}");  // 35.00

        // Example 2: Group data points by label (interval-based aggregation)
        var labeledPoints = new List<DataPoint>
        {
            new DataPoint(1.0, 100.0) { Label = "Q1" },
            new DataPoint(2.0, 150.0) { Label = "Q1" },
            new DataPoint(3.0, 200.0) { Label = "Q2" },
            new DataPoint(4.0, 250.0) { Label = "Q2" }
        };

        var grouped = aggregator.AggregateByInterval(labeledPoints, AggregationType.Average);
        Console.WriteLine($"Groups created: {grouped.Count}");      // 2
        Console.WriteLine($"Q1 data points: {grouped["Q1"].Count}"); // 2
        Console.WriteLine($"Q2 data points: {grouped["Q2"].Count}"); // 2

        // Example 3: Calculate comprehensive statistics
        var stats = aggregator.CalculateStatistics(dataPoints);
        Console.WriteLine($"Statistics - Count: {stats?.Count}, Sum: {stats?.Sum:F2}");
        Console.WriteLine($"Min: {stats?.Min:F2}, Max: {stats?.Max:F2}");
        Console.WriteLine($"Average: {stats?.Average:F2}, Median: {stats?.Median:F2}");
        Console.WriteLine($"Range: {stats?.Range:F2}, StdDev: {stats?.StandardDeviation:F4}");
        Console.WriteLine($"Calculated at: {stats?.CalculatedAt:O}");
    }
}
```

## ChartRenderingIntegrationTests

`ChartRenderingIntegrationTests` validates the end-to-end rendering pipeline of the SkiaSharp chart engine, ensuring that charts are generated and exported correctly across various formats and configurations. It covers functional testing for rendering, exporting, caching, and parallel processing capabilities, providing confidence in the system's reliability under different scenarios.

```csharp
using System;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Tests; 

public class ChartRenderingIntegrationTestsExample
{
    public static async Task Main()
    {
        // Setup 
        using var testEngine = new ChartRenderingIntegrationTests();

        // 1. Basic rendering tests
        testEngine.CanRenderSimpleLineChartToFile();
        
        var bytes = testEngine.CanRenderChartToByteArray();
        Console.WriteLine($"Rendered chart to byte array, size: {bytes.Length}");

        // 2. Async rendering
        await testEngine.CanRenderChartAsyncToFile();

        // 3. Export tests
        testEngine.CanExportChartAsPng();
        await testEngine.CanExportChartAsSvg();
        await testEngine.CanExportChartAsJpeg();
        await testEngine.CanExportChartAsWebP();

        // 4. Advanced rendering features
        testEngine.CanRenderMultiSeriesChart();
        testEngine.CanToggleSeriesVisibility();
        testEngine.CanRenderChartWithSingleDataPoint();

        // 5. Caching and configuration
        testEngine.CachingImprovesPerfomanceOnSecondRender();
        testEngine.DifferentChartsProduceDifferentCacheKeys();
        testEngine.CanApplyCustomConfiguration();
        testEngine.CanDisableGridAndLegend();

        // 6. Error handling
        testEngine.RenderingFailsWithMissingData();
        testEngine.ExportFailsWithInvalidPath();

        // 7. Parallel processing
        await testEngine.CanRenderMultipleChartsInParallel();
        await testEngine.CanExportMultipleChartsInParallel();
    }
}
```
```
