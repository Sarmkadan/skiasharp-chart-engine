// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;

namespace SkiaSharpChartEngine.Caching;

/// <summary>
/// Extension methods for <see cref="CachePolicy"/> providing convenient helper methods
/// for configuring cache expiration and behavior.
/// </summary>
public static class CachePolicyExtensions
{
    /// <summary>
    /// Sets the cache priority for this policy.
    /// </summary>
    /// <param name="policy">The cache policy to configure. Cannot be <see langword="null"/>.</param>
    /// <param name="priority">The priority level to set.</param>
    /// <returns>The configured cache policy for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="policy"/> is <see langword="null"/>.</exception>
    public static CachePolicy WithPriority(this CachePolicy policy, CachePriority priority)
    {
        ArgumentNullException.ThrowIfNull(policy);

        policy.Priority = priority;
        return policy;
    }

    /// <summary>
    /// Sets a post-eviction callback for this policy.
    /// </summary>
    /// <param name="policy">The cache policy to configure. Cannot be <see langword="null"/>.</param>
    /// <param name="callback">The callback to invoke when a cache item is evicted. Can be <see langword="null"/> to clear any existing callback.</param>
    /// <returns>The configured cache policy for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="policy"/> is <see langword="null"/>.</exception>
    public static CachePolicy WithPostEvictionCallback(this CachePolicy policy, Action<string>? callback)
    {
        ArgumentNullException.ThrowIfNull(policy);

        policy.PostEvictionCallbacks = callback;
        return policy;
    }

    /// <summary>
    /// Sets both absolute and sliding expiration times.
    /// </summary>
    /// <param name="policy">The cache policy to configure. Cannot be <see langword="null"/>.</param>
    /// <param name="absoluteExpiration">Absolute expiration time span. Can be <see langword="null"/> for no absolute expiration.</param>
    /// <param name="slidingExpiration">Sliding expiration time span. Can be <see langword="null"/> for no sliding expiration.</param>
    /// <returns>The configured cache policy for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="policy"/> is <see langword="null"/>.</exception>
    public static CachePolicy WithExpiration(this CachePolicy policy, TimeSpan? absoluteExpiration, TimeSpan? slidingExpiration)
    {
        ArgumentNullException.ThrowIfNull(policy);

        policy.AbsoluteExpirationRelativeToNow = absoluteExpiration;
        policy.SlidingExpiration = slidingExpiration;
        return policy;
    }

    /// <summary>
    /// Ensures the sliding expiration is valid relative to absolute expiration.
    /// If sliding expiration exceeds absolute expiration, it is clamped to 80% of absolute expiration
    /// to prevent premature eviction and ensure cache entries remain valid for their intended duration.
    /// </summary>
    /// <param name="policy">The cache policy to validate and adjust. Cannot be <see langword="null"/>.</param>
    /// <returns>The policy with clamped sliding expiration.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="policy"/> is <see langword="null"/>.</exception>
    public static CachePolicy EnsureValidSlidingExpiration(this CachePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (policy.AbsoluteExpirationRelativeToNow.HasValue && policy.SlidingExpiration.HasValue)
        {
            var absolute = policy.AbsoluteExpirationRelativeToNow.Value;
            var sliding = policy.SlidingExpiration.Value;

            // Ensure sliding expiration is not greater than absolute expiration
            // Clamp to 80% of absolute expiration to maintain reasonable cache retention
            if (sliding > absolute)
            {
                policy.SlidingExpiration = TimeSpan.FromTicks((long)(absolute.Ticks * 0.8));
            }
        }

        return policy;
    }
}