// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Processes items in batches with configurable batch size and timeout.
/// Useful for bulk operations on charts and data points.
/// </summary>
public class BatchProcessor<T>
{
    private readonly ILogger<BatchProcessor<T>> _logger;
    private readonly int _batchSize;
    private readonly int _timeoutMs;
    private readonly Func<List<T>, Task> _batchProcessor;

    public BatchProcessor(int batchSize, int timeoutMs, Func<List<T>, Task> batchProcessor, ILogger<BatchProcessor<T>> logger)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be positive", nameof(batchSize));

        if (batchProcessor == null)
            throw new ArgumentNullException(nameof(batchProcessor));

        _batchSize = batchSize;
        _timeoutMs = timeoutMs;
        _batchProcessor = batchProcessor;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Process items in batches
    public async Task ProcessAsync(IEnumerable<T> items)
    {
        try
        {
            if (items == null)
                return;

            var itemList = items.ToList();
            _logger.LogInformation("Starting batch processing: {ItemCount} items, batch size: {BatchSize}", itemList.Count, _batchSize);

            var totalBatches = (itemList.Count + _batchSize - 1) / _batchSize;
            var processedBatches = 0;

            for (int i = 0; i < itemList.Count; i += _batchSize)
            {
                var batch = itemList.Skip(i).Take(_batchSize).ToList();
                await _processBatchWithTimeout(batch);
                processedBatches++;

                var progress = (double)processedBatches / totalBatches * 100;
                _logger.LogDebug("Batch processing progress: {Progress:F1}%", progress);
            }

            _logger.LogInformation("Batch processing completed: {TotalBatches} batches processed", processedBatches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during batch processing");
            throw;
        }
    }

    // Process single item
    public async Task ProcessSingleAsync(T item)
    {
        try
        {
            await ProcessAsync(new[] { item });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing single item");
            throw;
        }
    }

    private async Task _processBatchWithTimeout(List<T> batch)
    {
        try
        {
            var task = _batchProcessor(batch);
            var completedTask = await Task.WhenAny(task, Task.Delay(_timeoutMs));

            if (completedTask != task)
            {
                _logger.LogWarning("Batch processing timeout after {TimeoutMs}ms", _timeoutMs);
                throw new TimeoutException($"Batch processing exceeded timeout of {_timeoutMs}ms");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch");
            throw;
        }
    }
}

/// <summary>
/// Batch processing result with metrics.
/// </summary>
public class BatchProcessingResult
{
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int FailedItems { get; set; }
    public int TotalBatches { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
    public bool Success => FailedItems == 0;
}
