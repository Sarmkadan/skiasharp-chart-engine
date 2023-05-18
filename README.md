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
