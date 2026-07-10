// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;

namespace SkiaSharpChartEngine.Caching;

/// <summary>
/// Extension methods for CachePolicy providing convenient helper methods
/// </summary>
public static class CachePolicyExtensions
{
    /// <summary>
    /// Sets the cache priority for this policy
    /// </summary>
    /// <param name="policy">The cache policy to configure</param>
    /// <param name="priority">The priority level</param>
    /// <returns>The configured cache policy for method chaining</returns>
    public static CachePolicy WithPriority(this CachePolicy policy, CachePriority priority)
    {
        if (policy == null)
            throw new ArgumentNullException(nameof(policy));

        policy.Priority = priority;
        return policy;
    }

    /// <summary>
    /// Sets a post-eviction callback for this policy
    /// </summary>
    /// <param name="policy">The cache policy to configure</param>
    /// <param name="callback">The callback to invoke when cache item is evicted</param>
    /// <returns>The configured cache policy for method chaining</returns>
    public static CachePolicy WithPostEvictionCallback(this CachePolicy policy, Action<string> callback)
    {
        if (policy == null)
            throw new ArgumentNullException(nameof(policy));

        policy.PostEvictionCallbacks = callback;
        return policy;
    }

    /// <summary>
    /// Sets both absolute and sliding expiration times
    /// </summary>
    /// <param name="policy">The cache policy to configure</param>
    /// <param name="absoluteExpiration">Absolute expiration time span</param>
    /// <param name="slidingExpiration">Sliding expiration time span</param>
    /// <returns>The configured cache policy for method chaining</returns>
    public static CachePolicy WithExpiration(this CachePolicy policy, TimeSpan? absoluteExpiration, TimeSpan? slidingExpiration)
    {
        if (policy == null)
            throw new ArgumentNullException(nameof(policy));

        policy.AbsoluteExpirationRelativeToNow = absoluteExpiration;
        policy.SlidingExpiration = slidingExpiration;
        return policy;
    }

    /// <summary>
    /// Clamps the sliding expiration to be no more than 80% of absolute expiration
    /// to prevent premature eviction
    /// </summary>
    /// <param name="policy">The cache policy to validate and adjust</param>
    /// <returns>The policy with clamped sliding expiration</returns>
    public static CachePolicy EnsureValidSlidingExpiration(this CachePolicy policy)
    {
        if (policy == null)
            throw new ArgumentNullException(nameof(policy));

        if (policy.AbsoluteExpirationRelativeToNow.HasValue && policy.SlidingExpiration.HasValue)
        {
            var absolute = policy.AbsoluteExpirationRelativeToNow.Value;
            var sliding = policy.SlidingExpiration.Value;

            // Ensure sliding expiration is not greater than absolute expiration
            if (sliding > absolute)
            {
                policy.SlidingExpiration = TimeSpan.FromTicks((long)(absolute.Ticks * 0.8));
            }
        }

        return policy;
    }
}