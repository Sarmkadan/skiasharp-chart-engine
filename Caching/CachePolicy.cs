// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SkiaSharpChartEngine.Caching;

/// <summary>
/// Defines caching policy and expiration settings
/// Allows fine-grained control over cache behavior
/// </summary>
public class CachePolicy
{
    /// <summary>
    /// Default cache policy with reasonable defaults
    /// </summary>
    public static readonly CachePolicy Default = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };

    /// <summary>
    /// Short-lived cache policy for frequently accessed data
    /// </summary>
    public static readonly CachePolicy ShortLived = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
        SlidingExpiration = TimeSpan.FromMinutes(5)
    };

    /// <summary>
    /// Long-lived cache policy for stable data
    /// </summary>
    public static readonly CachePolicy LongLived = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7),
        SlidingExpiration = TimeSpan.FromDays(1)
    };

    /// <summary>
    /// No expiration - cache until explicitly cleared
    /// </summary>
    public static readonly CachePolicy NoExpiration = new()
    {
        AbsoluteExpirationRelativeToNow = null,
        SlidingExpiration = null
    };

    /// <summary>
    /// Time after which cache entry expires absolutely
    /// </summary>
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    /// <summary>
    /// Time period after which unused cache expires
    /// </summary>
    public TimeSpan? SlidingExpiration { get; set; }

    /// <summary>
    /// Whether to prioritize the cache item (for memory pressure situations)
    /// </summary>
    public CachePriority Priority { get; set; } = CachePriority.Normal;

    /// <summary>
    /// Optional callback when cache item is evicted
    /// </summary>
    public Action<string>? PostEvictionCallbacks { get; set; }

    /// <summary>
    /// Gets the absolute expiration time based on current time
    /// </summary>
    public DateTime? GetAbsoluteExpiration()
    {
        if (AbsoluteExpirationRelativeToNow.HasValue)
        {
            return DateTime.UtcNow.Add(AbsoluteExpirationRelativeToNow.Value);
        }

        return null;
    }

    /// <summary>
    /// Validates cache policy is reasonable
    /// </summary>
    public bool IsValid()
    {
        if (AbsoluteExpirationRelativeToNow < TimeSpan.Zero)
            return false;

        if (SlidingExpiration < TimeSpan.Zero)
            return false;

        if (AbsoluteExpirationRelativeToNow.HasValue && SlidingExpiration.HasValue)
        {
            if (SlidingExpiration > AbsoluteExpirationRelativeToNow)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Creates a policy with custom expiration times
    /// </summary>
    public static CachePolicy Create(TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        return new CachePolicy
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration,
            SlidingExpiration = slidingExpiration
        };
    }

    /// <summary>
    /// Creates a policy that expires after specified minutes
    /// </summary>
    public static CachePolicy ExpiresAfterMinutes(int minutes)
    {
        return new CachePolicy
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes),
            SlidingExpiration = TimeSpan.FromMinutes(Math.Max(1, minutes / 6))
        };
    }

    /// <summary>
    /// Creates a policy that expires after specified hours
    /// </summary>
    public static CachePolicy ExpiresAfterHours(int hours)
    {
        return new CachePolicy
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(hours),
            SlidingExpiration = TimeSpan.FromMinutes(Math.Max(10, hours * 5))
        };
    }
}

/// <summary>
/// Cache priority level for eviction under memory pressure
/// </summary>
public enum CachePriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Cache entry metadata
/// </summary>
public class CacheEntryMetadata
{
    public string Key { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public int AccessCount { get; set; }
    public long? SizeInBytes { get; set; }
    public CachePriority Priority { get; set; }
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Gets how long the entry has been in cache
    /// </summary>
    public TimeSpan GetAge() => DateTime.UtcNow - CreatedAt;

    /// <summary>
    /// Gets time until expiration
    /// </summary>
    public TimeSpan? GetTimeToExpiration() =>
        ExpiresAt.HasValue ? ExpiresAt.Value - DateTime.UtcNow : null;

    /// <summary>
    /// Checks if entry has expired
    /// </summary>
    public bool IsExpired() => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}
