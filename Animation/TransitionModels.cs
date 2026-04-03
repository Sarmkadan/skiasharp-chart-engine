// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Animation;

// ── Easing catalogue ──────────────────────────────────────────────────────────

/// <summary>
/// Extended easing catalogue for chart transitions. Covers all CSS easing presets plus
/// spring-physics and bounce variants not present in <see cref="Models.EasingFunction"/>.
/// </summary>
public enum TransitionEasing
{
    /// <summary>Constant velocity throughout.</summary>
    Linear = 0,
    /// <summary>Gradual acceleration.</summary>
    EaseInQuad = 1,
    /// <summary>Gradual deceleration.</summary>
    EaseOutQuad = 2,
    /// <summary>Accelerate then decelerate.</summary>
    EaseInOutQuad = 3,
    /// <summary>Strong cubic acceleration.</summary>
    EaseInCubic = 4,
    /// <summary>Strong cubic deceleration.</summary>
    EaseOutCubic = 5,
    /// <summary>Strong cubic acceleration then deceleration.</summary>
    EaseInOutCubic = 6,
    /// <summary>Exponential acceleration.</summary>
    EaseInExpo = 7,
    /// <summary>Exponential deceleration.</summary>
    EaseOutExpo = 8,
    /// <summary>Exponential acceleration and deceleration.</summary>
    EaseInOutExpo = 9,
    /// <summary>Overshoots slightly before settling — back-ease in.</summary>
    EaseInBack = 10,
    /// <summary>Reaches the target then overshoots slightly before settling — back-ease out.</summary>
    EaseOutBack = 11,
    /// <summary>Overshoot on both ends.</summary>
    EaseInOutBack = 12,
    /// <summary>Elastic spring snap inward.</summary>
    EaseInElastic = 13,
    /// <summary>Elastic spring snap outward.</summary>
    EaseOutElastic = 14,
    /// <summary>Elastic spring snap on both ends.</summary>
    EaseInOutElastic = 15,
    /// <summary>Simulates a ball bouncing to rest (decelerating).</summary>
    EaseOutBounce = 16,
    /// <summary>Simulates a ball bouncing from rest (accelerating).</summary>
    EaseInBounce = 17,
    /// <summary>Bounce on both ends.</summary>
    EaseInOutBounce = 18,
    /// <summary>
    /// Physically-accurate damped spring (stiffness 200, damping 20).
    /// May produce values slightly outside [0, 1] due to oscillation overshoot —
    /// this is intentional and creates a natural spring feel in data values.
    /// </summary>
    Spring = 19,
}

/// <summary>
/// Evaluates <see cref="TransitionEasing"/> functions for a normalised linear input
/// <c>t ∈ [0, 1]</c>. The return value is clamped to [0, 1] for all easing variants
/// except <see cref="TransitionEasing.Spring"/>, which may transiently exceed those bounds.
/// </summary>
public static class TransitionEasingCalculator
{
    private const double BackOvershoot = 1.70158;

    /// <summary>Returns the eased value for linear progress <paramref name="t"/> in [0, 1].</summary>
    /// <param name="easing">The easing function to apply.</param>
    /// <param name="t">Linear progress in [0, 1].</param>
    public static double Calculate(TransitionEasing easing, double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);

