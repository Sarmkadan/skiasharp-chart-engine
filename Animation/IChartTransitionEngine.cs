// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Animation;

/// <summary>
/// Contract for the v2 animated-transitions engine.
/// </summary>
/// <remarks>
/// <para>
/// Implementations are responsible for the complete animation pipeline:
/// timeline validation → per-frame time resolution → chart-state interpolation →
/// SkiaSharp rendering → frame assembly → result packaging.
/// </para>
/// <para>
/// Register the default implementation via
/// <see cref="TransitionServiceExtensions.AddChartTransitions"/> and resolve
/// <see cref="IChartTransitionEngine"/> from the DI container.
/// </para>
/// </remarks>
public interface IChartTransitionEngine
{
    /// <summary>
    /// Renders a complete transition sequence described by <paramref name="timeline"/>.
    /// Each frame is an interpolated chart state encoded as PNG bytes.
    /// </summary>
    /// <param name="timeline">
    /// The ordered sequence of chart keyframes to animate between.
    /// Must contain at least two keyframes with strictly ascending time offsets.
    /// </param>
    /// <param name="options">
    /// Render options controlling frame rate, quality, and playback behaviour.
    /// When <see langword="null"/>, a default <see cref="TransitionOptions"/> is used.
    /// </param>
    /// <param name="cancellationToken">
    /// Propagates a cancellation signal across all async rendering operations.
    /// </param>
    /// <returns>
    /// A <see cref="TransitionResult"/> whose <see cref="TransitionResult.Frames"/> collection
    /// contains PNG-encoded frames in chronological order.
    /// On cancellation, <see cref="TransitionResult.Success"/> is <see langword="false"/>
    /// and no partial frame list is returned.
    /// </returns>
    Task<TransitionResult> RenderTransitionAsync(
        TransitionTimeline timeline,
        TransitionOptions? options            = null,
        CancellationToken  cancellationToken  = default);

    /// <summary>
    /// Convenience overload that builds a simple two-keyframe
    /// <see cref="TransitionTimeline"/> internally and renders it.
    /// </summary>
    /// <param name="from">Initial chart state at time zero.</param>
    /// <param name="to">Final chart state at the end of the animation.</param>
    /// <param name="settings">
    /// Animation settings (duration in ms, frame rate, easing type) sourced from the chart
    /// configuration. When <see langword="null"/>, default settings of 500 ms at 60 fps with
    /// <see cref="EasingFunction.EaseInOutQuad"/> are used.
    /// </param>
    /// <param name="options">
    /// Additional render options. When <see langword="null"/>, defaults are applied.
    /// </param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    Task<TransitionResult> RenderTransitionAsync(
        Chart              from,
        Chart              to,
        AnimationSettings? settings           = null,
        TransitionOptions? options            = null,
        CancellationToken  cancellationToken  = default);

    /// <summary>
    /// Renders a single interpolated frame between two chart states without constructing a
    /// full timeline. Suitable for scrubbing operations, thumbnail previews, or one-shot
    /// snapshots at a specific point in an animation.
    /// </summary>
    /// <param name="from">Start chart state (progress = 0).</param>
    /// <param name="to">End chart state (progress = 1).</param>
    /// <param name="progress">
    /// Linear progress in [0, 1] <em>before</em> the easing function is applied.
    /// </param>
    /// <param name="easing">Easing function to apply to <paramref name="progress"/>.</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>PNG-encoded image bytes for the interpolated frame.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the underlying rendering service fails to produce image data.
    /// </exception>
    Task<byte[]> RenderFrameAsync(
        Chart            from,
        Chart            to,
        double           progress,
        TransitionEasing easing             = TransitionEasing.EaseInOutCubic,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new, empty <see cref="TransitionTimeline"/> ready to be populated
    /// with keyframes via the fluent API.
    /// </summary>
    /// <returns>A new <see cref="TransitionTimeline"/> instance with no keyframes.</returns>
    TransitionTimeline CreateTimeline();
}
