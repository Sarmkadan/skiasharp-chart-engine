// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Semaphore-based concurrency limiter for throttling concurrent operations.
/// Useful for limiting CPU/memory usage during intensive rendering operations.
/// </summary>
public class ConcurrencyLimiter : IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<ConcurrencyLimiter> _logger;
    private readonly int _maxConcurrency;
    private bool _disposed;

    public ConcurrencyLimiter(int maxConcurrency, ILogger<ConcurrencyLimiter> logger)
    {
        if (maxConcurrency <= 0)
            throw new ArgumentException("Max concurrency must be positive", nameof(maxConcurrency));

        _maxConcurrency = maxConcurrency;
        _semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Execute operation with concurrency control
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogDebug("Operation started, available slots: {Slots}", _semaphore.CurrentCount);
            return await operation();
        }
        finally
        {
            _semaphore.Release();
            _logger.LogDebug("Operation completed, available slots: {Slots}", _semaphore.CurrentCount);
        }
    }

    // Execute operation without return value
    public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogDebug("Operation started, available slots: {Slots}", _semaphore.CurrentCount);
            await operation();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Execute synchronous operation
    public T Execute<T>(Func<T> operation)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        _semaphore.Wait();
        try
        {
            return operation();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Get current available slots
    public int GetAvailableSlots() => _semaphore.CurrentCount;

    // Get used slots
    public int GetUsedSlots() => _maxConcurrency - _semaphore.CurrentCount;

    // Wait until slot available
    public async Task WaitAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        _semaphore.Release();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _semaphore?.Dispose();
        _disposed = true;
        _logger.LogInformation("ConcurrencyLimiter disposed");
    }
}
