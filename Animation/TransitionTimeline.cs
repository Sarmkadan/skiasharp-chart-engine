// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Animation;

/// <summary>
/// Defines a sequence of chart states distributed along a time axis. Each state is captured
/// as a <see cref="TransitionKeyframe"/>; the engine interpolates data values between
/// consecutive keyframes to produce smooth animated transitions.
/// </summary>
/// <remarks>
/// <para>
/// Build a timeline with the fluent API and pass it to
/// <see cref="IChartTransitionEngine.RenderTransitionAsync(TransitionTimeline, TransitionOptions?, CancellationToken)"/>.
/// </para>
/// <para>
/// Typical patterns:
/// <code>
/// // Two-chart shorthand
/// var tl = TransitionTimeline.Between(chartA, chartB, durationMs: 600);
///
/// // Multi-step sequence
/// var tl = new TransitionTimeline()
///     .StartWith(chartA)
///     .AppendTransition(chartB, 400, TransitionEasing.EaseOutBack)
///     .AppendTransition(chartC, 600, TransitionEasing.Spring)
///     .Repeat(3);
/// </code>
/// </para>
/// </remarks>
public sealed class TransitionTimeline
{
    private readonly List<TransitionKeyframe> _keyframes = new();
    private int _loopCount = 1;

    // ── Properties ─────────────────────────────────────────────────────────

    /// <summary>Gets the keyframes that make up this timeline, in insertion order.</summary>
    /// <remarks>Use <see cref="GetSegments"/> to enumerate consecutive pairs in time order.</remarks>
    public IReadOnlyList<TransitionKeyframe> Keyframes => _keyframes.AsReadOnly();

    /// <summary>
    /// Gets the total duration of the timeline in milliseconds, defined as the time
    /// offset of the last keyframe.  Returns 0 when no keyframes have been added.
    /// </summary>
    public double TotalDurationMs => _keyframes.Count > 0 ? _keyframes[^1].TimeMs : 0;

    /// <summary>Gets the number of playback repetitions configured via <see cref="Repeat"/>.</summary>
    public int LoopCount => _loopCount;

    // ── Fluent builder ──────────────────────────────────────────────────────

    /// <summary>
    /// Inserts a keyframe at an explicit time offset. Keyframes added out of order are
    /// accepted and sorted automatically when the timeline is validated or enumerated.
    /// </summary>
    /// <param name="chart">Chart state to capture at this point in time.</param>
    /// <param name="timeMs">
    /// Absolute time in milliseconds from the timeline start. Must be non-negative.
    /// </param>
    /// <param name="easing">
    /// Easing applied when transitioning <em>from</em> this keyframe to the next.
    /// The value of the final keyframe's easing is not used.
    /// </param>
    /// <returns>This timeline instance, for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="chart"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="timeMs"/> is negative.
    /// </exception>
    public TransitionTimeline AddKeyframe(
        Chart           chart,
        double          timeMs,
        TransitionEasing easing = TransitionEasing.EaseInOutCubic)
    {
        ArgumentNullException.ThrowIfNull(chart);
        if (timeMs < 0)
            throw new ArgumentOutOfRangeException(nameof(timeMs),
                "Keyframe time offset must be non-negative.");

        _keyframes.Add(new TransitionKeyframe(chart, timeMs, easing));
        return this;
    }

    /// <summary>
    /// Appends a new keyframe immediately after the current last keyframe, extending the
    /// timeline by <paramref name="durationMs"/> milliseconds.
    /// </summary>
    /// <param name="toChart">The chart state to transition into.</param>
    /// <param name="durationMs">Duration of this new segment in milliseconds. Must be positive.</param>
    /// <param name="easing">
    /// Easing applied when transitioning from the <em>current</em> last keyframe to
    /// <paramref name="toChart"/>. The current last keyframe's easing is overwritten.
    /// </param>
    /// <returns>This timeline instance, for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="toChart"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="durationMs"/> is not positive.
    /// </exception>
    public TransitionTimeline AppendTransition(
        Chart           toChart,
        double          durationMs,
        TransitionEasing easing = TransitionEasing.EaseInOutCubic)
    {
        ArgumentNullException.ThrowIfNull(toChart);
        if (durationMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMs), "Segment duration must be positive.");

        // Patch the easing of the previous last keyframe so it drives this new segment.
        if (_keyframes.Count > 0)
        {
            var prev = _keyframes[^1];
            _keyframes[^1] = prev with { Easing = easing };
        }

