// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Animation;

/// <summary>
/// Provides extension methods for <see cref="TransitionTimeline"/> to simplify common timeline
/// construction patterns and queries.
/// </summary>
public static class TransitionTimelineExtensions
{
    /// <summary>
    /// Inserts a keyframe at the specified relative time (percentage of total duration).
    /// </summary>
    /// <param name="timeline">The timeline to extend.</param>
    /// <param name="chart">Chart state to capture at the relative time position.</param>
    /// <param name="relativeTimeMs">Relative time in milliseconds (0 = start, TotalDurationMs = end).</param>
    /// <param name="easing">Easing applied when transitioning from this keyframe to the next.</param>
    /// <returns>The same timeline instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when timeline or chart is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when relativeTimeMs is negative.</exception>
    public static TransitionTimeline AddKeyframeAt(
        this TransitionTimeline timeline,
        Chart chart,
        double relativeTimeMs,
        TransitionEasing easing = TransitionEasing.EaseInOutCubic)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(chart);

        if (relativeTimeMs < 0)
            throw new ArgumentOutOfRangeException(nameof(relativeTimeMs), "Relative time must be non-negative.");

        return timeline.AddKeyframe(chart, relativeTimeMs, easing);
    }

    /// <summary>
    /// Appends a transition that starts at a specific relative time position.
    /// </summary>
    /// <param name="timeline">The timeline to extend.</param>
    /// <param name="toChart">The chart state to transition into.</param>
    /// <param name="relativeStartTimeMs">
    /// Relative start time in milliseconds (0 = start, TotalDurationMs = end).
    /// Must be non-negative and less than or equal to the timeline's total duration.
    /// </param>
    /// <param name="durationMs">
    /// Duration of this new segment in milliseconds. Must be positive.
    /// </param>
    /// <param name="easing">
    /// Easing applied when transitioning from the previous keyframe to <paramref name="toChart"/>.
    /// </param>
    /// <returns>The same timeline instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeline"/> or <paramref name="toChart"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="relativeStartTimeMs"/> is negative or <paramref name="durationMs"/> is not positive.
    /// </exception>
    public static TransitionTimeline AppendTransitionAt(
        this TransitionTimeline timeline,
        Chart toChart,
        double relativeStartTimeMs,
        double durationMs,
        TransitionEasing easing = TransitionEasing.EaseInOutCubic)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(toChart);

        if (relativeStartTimeMs < 0)
            throw new ArgumentOutOfRangeException(nameof(relativeStartTimeMs), "Relative start time must be non-negative.");

        if (durationMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMs), "Segment duration must be positive.");

        double absoluteEndTime = relativeStartTimeMs + durationMs;

        // If this is the first keyframe, set it at time 0
        if (timeline.Keyframes.Count == 0)
        {
            timeline.AddKeyframe(toChart, 0, easing);
            return timeline;
        }

        // Find the keyframe at or before the relative start time
        var segments = timeline.GetSegments().ToList();
        TransitionKeyframe? fromKeyframe = null;

        foreach (var segment in segments)
        {
            if (segment.From.TimeMs <= relativeStartTimeMs && segment.To.TimeMs >= relativeStartTimeMs)
            {
                fromKeyframe = segment.From;
                break;
            }
        }

        // If we found a starting point within an existing segment, insert the transition there
        if (fromKeyframe != null)
        {
            // Insert a keyframe at the relative start time if it doesn't already exist
            if (timeline.Keyframes.All(k => Math.Abs(k.TimeMs - relativeStartTimeMs) > double.Epsilon))
            {
                timeline.AddKeyframe(toChart, relativeStartTimeMs, easing);
            }

            // Add the new keyframe at the end time
            timeline.AddKeyframe(toChart, absoluteEndTime, easing);
        }
        else
        {
            // Fallback: append at the end
            timeline.AppendTransition(toChart, durationMs, easing);
        }

        return timeline;
    }

    /// <summary>
    /// Creates a copy of this timeline with all keyframes shifted by a specified time offset.
    /// </summary>
    /// <param name="timeline">The timeline to copy and shift.</param>
    /// <param name="timeOffsetMs">Time offset in milliseconds to add to all keyframes.</param>
    /// <returns>A new timeline instance with shifted keyframes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when timeline is null.</exception>
    public static TransitionTimeline ShiftTime(
        this TransitionTimeline timeline,
        double timeOffsetMs)
    {
        ArgumentNullException.ThrowIfNull(timeline);

        return timeOffsetMs == 0
            ? timeline
            : CreateShiftedTimeline(timeline, timeOffsetMs);

        static TransitionTimeline CreateShiftedTimeline(TransitionTimeline timeline, double timeOffsetMs)
        {
            var newTimeline = new TransitionTimeline();
            newTimeline.Repeat(timeline.LoopCount);

            foreach (var keyframe in timeline.Keyframes)
            {
                newTimeline.AddKeyframe(keyframe.Chart, keyframe.TimeMs + timeOffsetMs, keyframe.Easing);
            }

            return newTimeline;
        }
    }

    /// <summary>
    /// Gets the duration of each segment in the timeline.
    /// </summary>
    /// <param name="timeline">The timeline to analyze.</param>
    /// <returns>
    /// An array of segment durations in milliseconds, or empty array if timeline has fewer than 2 keyframes.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeline"/> is null.</exception>
    public static double[] GetSegmentDurations(this TransitionTimeline timeline)
    {
        ArgumentNullException.ThrowIfNull(timeline);

        return timeline.GetSegments()
            .Select(segment => segment.To.TimeMs - segment.From.TimeMs)
            .ToArray();
    }

    /// <summary>
    /// Creates a new timeline that is a reversed copy of this timeline.
    /// The first keyframe becomes the last, and vice versa, with easing directions preserved.
    /// </summary>
    /// <param name="timeline">The timeline to reverse.</param>
    /// <returns>A new timeline instance with reversed keyframe order.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="timeline"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="timeline"/> has fewer than 2 keyframes.
    /// </exception>
    public static TransitionTimeline Reverse(
        this TransitionTimeline timeline)
    {
        ArgumentNullException.ThrowIfNull(timeline);

        if (timeline.Keyframes.Count < 2)
            throw new InvalidOperationException("Cannot reverse a timeline with fewer than 2 keyframes.");

        var reversedTimeline = new TransitionTimeline();
        reversedTimeline.Repeat(timeline.LoopCount);

        // Add keyframes in reverse order
        for (int i = timeline.Keyframes.Count - 1; i >= 0; i--)
        {
            var keyframe = timeline.Keyframes[i];
            reversedTimeline.AddKeyframe(keyframe.Chart, keyframe.TimeMs, keyframe.Easing);
        }

        return reversedTimeline;
    }
}