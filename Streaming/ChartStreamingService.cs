// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

namespace SkiaSharpChartEngine.Streaming;

/// <summary>
/// Default implementation of <see cref="IChartStreamingService"/>.
/// Uses a <see cref="Channel{T}"/> per registered chart for lock-free, bounded buffering
/// and produces rendered frames via <see cref="IChartRenderingService"/>.
/// </summary>
public sealed class ChartStreamingService : IChartStreamingService, IDisposable
{
    private record StreamEntry(
        Chart Chart,
        Channel<StreamDataPoint> Channel,
        StreamingChartOptions Options);

    private readonly ConcurrentDictionary<string, StreamEntry> _entries = new();
    private readonly IChartRenderingService _renderingService;
    private readonly ILogger<ChartStreamingService> _logger;
    private bool _disposed;

    public ChartStreamingService(
        IChartRenderingService renderingService,
        ILogger<ChartStreamingService> logger)
    {
        _renderingService = renderingService ?? throw new ArgumentNullException(nameof(renderingService));
        _logger           = logger           ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void Register(Chart chart, StreamingChartOptions? options = null)
    {
        if (chart == null) throw new ArgumentNullException(nameof(chart));
        options ??= new StreamingChartOptions();

        var channel = Channel.CreateBounded<StreamDataPoint>(new BoundedChannelOptions(options.MaxBufferSize)
        {
            FullMode     = BoundedChannelFullMode.DropOldest,
            SingleReader = false,
            SingleWriter = false
        });

        var entry = new StreamEntry(chart, channel, options);
        _entries.AddOrUpdate(chart.Id, entry, (_, _) => entry);
        _logger.LogInformation("Chart registered for streaming – chartId={ChartId}", chart.Id);
    }

    /// <inheritdoc/>
    public void Unregister(string chartId)
    {
        if (_entries.TryRemove(chartId, out var entry))
        {
            entry.Channel.Writer.TryComplete();
            _logger.LogInformation("Chart unregistered from streaming – chartId={ChartId}", chartId);
        }
    }

    /// <inheritdoc/>
    public bool Publish(string chartId, StreamDataPoint point)
    {
        if (point == null) throw new ArgumentNullException(nameof(point));

        if (!_entries.TryGetValue(chartId, out var entry))
            throw new InvalidOperationException($"Chart '{chartId}' is not registered for streaming.");

        var written = entry.Channel.Writer.TryWrite(point);
        if (!written)
            _logger.LogWarning("Buffer full, point dropped – chartId={ChartId}, series={Series}", chartId, point.SeriesName);

        return written;
    }

    /// <inheritdoc/>
    public int PublishBatch(string chartId, IEnumerable<StreamDataPoint> points)
    {
        if (points == null) throw new ArgumentNullException(nameof(points));

        var count = 0;
        foreach (var p in points)
            if (Publish(chartId, p))
                count++;
        return count;
    }

    /// <inheritdoc/>
    public Chart GetSnapshot(string chartId)
    {
        if (!_entries.TryGetValue(chartId, out var entry))
            throw new InvalidOperationException($"Chart '{chartId}' is not registered for streaming.");

        // Drain the channel synchronously to produce the latest snapshot
        ApplyBuffered(entry);
        return entry.Chart;
    }

    /// <inheritdoc/>
    public async Task<int> FlushAsync(string chartId, CancellationToken cancellationToken = default)
    {
        if (!_entries.TryGetValue(chartId, out var entry))
            throw new InvalidOperationException($"Chart '{chartId}' is not registered for streaming.");

        return await Task.Run(() => ApplyBuffered(entry), cancellationToken);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<StreamFrame> RenderFramesAsync(
        string chartId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_entries.TryGetValue(chartId, out var entry))
            throw new InvalidOperationException($"Chart '{chartId}' is not registered for streaming.");

        long frameNumber = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var delayMs = entry.Options.FlushIntervalMs;

            try
            {
                await Task.Delay(delayMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }

            var applied = ApplyBuffered(entry);
            if (applied == 0)
                continue; // nothing new – skip this frame

            var sw = Stopwatch.StartNew();
            RenderResult result;
            try
            {
                result = await _renderingService.RenderToByteArrayAsync(entry.Chart, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Frame render failed – chartId={ChartId}", chartId);
                continue;
            }

            sw.Stop();

            if (!result.Success || result.ImageData == null)
            {
                _logger.LogWarning("Frame render returned no data – chartId={ChartId}", chartId);
                continue;
            }

            frameNumber++;
            yield return new StreamFrame
            {
                ChartId     = chartId,
                FrameNumber = frameNumber,
                ImageData   = result.ImageData,
                RenderTimeMs = sw.ElapsedMilliseconds
            };

            _logger.LogDebug(
                "Frame rendered – chartId={ChartId}, frame={FrameNumber}, renderMs={RenderMs}",
                chartId, frameNumber, sw.ElapsedMilliseconds);
        }
    }

    // -------------------------------------------------------------------------
    // Drains the channel and applies points to the chart's series.
    // Returns the number of points applied.
    private int ApplyBuffered(StreamEntry entry)
    {
        var count = 0;
        while (entry.Channel.Reader.TryRead(out var item))
        {
            ApplyPoint(entry, item);
            count++;
        }
        return count;
    }

    private void ApplyPoint(StreamEntry entry, StreamDataPoint item)
    {
        var series = entry.Chart.GetSeriesByName(item.SeriesName);
        if (series == null)
        {
            // Auto-create series when it doesn't exist yet
            series = new ChartSeries(item.SeriesName);
            entry.Chart.AddSeries(series);
        }

        if (entry.Options.ReplaceOnUpdate)
            series.ClearDataPoints();

        series.AddDataPoint(item.X, item.Y, item.Label);

        // Enforce sliding window
        if (entry.Options.WindowSize > 0)
        {
            while (series.GetDataPointCount() > entry.Options.WindowSize)
                series.RemoveDataPoint(0);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var entry in _entries.Values)
            entry.Channel.Writer.TryComplete();

        _entries.Clear();
    }
}
