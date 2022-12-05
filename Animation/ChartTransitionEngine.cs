// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Services;

namespace SkiaSharpChartEngine.Animation;

/// <summary>
/// Production implementation of <see cref="IChartTransitionEngine"/>.
/// </summary>
/// <remarks>
/// <para>
/// The engine executes the following pipeline for each call to
/// <see cref="RenderTransitionAsync(TransitionTimeline, TransitionOptions?, CancellationToken)"/>:
/// </para>
/// <list type="number">
///   <item>Validate inputs (timeline ≥ 2 keyframes, options in range).</item>
///   <item>Compute the frame schedule from the timeline duration and target frame rate.</item>
///   <item>
///     For each frame: resolve the owning timeline segment, evaluate the per-segment
///     easing, linearly interpolate all data-point coordinates, render the interpolated
///     chart via <see cref="IChartRenderingService"/>, collect the PNG bytes.
///   </item>
///   <item>Return a <see cref="TransitionResult"/> with all frames and render metadata.</item>
/// </list>
/// <para>
/// Thread safety: the engine itself is stateless. Whether frame rendering is safe to
/// parallelise depends on the injected <see cref="IChartRenderingService"/>. Control
/// concurrency through <see cref="TransitionOptions.EnableParallelRendering"/> and
/// <see cref="TransitionOptions.MaxParallelism"/>.
/// </para>
/// </remarks>
public sealed class ChartTransitionEngine : IChartTransitionEngine
{
    private readonly IChartRenderingService        _renderer;
    private readonly ILogger<ChartTransitionEngine> _logger;

