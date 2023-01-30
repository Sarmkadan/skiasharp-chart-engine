// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Workers;

/// <summary>
/// Background worker for processing chart rendering jobs
/// Handles async rendering without blocking main threads
/// </summary>
public class ChartProcessingWorker : IDisposable
{
    private readonly ILogger<ChartProcessingWorker> _logger;
    private readonly BlockingCollection<ChartProcessingJob> _jobQueue;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _workerTask;
    private readonly int _concurrencyLevel;

    public ChartProcessingWorker(ILogger<ChartProcessingWorker> logger, int concurrencyLevel = 4)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _concurrencyLevel = Math.Max(1, concurrencyLevel);
        _jobQueue = new BlockingCollection<ChartProcessingJob>();
        _cancellationTokenSource = new CancellationTokenSource();
        _workerTask = StartWorkerAsync(_cancellationTokenSource.Token);

        _logger.LogInformation("Chart processing worker started with concurrency level {ConcurrencyLevel}",
            _concurrencyLevel);
    }

    /// <summary>
    /// Enqueues a chart for processing
    /// </summary>
    public string EnqueueJob(ChartProcessingJob job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        job.JobId ??= Guid.NewGuid().ToString();
        _jobQueue.Add(job);

        _logger.LogDebug("Chart processing job enqueued: {JobId}", job.JobId);
        return job.JobId;
    }

    /// <summary>
    /// Starts processing jobs from the queue
    /// </summary>
    private async Task StartWorkerAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_jobQueue.TryTake(out var job, 1000, cancellationToken))
                {
                    await ProcessJobAsync(job, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Chart processing worker cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chart processing worker");
        }
    }

    /// <summary>
    /// Processes a single chart job
    /// </summary>
    private async Task ProcessJobAsync(ChartProcessingJob job, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Processing chart job {JobId}: {Title}", job.JobId, job.ChartTitle);

            job.Status = JobStatus.Processing;
            job.StartedAt = startTime;

            if (job.ProcessingDelegate != null)
            {
                await job.ProcessingDelegate(job, cancellationToken);
            }

            job.Status = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation("Chart job {JobId} completed in {Duration}ms",
                job.JobId, (DateTime.UtcNow - startTime).TotalMilliseconds);

            job.OnComplete?.Invoke(job);
        }
        catch (OperationCanceledException)
        {
            job.Status = JobStatus.Cancelled;
            _logger.LogWarning("Chart job {JobId} was cancelled", job.JobId);
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogError(ex, "Error processing chart job {JobId}", job.JobId);
            job.OnError?.Invoke(job, ex);
        }
    }

    /// <summary>
    /// Gets current queue statistics
    /// </summary>
    public WorkerStatistics GetStatistics()
    {
        return new WorkerStatistics
        {
            PendingJobs = _jobQueue.Count,
            ConcurrencyLevel = _concurrencyLevel,
            IsRunning = !_cancellationTokenSource.Token.IsCancellationRequested
        };
    }

    /// <summary>
    /// Gracefully shuts down the worker
    /// </summary>
    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping chart processing worker...");
        _jobQueue.CompleteAdding();

        try
        {
            await _workerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _jobQueue?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}

/// <summary>
/// Represents a chart processing job
/// </summary>
public class ChartProcessingJob
{
    public string? JobId { get; set; }
    public string? ChartTitle { get; set; }
    public string? ChartId { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public object? Result { get; set; }

    public Func<ChartProcessingJob, CancellationToken, Task>? ProcessingDelegate { get; set; }
    public Action<ChartProcessingJob>? OnComplete { get; set; }
    public Action<ChartProcessingJob, Exception>? OnError { get; set; }

    /// <summary>
    /// Gets job execution duration
    /// </summary>
    public TimeSpan? GetDuration()
    {
        if (StartedAt.HasValue && CompletedAt.HasValue)
            return CompletedAt.Value - StartedAt.Value;

        if (StartedAt.HasValue)
            return DateTime.UtcNow - StartedAt.Value;

        return null;
    }
}

/// <summary>
/// Job processing status
/// </summary>
public enum JobStatus
{
    Queued,
    Processing,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Worker statistics
/// </summary>
public class WorkerStatistics
{
    public int PendingJobs { get; set; }
    public int ConcurrencyLevel { get; set; }
    public bool IsRunning { get; set; }
}
