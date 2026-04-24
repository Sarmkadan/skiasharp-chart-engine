// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SkiaSharpChartEngine.Animation;

/// <summary>Specifies how the animation plays back when it reaches the final frame.</summary>
public enum PlaybackMode
{
    /// <summary>Play the animation once and hold the final frame.</summary>
    Once = 0,

    /// <summary>Loop continuously from the first frame back to the first frame.</summary>
    Loop = 1,

    /// <summary>
    /// Alternate direction on each iteration (ping-pong). The animation plays forward,
    /// then in reverse, then forward again, and so on.
    /// </summary>
    Bounce = 2,
}

/// <summary>
/// Configuration that controls how a <see cref="TransitionTimeline"/> is rendered.
/// All mutation goes through validated property setters; use <see cref="Clone"/> to
/// derive a modified copy without altering the original.
/// </summary>
public sealed class TransitionOptions
{
    private int    _frameRate = 60;
    private int    _quality   = 95;
    private int    _loopCount = 1;
    private double _preDelayMs;
    private double _postDelayMs;

    // ── Core render settings ───────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the output frame rate in frames per second.
    /// Valid range: 1–120. Default: 60.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is outside [1, 120].
    /// </exception>
    public int FrameRate
    {
        get => _frameRate;
        set
        {
            if (value < 1 || value > 120)
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"FrameRate must be between 1 and 120, got {value}.");
            _frameRate = value;
        }
    }

    /// <summary>
    /// Gets or sets the PNG encoding quality in the range [1, 100].
    /// Higher values produce larger, higher-fidelity files. Default: 95.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is outside [1, 100].
    /// </exception>
    public int Quality
    {
        get => _quality;
        set
        {
            if (value < 1 || value > 100)
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"Quality must be between 1 and 100, got {value}.");
            _quality = value;
        }
    }

    // ── Playback settings ──────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the playback mode used by the consumer when cycling through frames.
    /// Default: <see cref="PlaybackMode.Once"/>.
    /// </summary>
    public PlaybackMode Playback { get; set; } = PlaybackMode.Once;

    /// <summary>
    /// Gets or sets the number of playback repetitions.
    /// Meaningful only when <see cref="Playback"/> is not <see cref="PlaybackMode.Once"/>.
    /// Use <c>-1</c> for infinite. Default: 1.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is 0 or less than -1.
    /// </exception>
    public int LoopCount
    {
        get => _loopCount;
        set
        {
            if (value < -1 || value == 0)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "LoopCount must be -1 (infinite) or a positive integer.");
            _loopCount = value;
        }
    }

    /// <summary>
    /// Gets or sets the silence period in milliseconds inserted before the animation starts.
    /// The first keyframe is held for this duration. Default: 0.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public double PreDelayMs
    {
        get => _preDelayMs;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "PreDelayMs must be non-negative.");
            _preDelayMs = value;
        }
    }

    /// <summary>
    /// Gets or sets the hold duration in milliseconds after the animation ends.
    /// The final keyframe is held for this duration. Default: 0.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    public double PostDelayMs
    {
        get => _postDelayMs;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "PostDelayMs must be non-negative.");
            _postDelayMs = value;
        }
    }

    // ── Performance settings ───────────────────────────────────────────────

    /// <summary>
    /// Gets or sets whether frames may be rendered concurrently using
    /// <see cref="System.Threading.Tasks.Task.WhenAll"/>.
    /// Disable when the underlying rendering service is not thread-safe. Default: <see langword="false"/>.
    /// </summary>
    public bool EnableParallelRendering { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of concurrent rendering tasks when
    /// <see cref="EnableParallelRendering"/> is enabled.
    /// Must be at least 1. Default: 4.
    /// </summary>
    public int MaxParallelism { get; set; } = 4;

    // ── Diagnostics settings ───────────────────────────────────────────────

    /// <summary>
    /// Gets or sets whether per-frame render timings are collected and attached to
    /// <see cref="TransitionResult.Metadata"/> under the key <c>"frameTimingsMs"</c>.
    /// Useful for profiling but incurs a small overhead. Default: <see langword="false"/>.
    /// </summary>
    public bool CollectFrameTimings { get; set; } = false;

    // ── Computed helpers ───────────────────────────────────────────────────

    /// <summary>Gets the interval between frames in milliseconds: <c>1000 / FrameRate</c>.</summary>
    public double FrameIntervalMs => 1000.0 / _frameRate;

    // ── Validation ─────────────────────────────────────────────────────────

    /// <summary>
    /// Validates this instance, throwing <see cref="InvalidOperationException"/> on any
    /// invalid combination of settings.
    /// </summary>
    public void Validate()
    {
        if (_frameRate < 1 || _frameRate > 120)
            throw new InvalidOperationException($"FrameRate {_frameRate} is outside the valid range [1, 120].");
        if (_quality < 1 || _quality > 100)
            throw new InvalidOperationException($"Quality {_quality} is outside the valid range [1, 100].");
        if (MaxParallelism < 1)
            throw new InvalidOperationException($"MaxParallelism must be at least 1, got {MaxParallelism}.");
        if (_preDelayMs < 0)
            throw new InvalidOperationException("PreDelayMs must be non-negative.");
        if (_postDelayMs < 0)
            throw new InvalidOperationException("PostDelayMs must be non-negative.");
    }

    // ── Clone & factories ──────────────────────────────────────────────────

    /// <summary>Creates an independent deep copy of this instance.</summary>
    public TransitionOptions Clone()
    {
        var clone = new TransitionOptions();
        clone._frameRate             = _frameRate;
        clone._quality               = _quality;
        clone.Playback               = Playback;
        clone._loopCount             = _loopCount;
        clone._preDelayMs            = _preDelayMs;
        clone._postDelayMs           = _postDelayMs;
        clone.EnableParallelRendering = EnableParallelRendering;
        clone.MaxParallelism         = MaxParallelism;
        clone.CollectFrameTimings    = CollectFrameTimings;
        return clone;
    }

    /// <summary>
    /// Returns a <see cref="TransitionOptions"/> preset optimised for high-quality output
    /// at 60 fps with maximum PNG fidelity.
    /// </summary>
    public static TransitionOptions HighQuality => new() { FrameRate = 60, Quality = 100 };

    /// <summary>
    /// Returns a <see cref="TransitionOptions"/> preset optimised for fast preview rendering
    /// at 24 fps with reduced quality.
    /// </summary>
    public static TransitionOptions Preview => new() { FrameRate = 24, Quality = 75 };

    /// <summary>
    /// Returns a <see cref="TransitionOptions"/> preset for looping GIF-like playback
    /// at 30 fps.
    /// </summary>
    public static TransitionOptions Looping => new()
    {
        FrameRate = 30,
        Quality   = 90,
        Playback  = PlaybackMode.Loop,
        LoopCount = -1,
    };

    /// <inheritdoc/>
    public override string ToString()
        => $"TransitionOptions(FrameRate={_frameRate}, Quality={_quality}, Playback={Playback})";
}
