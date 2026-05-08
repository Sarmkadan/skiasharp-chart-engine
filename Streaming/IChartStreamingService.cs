// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Streaming;

/// <summary>
/// Provides real-time data streaming into chart series and produces rendered frames
/// as an asynchronous sequence.
/// </summary>
public interface IChartStreamingService
{
    /// <summary>
    /// Writes a single data point to the streaming buffer for the given chart.
    /// Returns <c>false</c> when the buffer is full and the item was dropped.
    /// </summary>
    bool Publish(string chartId, StreamDataPoint point);

    /// <summary>
    /// Writes a batch of data points for the given chart.
    /// Returns the number of items that were successfully enqueued.
    /// </summary>
    int PublishBatch(string chartId, IEnumerable<StreamDataPoint> points);

    /// <summary>
    /// Registers a chart to receive streamed data.
    /// Multiple calls with the same <paramref name="chartId"/> are idempotent.
    /// </summary>
    void Register(Chart chart, StreamingChartOptions? options = null);

    /// <summary>
    /// Unregisters a chart and releases its buffer.
    /// Any in-flight data for this chart is discarded.
    /// </summary>
    void Unregister(string chartId);

    /// <summary>
    /// Returns the current snapshot of a chart after applying all buffered updates.
    /// </summary>
    Chart GetSnapshot(string chartId);

    /// <summary>
    /// Produces an infinite asynchronous sequence of rendered PNG frames for the given chart.
    /// Each frame reflects all data points received since the previous frame.
    /// The sequence ends when <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    IAsyncEnumerable<StreamFrame> RenderFramesAsync(
        string chartId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consumes all buffered data points and applies them to the chart, then
    /// returns the total number of points applied.
    /// </summary>
    Task<int> FlushAsync(string chartId, CancellationToken cancellationToken = default);
}