        return easing switch
        {
            TransitionEasing.Linear           => t,
            TransitionEasing.EaseInQuad       => t * t,
            TransitionEasing.EaseOutQuad      => t * (2 - t),
            TransitionEasing.EaseInOutQuad    => t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t,
            TransitionEasing.EaseInCubic      => t * t * t,
            TransitionEasing.EaseOutCubic     => 1 + Math.Pow(t - 1, 3),
            TransitionEasing.EaseInOutCubic   => t < 0.5 ? 4 * t * t * t : 1 + Math.Pow(2 * t - 2, 3) / 2,
            TransitionEasing.EaseInExpo       => t == 0.0 ? 0.0 : Math.Pow(2, 10 * t - 10),
            TransitionEasing.EaseOutExpo      => t == 1.0 ? 1.0 : 1 - Math.Pow(2, -10 * t),
            TransitionEasing.EaseInOutExpo    => EaseInOutExpo(t),
            TransitionEasing.EaseInBack       => t * t * ((BackOvershoot + 1) * t - BackOvershoot),
            TransitionEasing.EaseOutBack      => EaseOutBack(t),
            TransitionEasing.EaseInOutBack    => EaseInOutBack(t),
            TransitionEasing.EaseInElastic    => EaseInElastic(t),
            TransitionEasing.EaseOutElastic   => EaseOutElastic(t),
            TransitionEasing.EaseInOutElastic => EaseInOutElastic(t),
            TransitionEasing.EaseOutBounce    => EaseOutBounce(t),
            TransitionEasing.EaseInBounce     => 1 - EaseOutBounce(1 - t),
            TransitionEasing.EaseInOutBounce  => t < 0.5
                                                     ? (1 - EaseOutBounce(1 - 2 * t)) / 2
                                                     : (1 + EaseOutBounce(2 * t - 1)) / 2,
            TransitionEasing.Spring           => Spring(t),
            _                                 => t,
        };
    }

    private static double EaseInOutExpo(double t)
    {
        if (t == 0.0) return 0.0;
        if (t == 1.0) return 1.0;
        return t < 0.5
            ? Math.Pow(2, 20 * t - 10) / 2
            : (2 - Math.Pow(2, -20 * t + 10)) / 2;
    }

    private static double EaseOutBack(double t)
    {
        double c = BackOvershoot + 1;
        return 1 + c * Math.Pow(t - 1, 3) + BackOvershoot * Math.Pow(t - 1, 2);
    }

    private static double EaseInOutBack(double t)
    {
        double c = BackOvershoot * 1.525;
        return t < 0.5
            ? Math.Pow(2 * t, 2) * ((c + 1) * 2 * t - c) / 2
            : (Math.Pow(2 * t - 2, 2) * ((c + 1) * (2 * t - 2) + c) + 2) / 2;
    }

    private static double EaseInElastic(double t)
    {
        if (t == 0.0) return 0.0;
        if (t == 1.0) return 1.0;
        double c = 2 * Math.PI / 3;
        return -Math.Pow(2, 10 * t - 10) * Math.Sin((t * 10 - 10.75) * c);
    }

    private static double EaseOutElastic(double t)
    {
        if (t == 0.0) return 0.0;
        if (t == 1.0) return 1.0;
        double c = 2 * Math.PI / 3;
        return Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * c) + 1;
    }

    private static double EaseInOutElastic(double t)
    {
        if (t == 0.0) return 0.0;
        if (t == 1.0) return 1.0;
        double c = 2 * Math.PI / 4.5;
        return t < 0.5
            ? -(Math.Pow(2, 20 * t - 10) * Math.Sin((20 * t - 11.125) * c)) / 2
            : (Math.Pow(2, -20 * t + 10) * Math.Sin((20 * t - 11.125) * c)) / 2 + 1;
    }

    private static double EaseOutBounce(double t)
    {
        const double n1 = 7.5625, d1 = 2.75;
        if (t < 1 / d1)       return n1 * t * t;
        if (t < 2 / d1)       return n1 * (t -= 1.5 / d1)   * t + 0.75;
        if (t < 2.5 / d1)     return n1 * (t -= 2.25 / d1)  * t + 0.9375;
        return n1 * (t -= 2.625 / d1) * t + 0.984375;
    }

    // Under-damped spring: stiffness=200, damping=20, mass=1 → ζ≈0.707
    private static double Spring(double t)
    {
        const double stiffness = 200.0, damping = 20.0, mass = 1.0;
        double w0   = Math.Sqrt(stiffness / mass);
        double zeta = damping / (2 * Math.Sqrt(stiffness * mass));
        if (zeta >= 1.0)
            return 1 - Math.Exp(-w0 * t) * (1 + w0 * t);
        double wd = w0 * Math.Sqrt(1 - zeta * zeta);
        return 1 - Math.Exp(-zeta * w0 * t) * (Math.Cos(wd * t) + zeta * w0 / wd * Math.Sin(wd * t));
    }
}

