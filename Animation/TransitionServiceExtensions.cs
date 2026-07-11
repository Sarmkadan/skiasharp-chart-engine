// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using SkiaSharpChartEngine.Configuration;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Animation;

/// <summary>
/// Extension methods that wire the v2 animated-transitions subsystem into a
/// <see cref="IServiceCollection"/> and provide ergonomic shortcuts on
/// <see cref="Chart"/> for creating timelines inline.
/// </summary>
public static class TransitionServiceExtensions
{
    // ── DI registration ──────────────────────────────────────────────────────

    /// <summary>
    /// Registers <see cref="IChartTransitionEngine"/> and a singleton
    /// <see cref="TransitionOptions"/> with the dependency injection container.
    /// </summary>
    /// <remarks>
    /// The core chart engine services must already be registered before calling this method.
    /// Use <see cref="ServiceCollectionExtensions.AddSkiaSharpChartEngine"/> or
    /// <see cref="AddSkiaSharpChartEngineWithTransitions"/> to register both in one call.
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureOptions">
    /// Optional delegate to customise the default <see cref="TransitionOptions"/> instance
    /// that is injected throughout the application.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for further chaining.</returns>
    public static IServiceCollection AddChartTransitions(
        this IServiceCollection services,
        Action<TransitionOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var defaults = new TransitionOptions();
        configureOptions?.Invoke(defaults);

        services.AddSingleton(defaults);
        services.AddSingleton<IChartTransitionEngine, ChartTransitionEngine>();

        return services;
    }

    /// <summary>
    /// Registers the complete SkiaSharp Chart Engine together with the animated-transitions
    /// subsystem in a single call — a convenience wrapper around
    /// <see cref="ServiceCollectionExtensions.AddSkiaSharpChartEngine"/> and
    /// <see cref="AddChartTransitions"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureEngine">
    /// Optional delegate to customise <see cref="ChartEngineOptions"/>.
    /// </param>
    /// <param name="configureTransitions">
    /// Optional delegate to customise the default <see cref="TransitionOptions"/>.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for further chaining.</returns>
    public static IServiceCollection AddSkiaSharpChartEngineWithTransitions(
        this IServiceCollection services,
        Action<ChartEngineOptions>? configureEngine = null,
        Action<TransitionOptions>? configureTransitions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddSkiaSharpChartEngine(configureEngine)
            .AddChartTransitions(configureTransitions);
    }

    // ── Chart extension methods ───────────────────────────────────────────────

    /// <summary>
    /// Creates a new <see cref="TransitionTimeline"/> anchored to this chart at time zero.
    /// Equivalent to <c>new TransitionTimeline().AddKeyframe(chart, 0)</c>.
    /// </summary>
    /// <param name="chart">The opening chart state of the animation.</param>
    /// <returns>
    /// A new <see cref="TransitionTimeline"/> with this chart as its first keyframe.
    /// </returns>
    /// <example>
    /// <code>
    /// var timeline = chartA
    ///     .BeginTransition()
    ///     .AppendTransition(chartB, durationMs: 500, TransitionEasing.EaseOutBack)
    ///     .AppendTransition(chartC, durationMs: 400, TransitionEasing.Spring);
    /// </code>
    /// </example>
    public static TransitionTimeline BeginTransition(this Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);
        return new TransitionTimeline().AddKeyframe(chart, 0);
    }

    /// <summary>
    /// Creates a two-keyframe <see cref="TransitionTimeline"/> between this chart and
    /// <paramref name="to"/> — a shorthand for <see cref="TransitionTimeline.Between"/>.
    /// </summary>
    /// <param name="from">The starting chart state.</param>
    /// <param name="to">The ending chart state.</param>
    /// <param name="durationMs">Total transition duration in milliseconds. Must be positive.</param>
    /// <param name="easing">Easing applied to the transition.</param>
    /// <returns>A new two-keyframe <see cref="TransitionTimeline"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="from"/> or <paramref name="to"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="durationMs"/> is less than or equal to zero.
    /// </exception>
    public static TransitionTimeline TransitionTo(
        this Chart from,
        Chart to,
        double durationMs,
        TransitionEasing easing = TransitionEasing.EaseInOutCubic)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(durationMs, 0);

        return TransitionTimeline.Between(from, to, durationMs, easing);
    }

    // ── AnimationSettings bridge ───────────────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="TransitionOptions"/> whose frame rate mirrors the supplied
    /// <see cref="AnimationSettings"/>, bridging the v1 animation API with the v2
    /// transition engine.
    /// </summary>
    /// <param name="settings">Source animation settings.</param>
    /// <returns>
    /// A new <see cref="TransitionOptions"/> initialised from <paramref name="settings"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="settings"/> is <see langword="null"/>.
    /// </exception>
    public static TransitionOptions ToTransitionOptions(this AnimationSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return new TransitionOptions
        {
            // Clamp: AnimationSettings.FrameRate is validated ≥ 1; TransitionOptions caps at 120.
            FrameRate = Math.Clamp(settings.FrameRate, 1, 120),
        };
    }

    /// <summary>
    /// Creates a <see cref="TransitionTimeline"/> driven by the duration configured in the
    /// chart's <see cref="ChartConfiguration.AnimationDurationMs"/>, transitioning to
    /// <paramref name="to"/> with the default cubic easing.
    /// </summary>
    /// <param name="from">
    /// The starting chart; its <see cref="ChartConfiguration.AnimationDurationMs"/> sets
    /// the segment duration.
    /// </param>
    /// <param name="to">The ending chart state.</param>
    /// <param name="easing">
    /// Easing for the single segment. Defaults to <see cref="TransitionEasing.EaseInOutCubic"/>.
    /// </param>
    /// <returns>A configured two-keyframe <see cref="TransitionTimeline"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="from"/> or <paramref name="to"/> is <see langword="null"/>.
    /// </exception>
    public static TransitionTimeline ToAnimatedTimeline(
        this Chart from,
        Chart to,
        TransitionEasing easing = TransitionEasing.EaseInOutCubic)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        double durationMs = from.Configuration.AnimationDurationMs > 0
            ? from.Configuration.AnimationDurationMs
            : 500.0;

        return TransitionTimeline.Between(from, to, durationMs, easing);
    }
}