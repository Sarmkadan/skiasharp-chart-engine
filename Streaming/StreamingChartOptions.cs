// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SkiaSharpChartEngine.Streaming;

/// <summary>
/// Configuration options for real-time chart data streaming.
/// </summary>
public sealed class StreamingChartOptions
{
    private int _maxBufferSize = 1000;
    private int _windowSize = 0;
    private int _flushIntervalMs = 100;

    /// <summary>
    /// Gets or sets the maximum number of buffered data items waiting to be consumed.
    /// When the buffer is full, new items are dropped and a warning is logged.
    /// Defaults to 1000.
    /// </summary>
    public int MaxBufferSize
    {
        get => _maxBufferSize;
        set
        {
            if (value < 1) throw new ArgumentOutOfRangeException(nameof(value), "MaxBufferSize must be at least 1.");
            _maxBufferSize = value;
        }
    }

    /// <summary>
    /// Gets or sets the sliding-window size for the chart series.
    /// When greater than zero, only the most recent <c>WindowSize</c> data points
    /// are retained in each series after every update.
    /// Set to 0 to disable windowing (accumulate all points).
    /// </summary>
    public int WindowSize
    {
        get => _windowSize;
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "WindowSize must be >= 0.");
            _windowSize = value;
        }
    }

    /// <summary>
    /// Gets or sets the interval in milliseconds between automatic chart re-renders
    /// when using <see cref="IChartStreamingService.RenderFramesAsync"/>.
    /// Defaults to 100 ms (10 fps).
    /// </summary>
    public int FlushIntervalMs
    {
        get => _flushIntervalMs;
        set
        {
            if (value < 10) throw new ArgumentOutOfRangeException(nameof(value), "FlushIntervalMs must be at least 10.");
            _flushIntervalMs = value;
        }
    }

    /// <summary>
    /// Gets or sets whether to replace the entire series on each batch update
    /// instead of appending. Defaults to <c>false</c>.
    /// </summary>
    public bool ReplaceOnUpdate { get; set; } = false;
}

/// <summary>
/// A single streaming data item delivered to a chart series.
/// </summary>
public sealed class StreamDataPoint
{
    /// <summary>Gets or sets the name of the target series.</summary>
    public required string SeriesName { get; init; }

    /// <summary>Gets or sets the X value.</summary>
    public double X { get; init; }

    /// <summary>Gets or sets the Y value.</summary>
    public double Y { get; init; }

    /// <summary>Gets or sets an optional display label for the data point.</summary>
    public string? Label { get; init; }

    /// <summary>Gets the UTC timestamp when this item was produced.</summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}

/// <summary>
/// A rendered frame produced by <see cref="IChartStreamingService.RenderFramesAsync"/>.
/// </summary>
public sealed class StreamFrame
{
    /// <summary>Gets the chart identifier this frame belongs to.</summary>
    public required string ChartId { get; init; }

    /// <summary>Gets the sequence number of this frame (starts at 1).</summary>
    public long FrameNumber { get; init; }

    /// <summary>Gets the rendered image bytes in PNG format.</summary>
    public required byte[] ImageData { get; init; }

    /// <summary>Gets the UTC timestamp when this frame was rendered.</summary>
    public DateTime RenderedAt { get; } = DateTime.UtcNow;

    /// <summary>Gets the render duration in milliseconds.</summary>
    public long RenderTimeMs { get; init; }
}