// ── DTOs ──────────────────────────────────────────────────────────────────────

/// <summary>
/// An immutable snapshot of a chart state at a specific point along an animation timeline.
/// </summary>
/// <param name="Chart">The chart state captured at this keyframe.</param>
/// <param name="TimeMs">
/// Absolute time offset in milliseconds from the start of the timeline.
/// Must be non-negative; keyframes are ordered by this value.
/// </param>
/// <param name="Easing">
/// Easing function applied when transitioning <em>from</em> this keyframe to the next.
/// The easing of the final keyframe is not used.
/// </param>
public sealed record TransitionKeyframe(
    Chart Chart,
    double TimeMs,
    TransitionEasing Easing = TransitionEasing.EaseInOutCubic);

/// <summary>
/// A single PNG-encoded image frame produced during an animation rendering pass.
/// </summary>
/// <param name="FrameIndex">Zero-based position of this frame within the full sequence.</param>
/// <param name="TimeMs">
/// Timestamp of this frame in milliseconds, measured from the start of the animation.
/// </param>
/// <param name="Progress">
/// Overall normalised progress [0, 1] of the full animation at this frame.
/// </param>
/// <param name="ImageData">Raw PNG bytes for this frame, as returned by the rendering service.</param>
public sealed record TransitionRenderFrame(
    int    FrameIndex,
    double TimeMs,
    double Progress,
    byte[] ImageData);

/// <summary>
/// Encapsulates the outcome of rendering a complete animation transition sequence.
/// </summary>
public sealed class TransitionResult
{
    /// <summary>Gets the chart identifier for which this transition was rendered.</summary>
    public required string ChartId { get; init; }

    /// <summary>Gets whether all frames were rendered without error.</summary>
    public bool Success { get; init; }

    /// <summary>Gets the rendered frames in chronological order.</summary>
    public IReadOnlyList<TransitionRenderFrame> Frames { get; init; }
        = Array.Empty<TransitionRenderFrame>();

    /// <summary>Gets the full animation duration in milliseconds, taken from the timeline.</summary>
    public double TotalDurationMs { get; init; }

    /// <summary>Gets the wall-clock time taken to render all frames, in milliseconds.</summary>
    public long RenderTimeMs { get; init; }

    /// <summary>
    /// Gets a human-readable error description when <see cref="Success"/> is
    /// <see langword="false"/>.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Gets the exception that caused the failure, if any.</summary>
    public Exception? Exception { get; init; }

    /// <summary>Gets arbitrary key-value metadata produced during rendering.</summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; }
        = new Dictionary<string, object>();

    /// <summary>Creates a successful result containing all rendered frames.</summary>
    public static TransitionResult CreateSuccess(
        string                              chartId,
        IReadOnlyList<TransitionRenderFrame> frames,
        double                              totalDurationMs,
        long                                renderTimeMs,
        IReadOnlyDictionary<string, object>? metadata = null) => new()
    {
        ChartId         = chartId,
        Success         = true,
        Frames          = frames,
        TotalDurationMs = totalDurationMs,
        RenderTimeMs    = renderTimeMs,
        Metadata        = metadata ?? new Dictionary<string, object>(),
    };

    /// <summary>Creates a failure result with the supplied error information.</summary>
    public static TransitionResult CreateFailure(
        string     chartId,
        string     errorMessage,
        Exception? exception = null) => new()
    {
        ChartId      = chartId,
        Success      = false,
        ErrorMessage = errorMessage,
        Exception    = exception,
    };

    /// <inheritdoc/>
    public override string ToString()
        => $"TransitionResult(ChartId={ChartId}, Success={Success}, " +
           $"Frames={Frames.Count}, Duration={TotalDurationMs}ms)";
}