        _keyframes.Add(new TransitionKeyframe(toChart, TotalDurationMs + durationMs, easing));
        return this;
    }

    /// <summary>
    /// Inserts a keyframe for <paramref name="chart"/> at time zero, making it the
    /// unambiguous starting state of the animation.
    /// </summary>
    /// <param name="chart">The opening chart state.</param>
    /// <returns>This timeline instance, for fluent chaining.</returns>
    public TransitionTimeline StartWith(Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);
        _keyframes.Insert(0, new TransitionKeyframe(chart, 0));
        return this;
    }

    /// <summary>
    /// Configures how many times the animation repeats.
    /// </summary>
    /// <param name="count">Number of repetitions. Pass <c>-1</c> for infinite.</param>
    /// <returns>This timeline instance, for fluent chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="count"/> is 0 or less than -1.
    /// </exception>
    public TransitionTimeline Repeat(int count)
    {
        if (count < -1 || count == 0)
            throw new ArgumentOutOfRangeException(nameof(count),
                "Repeat count must be -1 (infinite) or a positive integer.");
        _loopCount = count;
        return this;
    }

    // ── Factory methods ─────────────────────────────────────────────────────

    /// <summary>
    /// Creates a two-keyframe timeline for a simple transition between two chart states.
    /// The first keyframe is placed at time 0, the second at <paramref name="durationMs"/>.
    /// </summary>
    /// <param name="from">Initial chart state.</param>
    /// <param name="to">Final chart state.</param>
    /// <param name="durationMs">Total transition duration in milliseconds. Must be positive.</param>
    /// <param name="easing">Easing applied to the single transition segment.</param>
    public static TransitionTimeline Between(
        Chart           from,
        Chart           to,
        double          durationMs,
        TransitionEasing easing = TransitionEasing.EaseInOutCubic)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        if (durationMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMs), "Duration must be positive.");

        return new TransitionTimeline()
            .AddKeyframe(from, 0, easing)
            .AddKeyframe(to, durationMs, easing);
    }

    /// <summary>
    /// Creates a multi-step timeline from a sequence of (chart, segmentDurationMs) pairs.
    /// The first pair anchors the opening state at time zero; each subsequent pair appends
    /// a new segment.
    /// </summary>
    /// <param name="easing">
    /// Uniform easing applied to every segment. Pass individual <see cref="AddKeyframe"/>
    /// calls for per-segment easing control.
    /// </param>
    /// <param name="steps">
    /// Ordered sequence of (chart state, segment duration in ms) pairs.
    /// Requires at least two entries.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when fewer than two steps are provided.
    /// </exception>
    public static TransitionTimeline FromSteps(
        TransitionEasing              easing,
        params (Chart Chart, double DurationMs)[] steps)
    {
        if (steps.Length < 2)
            throw new ArgumentException("At least two steps are required.", nameof(steps));

        var timeline = new TransitionTimeline();
        timeline.AddKeyframe(steps[0].Chart, 0, easing);

        for (int i = 1; i < steps.Length; i++)
            timeline.AppendTransition(steps[i].Chart, steps[i].DurationMs, easing);

        return timeline;
    }

    // ── Enumeration ─────────────────────────────────────────────────────────

    /// <summary>
    /// Enumerates consecutive (From, To) keyframe pairs in strict ascending time order.
    /// Each pair defines one renderable segment of the animation.
    /// </summary>
    public IEnumerable<(TransitionKeyframe From, TransitionKeyframe To)> GetSegments()
    {
        var sorted = _keyframes.OrderBy(k => k.TimeMs).ToList();
        for (int i = 0; i < sorted.Count - 1; i++)
            yield return (sorted[i], sorted[i + 1]);
    }

    // ── Validation ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the timeline is renderable, throwing
    /// <see cref="InvalidOperationException"/> on the first violation found.
    /// </summary>
    /// <remarks>
    /// Checks performed:
    /// <list type="bullet">
    ///   <item>At least two keyframes are present.</item>
    ///   <item>All keyframe time offsets are strictly ascending.</item>
    ///   <item>No <see cref="TransitionKeyframe.Chart"/> reference is <see langword="null"/>.</item>
    /// </list>
    /// </remarks>
    public void Validate()
    {
        if (_keyframes.Count < 2)
            throw new InvalidOperationException(
                "A TransitionTimeline must contain at least two keyframes.");

        var sorted = _keyframes.OrderBy(k => k.TimeMs).ToList();

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            if (sorted[i].Chart is null)
                throw new InvalidOperationException(
                    $"Keyframe at index {i} has a null Chart reference.");

            if (sorted[i].TimeMs >= sorted[i + 1].TimeMs)
                throw new InvalidOperationException(
                    $"Keyframe at time {sorted[i].TimeMs}ms must be strictly before " +
                    $"the following keyframe at {sorted[i + 1].TimeMs}ms.");
        }

        if (sorted[^1].Chart is null)
            throw new InvalidOperationException(
                $"The final keyframe has a null Chart reference.");
    }

    /// <inheritdoc/>
    public override string ToString()
        => $"TransitionTimeline(Keyframes={_keyframes.Count}, " +
           $"Duration={TotalDurationMs}ms, LoopCount={_loopCount})";
}
