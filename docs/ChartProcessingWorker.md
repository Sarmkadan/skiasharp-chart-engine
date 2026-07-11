# ChartProcessingWorker

A worker class that manages the asynchronous processing of chart generation jobs. It handles job queuing, execution, completion callbacks, and statistics tracking for chart rendering tasks in the SkiaSharp chart engine.

## API

### `ChartProcessingWorker`
The primary class for managing chart processing jobs. Instances are created to handle individual or batched chart generation tasks with configurable processing delegates, completion, and error handlers.

### `string EnqueueJob`
Enqueues a new chart processing job and returns a unique job identifier.
- **Returns**: A non-empty string representing the job ID.
- **Throws**: `InvalidOperationException` if the worker is already stopped or disposed.

### `WorkerStatistics GetStatistics()`
Retrieves current statistics about the worker's job processing.
- **Returns**: A `WorkerStatistics` object containing counts of completed, failed, and pending jobs.

### `async Task StopAsync()`
Asynchronously stops the worker, preventing new jobs from being enqueued and allowing in-progress jobs to complete.
- **Throws**: `ObjectDisposedException` if the worker has already been disposed.

### `public void Dispose()`
Releases all resources used by the worker and stops any ongoing processing.
- Implements `IDisposable` for deterministic cleanup.

### `string? JobId`
Gets the unique identifier of the current job being processed, if any.
- **Value**: `null` if no job is currently running.

### `string? ChartTitle`
Gets or sets the title of the chart being processed by the current job.
- **Value**: `null` if not set.

### `string? ChartId`
Gets or sets the identifier of the chart being processed by the current job.
- **Value**: `null` if not set.

### `public JobStatus Status`
Gets the current status of the job (e.g., Pending, Running, Completed, Failed).
- **Value**: One of the `JobStatus` enum values.

### `public DateTime CreatedAt`
Gets the timestamp when the job was created.

### `public DateTime? StartedAt`
Gets the timestamp when the job started processing, if applicable.
- **Value**: `null` if the job has not started.

### `public DateTime? CompletedAt`
Gets the timestamp when the job completed, if applicable.
- **Value**: `null` if the job is not completed.

### `public string? ErrorMessage`
Gets the error message associated with a failed job, if applicable.
- **Value**: `null` if no error occurred.

### `public Dictionary<string, object> Parameters`
Gets the collection of parameters used for chart rendering in the current job.
- **Value**: An empty dictionary if no parameters are set.

### `public object? Result`
Gets the result of the chart processing job, if successful.
- **Value**: `null` if the job is not completed or failed.

### `public Func<ChartProcessingJob, CancellationToken, Task>? ProcessingDelegate`
Gets or sets the asynchronous delegate responsible for performing the chart processing.
- **Value**: `null` if not set.

### `public Action<ChartProcessingJob>? OnComplete`
Gets or sets an optional callback invoked when the job completes successfully.
- **Parameters**: The completed `ChartProcessingJob`.

### `public Action<ChartProcessingJob, Exception>? OnError`
Gets or sets an optional callback invoked when the job fails.
- **Parameters**: The failed `ChartProcessingJob` and the thrown `Exception`.

### `public TimeSpan? GetDuration()`
Calculates the total duration of the job from creation to completion.
- **Returns**: The elapsed time as a `TimeSpan`, or `null` if the job is not completed.
- **Throws**: `InvalidOperationException` if the job is not completed.

### `public int PendingJobs`
Gets the number of jobs currently waiting to be processed.
- **Value**: Zero if no jobs are pending.

## Usage

### Example 1: Basic chart processing with callbacks
