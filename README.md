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
