// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Manages a queue of chart render operations with priority-based execution.
/// Prevents resource exhaustion by limiting concurrent renders.
/// </summary>
public class RenderQueueManager
{
    private readonly ILogger<RenderQueueManager> _logger;
    private readonly SortedSet<RenderJob> _queue;
    private readonly object _lockObject = new object();
    private readonly int _maxConcurrentRenders;
    private int _activeRenders;

    public RenderQueueManager(ILogger<RenderQueueManager> logger, int maxConcurrentRenders = 4)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _maxConcurrentRenders = maxConcurrentRenders;
        _activeRenders = 0;
        _queue = new SortedSet<RenderJob>();
    }

    // Enqueue render job
    public string Enqueue(Chart chart, int priority = 0)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            lock (_lockObject)
            {
                var job = new RenderJob
                {
                    Id = Guid.NewGuid().ToString(),
                    Chart = chart,
                    Priority = priority,
                    EnqueuedAt = DateTime.UtcNow,
                    Status = RenderJobStatus.Queued
                };

                _queue.Add(job);
                _logger.LogInformation("Render job enqueued: {JobId}, Priority: {Priority}", job.Id, priority);
                return job.Id;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueueing render job");
            throw;
        }
    }

    // Dequeue next job for processing
    public RenderJob Dequeue()
    {
        lock (_lockObject)
        {
            if (_activeRenders >= _maxConcurrentRenders)
            {
                _logger.LogDebug("Max concurrent renders reached ({Max})", _maxConcurrentRenders);
                return null;
            }

            if (_queue.Count == 0)
                return null;

            var job = _queue.First();
            _queue.Remove(job);
            job.Status = RenderJobStatus.Running;
            job.StartedAt = DateTime.UtcNow;
            _activeRenders++;

            _logger.LogInformation("Render job dequeued: {JobId}", job.Id);
            return job;
        }
    }

    // Mark job as complete
    public void CompleteJob(string jobId)
    {
        lock (_lockObject)
        {
            _activeRenders = Math.Max(0, _activeRenders - 1);
            _logger.LogInformation("Render job completed: {JobId}, Active renders: {Active}", jobId, _activeRenders);
        }
    }

    // Cancel job
    public bool CancelJob(string jobId)
    {
        lock (_lockObject)
        {
            var job = _queue.FirstOrDefault(j => j.Id == jobId);
            if (job != null)
            {
                _queue.Remove(job);
                _logger.LogInformation("Render job cancelled: {JobId}", jobId);
                return true;
            }

            return false;
        }
    }

    // Get queue status
    public QueueStatus GetStatus()
    {
        lock (_lockObject)
        {
            return new QueueStatus
            {
                QueuedCount = _queue.Count,
                ActiveRenders = _activeRenders,
                MaxConcurrentRenders = _maxConcurrentRenders,
                PendingCount = _queue.Count,
                StatusAt = DateTime.UtcNow
            };
        }
    }

    // Get job status
    public RenderJobStatus GetJobStatus(string jobId)
    {
        lock (_lockObject)
        {
            var job = _queue.FirstOrDefault(j => j.Id == jobId);
            return job?.Status ?? RenderJobStatus.NotFound;
        }
    }

    // Wait for queue to be empty
    public async Task WaitForQueueEmptyAsync(TimeSpan timeout = default)
    {
        var deadline = DateTime.UtcNow + (timeout == default ? TimeSpan.FromSeconds(30) : timeout);

        while (DateTime.UtcNow < deadline)
        {
            lock (_lockObject)
            {
                if (_queue.Count == 0 && _activeRenders == 0)
                {
                    _logger.LogInformation("Queue is empty");
                    return;
                }
            }

            await Task.Delay(100);
        }

        _logger.LogWarning("Timeout waiting for queue to empty");
    }

    public int GetQueueSize() => _queue.Count;
}

public class RenderJob : IComparable<RenderJob>
{
    public string Id { get; set; }
    public Chart Chart { get; set; }
    public int Priority { get; set; }
    public RenderJobStatus Status { get; set; }
    public DateTime EnqueuedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Higher priority jobs come first, then by enqueue time
    public int CompareTo(RenderJob other)
    {
        if (other == null) return 1;

        var priorityComparison = other.Priority.CompareTo(Priority);
        if (priorityComparison != 0) return priorityComparison;

        return EnqueuedAt.CompareTo(other.EnqueuedAt);
    }
}

public enum RenderJobStatus
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled,
    NotFound
}

public class QueueStatus
{
    public int QueuedCount { get; set; }
    public int ActiveRenders { get; set; }
    public int MaxConcurrentRenders { get; set; }
    public int PendingCount { get; set; }
    public DateTime StatusAt { get; set; }
}
