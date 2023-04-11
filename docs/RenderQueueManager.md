# RenderQueueManager

The `RenderQueueManager` provides a centralized mechanism for managing the asynchronous lifecycle of rendering tasks within the `skiasharp-chart-engine`. It orchestrates the ingestion, prioritization, execution, and tracking of `RenderJob` instances, ensuring that chart rendering is performed efficiently and safely while maintaining concurrent execution limits.

## API

### Constructors
- **`RenderQueueManager()`**: Initializes a new instance of the `RenderQueueManager` class with default configuration.

### Methods
- **`string Enqueue(RenderJob job)`**: Adds a `RenderJob` to the queue. Returns the unique string identifier for the enqueued job.
- **`RenderJob Dequeue()`**: Retrieves and removes the next `RenderJob` from the queue based on priority. Returns the `RenderJob` or null if the queue is empty.
- **`void CompleteJob(RenderJob job)`**: Marks a `RenderJob` as completed, updating its internal status and timestamps.
- **`bool CancelJob(string jobId)`**: Attempts to cancel a pending `RenderJob` by its identifier. Returns `true` if the job was found and cancelled; otherwise, `false`.
- **`QueueStatus GetStatus()`**: Returns the current overall `QueueStatus` of the manager.
- **`RenderJobStatus GetJobStatus(string jobId)`**: Retrieves the `RenderJobStatus` for a specific job identifier.
- **`async Task WaitForQueueEmptyAsync()`**: Asynchronously waits until the queue is empty and all active render operations have completed.
- **`int GetQueueSize()`**: Returns the total number of jobs currently waiting in the queue.
- **`int CompareTo(RenderQueueManager other)`**: Compares the current manager instance with another, typically based on priority for scheduling purposes. Returns an integer indicating relative order.

### Properties
- **`string Id`**: Gets the unique identifier for the manager instance.
- **`Chart Chart`**: Gets the `Chart` instance associated with this manager.
- **`int Priority`**: Gets or sets the priority level of the manager, affecting scheduling relative to other instances.
- **`RenderJobStatus Status`**: Gets the current status of the manager.
- **`DateTime EnqueuedAt`**: Gets the timestamp when the manager was initialized or last enqueued.
- **`DateTime? StartedAt`**: Gets the timestamp when rendering first started, if applicable.
- **`DateTime? CompletedAt`**: Gets the timestamp when all tasks were completed, if applicable.
- **`int QueuedCount`**: Gets the number of jobs currently in the queue.
- **`int ActiveRenders`**: Gets the number of jobs currently being processed.
- **`int MaxConcurrentRenders`**: Gets or sets the maximum number of render jobs that can be processed simultaneously.

## Usage

### Enqueueing and Completing a Job
```csharp
var manager = new RenderQueueManager();
var job = new RenderJob(myChart);

// Enqueue the job
string jobId = manager.Enqueue(job);

// Process the job
var currentJob = manager.Dequeue();
if (currentJob != null)
{
    // ... perform rendering logic ...
    manager.CompleteJob(currentJob);
}
```

### Waiting for Queue Completion
```csharp
var manager = new RenderQueueManager();
// ... assume jobs are added ...

// Wait for all jobs to finish
await manager.WaitForQueueEmptyAsync();

if (manager.GetQueueSize() == 0)
{
    Console.WriteLine("All rendering tasks completed.");
}
```

## Notes

- **Thread Safety**: The `RenderQueueManager` is designed to be thread-safe. Multiple threads can safely call `Enqueue`, `Dequeue`, or `CancelJob` simultaneously without external synchronization.
- **Exceptions**: `Enqueue` may throw an `ArgumentNullException` if the provided `RenderJob` is null. `CancelJob` may behave unexpectedly if called with an invalid job ID, though it gracefully returns `false`.
- **Concurrent Renders**: The `MaxConcurrentRenders` property controls throughput. Setting this too high may lead to resource contention, while setting it too low may increase latency for UI updates.
- **Async Operations**: `WaitForQueueEmptyAsync` should be awaited to ensure full completion of all pending work before disposing of resources or finalizing chart outputs.