    /// <summary>
    /// Initialises the engine with the required dependencies.
    /// </summary>
    /// <param name="renderer">
    /// Rendering service used to produce PNG-encoded bytes for each interpolated frame.
    /// </param>
    /// <param name="logger">Structured logger for operation telemetry.</param>
    public ChartTransitionEngine(
        IChartRenderingService        renderer,
        ILogger<ChartTransitionEngine> logger)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _logger   = logger   ?? throw new ArgumentNullException(nameof(logger));
    }

    // ── IChartTransitionEngine ──────────────────────────────────────────────

    /// <inheritdoc/>
    public TransitionTimeline CreateTimeline() => new();

    /// <inheritdoc/>
    public async Task<TransitionResult> RenderTransitionAsync(
        Chart              from,
        Chart              to,
        AnimationSettings? settings          = null,
        TransitionOptions? options           = null,
        CancellationToken  cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        settings ??= new AnimationSettings();
        var easing   = MapEasingFunction(settings.EasingType);
        var timeline = TransitionTimeline.Between(from, to, settings.DurationMs, easing);

        return await RenderTransitionAsync(timeline, options, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TransitionResult> RenderTransitionAsync(
        TransitionTimeline timeline,
        TransitionOptions? options           = null,
        CancellationToken  cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(timeline);

        options ??= new TransitionOptions();
        var chartId = timeline.Keyframes.Count > 0
            ? timeline.Keyframes[0].Chart.Id
            : "unknown";

        try
        {
            options.Validate();
            timeline.Validate();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transition validation failed for chart {ChartId}.", chartId);
            return TransitionResult.CreateFailure(chartId, ex.Message, ex);
        }

        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "Rendering transition — chart={ChartId}, keyframes={Keyframes}, " +
            "duration={Duration}ms, frameRate={FrameRate}fps.",
            chartId, timeline.Keyframes.Count, timeline.TotalDurationMs, options.FrameRate);

        try
        {
            var frames = await BuildFrameSequenceAsync(timeline, options, cancellationToken);
            sw.Stop();

            _logger.LogInformation(
                "Transition rendered — {FrameCount} frames in {Elapsed}ms.",
                frames.Count, sw.ElapsedMilliseconds);

            var metadata = BuildMetadata(timeline, frames, options);
            return TransitionResult.CreateSuccess(
                chartId, frames, timeline.TotalDurationMs, sw.ElapsedMilliseconds, metadata);
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            _logger.LogWarning("Transition rendering cancelled — chart={ChartId}.", chartId);
            return TransitionResult.CreateFailure(chartId, "Rendering was cancelled.");
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Transition rendering failed — chart={ChartId}.", chartId);
            return TransitionResult.CreateFailure(chartId, ex.Message, ex);
        }
    }

    /// <inheritdoc/>
    public async Task<byte[]> RenderFrameAsync(
        Chart             from,
        Chart             to,
        double            progress,
        TransitionEasing  easing            = TransitionEasing.EaseInOutCubic,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        double easedProgress = TransitionEasingCalculator.Calculate(easing, progress);
        var interpolated     = InterpolateChartState(from, to, easedProgress);

        var result = await _renderer.RenderToByteArrayAsync(interpolated, cancellationToken);

        if (!result.Success || result.ImageData is null)
            throw new InvalidOperationException(
                result.ErrorMessage ?? "Frame rendering returned no image data.");

        return result.ImageData;
    }

    // ── Frame scheduling ────────────────────────────────────────────────────

    private async Task<List<TransitionRenderFrame>> BuildFrameSequenceAsync(
        TransitionTimeline timeline,
        TransitionOptions  options,
        CancellationToken  cancellationToken)
    {
        double startMs     = timeline.Keyframes.Min(k => k.TimeMs);
        double endMs       = timeline.TotalDurationMs;
        double durationMs  = endMs - startMs;
        double intervalMs  = options.FrameIntervalMs;
        int    totalFrames = (int)Math.Ceiling(durationMs / intervalMs) + 1;

        var schedule = new List<(int Index, double TimeMs)>(totalFrames);
        for (int i = 0; i < totalFrames; i++)
            schedule.Add((i, startMs + Math.Min(i * intervalMs, durationMs)));

        return options.EnableParallelRendering
            ? await RenderParallelAsync(timeline, schedule, durationMs, options, cancellationToken)
            : await RenderSequentialAsync(timeline, schedule, durationMs, options, cancellationToken);
    }

    private async Task<List<TransitionRenderFrame>> RenderSequentialAsync(
        TransitionTimeline                   timeline,
        List<(int Index, double TimeMs)>     schedule,
        double                               durationMs,
        TransitionOptions                    options,
        CancellationToken                    cancellationToken)
    {
        var frames = new List<TransitionRenderFrame>(schedule.Count);
        foreach (var (index, timeMs) in schedule)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var frame = await RenderOneFrameAsync(
                timeline, index, timeMs, durationMs, options, cancellationToken);
            frames.Add(frame);
        }
        return frames;
    }

    private async Task<List<TransitionRenderFrame>> RenderParallelAsync(
        TransitionTimeline                   timeline,
        List<(int Index, double TimeMs)>     schedule,
        double                               durationMs,
        TransitionOptions                    options,
        CancellationToken                    cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(options.MaxParallelism, options.MaxParallelism);

        var tasks = schedule.Select(async entry =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await RenderOneFrameAsync(
                    timeline, entry.Index, entry.TimeMs, durationMs, options, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var unordered = await Task.WhenAll(tasks);
        return unordered.OrderBy(f => f.FrameIndex).ToList();
    }

    private async Task<TransitionRenderFrame> RenderOneFrameAsync(
        TransitionTimeline timeline,
        int                frameIndex,
        double             timeMs,
        double             durationMs,
        TransitionOptions  options,
        CancellationToken  cancellationToken)
    {
        double overallProgress = durationMs > 0 ? timeMs / durationMs : 1.0;

        var (fromKf, toKf, localProgress, easing) = ResolveSegment(timeline, timeMs);
        double easedProgress = TransitionEasingCalculator.Calculate(easing, localProgress);
        var interpolated     = InterpolateChartState(fromKf.Chart, toKf.Chart, easedProgress);

        Stopwatch? frameSw = options.CollectFrameTimings ? Stopwatch.StartNew() : null;

        var renderResult = await _renderer.RenderToByteArrayAsync(interpolated, cancellationToken);

        frameSw?.Stop();

        if (!renderResult.Success || renderResult.ImageData is null)
            throw new InvalidOperationException(
                $"Frame {frameIndex} at {timeMs:F1}ms failed: {renderResult.ErrorMessage}");

        _logger.LogTrace(
            "Frame {Index}/{Progress:P0} rendered in {FrameMs}ms.",
            frameIndex, overallProgress, frameSw?.ElapsedMilliseconds ?? renderResult.RenderTimeMs);

        return new TransitionRenderFrame(
            frameIndex,
            timeMs,
            Math.Clamp(overallProgress, 0.0, 1.0),
            renderResult.ImageData);
    }

    // ── Segment resolution ──────────────────────────────────────────────────

    /// <summary>
    /// Finds which (from, to) segment owns <paramref name="timeMs"/> and returns the
    /// local progress [0, 1] within that segment along with its easing function.
    /// </summary>
    private static (TransitionKeyframe From, TransitionKeyframe To, double LocalProgress, TransitionEasing Easing)
        ResolveSegment(TransitionTimeline timeline, double timeMs)
    {
        var segments = timeline.GetSegments().ToList();

        // Clamp to the opening segment for times before the first keyframe.
        var first = segments[0];
        if (timeMs <= first.From.TimeMs)
            return (first.From, first.To, 0.0, first.From.Easing);

        foreach (var (from, to) in segments)
        {
            if (timeMs <= to.TimeMs)
            {
                double segDuration   = to.TimeMs - from.TimeMs;
                double localProgress = segDuration > 0
                    ? (timeMs - from.TimeMs) / segDuration
                    : 1.0;
                return (from, to, Math.Clamp(localProgress, 0.0, 1.0), from.Easing);
            }
        }

        // Past the end — hold the final keyframe.
        var last = segments[^1];
        return (last.From, last.To, 1.0, last.From.Easing);
    }

    // ── Interpolation ───────────────────────────────────────────────────────

    /// <summary>
    /// Produces an interpolated chart whose data-point X/Y coordinates lie at
    /// <paramref name="easedProgress"/> between the corresponding points in
    /// <paramref name="from"/> and <paramref name="to"/>. Layout and styling are
    /// inherited from <paramref name="to"/> so the rendered frame always uses the
    /// target chart's configuration.
    /// </summary>
    private static Chart InterpolateChartState(Chart from, Chart to, double easedProgress)
    {
        // Share the target chart's configuration reference (read-only during rendering).
        var interpolated = new Chart(to.Configuration, to.Id)
        {
            CreatedAt = to.CreatedAt,
            Type      = to.Type,
        };

        int seriesCount = Math.Min(from.Series.Count, to.Series.Count);
        for (int si = 0; si < seriesCount; si++)
        {
            var fromSeries = from.Series[si];
            var toSeries   = to.Series[si];

            var newSeries = new ChartSeries(toSeries.Name, toSeries.Color, toSeries.SeriesType)
            {
                LineWidth  = toSeries.LineWidth,
                IsVisible  = toSeries.IsVisible,
                YAxisMin   = toSeries.YAxisMin,
                YAxisMax   = toSeries.YAxisMax,
                ZIndex     = toSeries.ZIndex,
            };

            int pointCount = Math.Min(fromSeries.DataPoints.Count, toSeries.DataPoints.Count);
            for (int pi = 0; pi < pointCount; pi++)
            {
                var fp = fromSeries.DataPoints[pi];
                var tp = toSeries.DataPoints[pi];

                newSeries.AddDataPoint(new DataPoint(
                    fp.X + (tp.X - fp.X) * easedProgress,
                    fp.Y + (tp.Y - fp.Y) * easedProgress,
                    tp.Label,
                    tp.Color));
            }

            interpolated.AddSeries(newSeries);
        }

        return interpolated;
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private static IReadOnlyDictionary<string, object> BuildMetadata(
        TransitionTimeline              timeline,
        IReadOnlyList<TransitionRenderFrame> frames,
        TransitionOptions               options)
    {
        var meta = new Dictionary<string, object>
        {
            ["frameCount"]      = frames.Count,
            ["keyframeCount"]   = timeline.Keyframes.Count,
            ["totalDurationMs"] = timeline.TotalDurationMs,
            ["frameRateFps"]    = options.FrameRate,
            ["playbackMode"]    = options.Playback.ToString(),
        };

        if (options.CollectFrameTimings)
        {
            meta["frameTimingsMs"] = frames
                .Select(f => new { f.FrameIndex, f.TimeMs, f.Progress })
                .ToList();
        }

        return meta;
    }

    /// <summary>
    /// Maps a <see cref="EasingFunction"/> (from <see cref="AnimationSettings"/>) to the
    /// equivalent <see cref="TransitionEasing"/> so callers can use either API interchangeably.
    /// </summary>
    private static TransitionEasing MapEasingFunction(EasingFunction source) => source switch
    {
        EasingFunction.Linear         => TransitionEasing.Linear,
        EasingFunction.EaseInQuad     => TransitionEasing.EaseInQuad,
        EasingFunction.EaseOutQuad    => TransitionEasing.EaseOutQuad,
        EasingFunction.EaseInOutQuad  => TransitionEasing.EaseInOutQuad,
        EasingFunction.EaseInCubic    => TransitionEasing.EaseInCubic,
        EasingFunction.EaseOutCubic   => TransitionEasing.EaseOutCubic,
        EasingFunction.EaseInOutCubic => TransitionEasing.EaseInOutCubic,
        EasingFunction.EaseInExpo     => TransitionEasing.EaseInExpo,
        EasingFunction.EaseOutExpo    => TransitionEasing.EaseOutExpo,
        EasingFunction.EaseInOutExpo  => TransitionEasing.EaseInOutExpo,
        _                             => TransitionEasing.EaseInOutCubic,
    };
}
