// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Middleware;

/// <summary>
/// Token bucket based rate limiting middleware
/// Prevents abuse by limiting requests per client
/// </summary>
public class RateLimitingMiddleware
{
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitPolicy _policy;
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets;
    private readonly Timer _cleanupTimer;

    public RateLimitingMiddleware(ILogger<RateLimitingMiddleware> logger, RateLimitPolicy? policy = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _policy = policy ?? new RateLimitPolicy();
        _buckets = new ConcurrentDictionary<string, TokenBucket>();

        // Cleanup expired buckets every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredBuckets, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    /// <summary>
    /// Checks if a request from the given client should be rate limited
    /// Returns true if request is allowed, false if rate limit exceeded
    /// </summary>
    public bool AllowRequest(string clientIdentifier, out RateLimitInfo? rateLimitInfo)
    {
        rateLimitInfo = null;

        if (string.IsNullOrEmpty(clientIdentifier))
        {
            clientIdentifier = "anonymous";
        }

        try
        {
            var bucket = _buckets.GetOrAdd(clientIdentifier, _ => new TokenBucket(_policy));

            if (bucket.TryConsumeToken(out var info))
            {
                rateLimitInfo = info;
                return true;
            }

            rateLimitInfo = info;
            _logger.LogWarning("Rate limit exceeded for client {ClientId}. Tokens: {Tokens}/{Capacity}",
                clientIdentifier, info?.AvailableTokens ?? 0, _policy.MaxTokens);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for client {ClientId}", clientIdentifier);
            // Fail open: allow request if rate limiting fails
            return true;
        }
    }

    /// <summary>
    /// Gets rate limit information for a client
    /// </summary>
    public RateLimitInfo? GetRateLimitInfo(string clientIdentifier)
    {
        if (string.IsNullOrEmpty(clientIdentifier))
            clientIdentifier = "anonymous";

        if (_buckets.TryGetValue(clientIdentifier, out var bucket))
        {
            return bucket.GetCurrentInfo();
        }

        return null;
    }

    /// <summary>
    /// Manually clears rate limit for a client (useful for testing or admin operations)
    /// </summary>
    public void ResetClientLimit(string clientIdentifier)
    {
        _buckets.TryRemove(clientIdentifier, out _);
        _logger.LogInformation("Rate limit reset for client {ClientId}", clientIdentifier);
    }

    private void CleanupExpiredBuckets(object? state)
    {
        try
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _buckets
                .Where(kvp => kvp.Value.IsExpired(now))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _buckets.TryRemove(key, out _);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogDebug("Cleaned up {Count} expired rate limit buckets", expiredKeys.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired rate limit buckets");
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

/// <summary>
/// Token bucket implementation for rate limiting
/// Uses sliding window approach for fair distribution
/// </summary>
internal class TokenBucket
{
    private readonly RateLimitPolicy _policy;
    private long _availableTokens;
    private DateTime _lastRefillTime;
    private readonly object _lock = new();

    public TokenBucket(RateLimitPolicy policy)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        _availableTokens = policy.MaxTokens;
        _lastRefillTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Attempts to consume a token, refilling if necessary
    /// </summary>
    public bool TryConsumeToken(out RateLimitInfo info)
    {
        lock (_lock)
        {
            RefillTokens();

            info = new RateLimitInfo
            {
                AvailableTokens = _availableTokens,
                MaxTokens = _policy.MaxTokens,
                ResetTime = _lastRefillTime.AddSeconds(_policy.RefillIntervalSeconds)
            };

            if (_availableTokens > 0)
            {
                _availableTokens--;
                info.AvailableTokens = _availableTokens;
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Gets current bucket information without consuming tokens
    /// </summary>
    public RateLimitInfo GetCurrentInfo()
    {
        lock (_lock)
        {
            RefillTokens();

            return new RateLimitInfo
            {
                AvailableTokens = _availableTokens,
                MaxTokens = _policy.MaxTokens,
                ResetTime = _lastRefillTime.AddSeconds(_policy.RefillIntervalSeconds)
            };
        }
    }

    /// <summary>
    /// Checks if bucket is expired (no activity for extended period)
    /// </summary>
    public bool IsExpired(DateTime now)
    {
        return (now - _lastRefillTime).TotalMinutes > 60;
    }

    private void RefillTokens()
    {
        var now = DateTime.UtcNow;
        var elapsed = (now - _lastRefillTime).TotalSeconds;

        if (elapsed >= _policy.RefillIntervalSeconds)
        {
            _availableTokens = _policy.MaxTokens;
            _lastRefillTime = now;
        }
    }
}

/// <summary>
/// Rate limiting policy configuration
/// </summary>
public class RateLimitPolicy
{
    /// <summary>
    /// Maximum number of tokens in the bucket
    /// </summary>
    public long MaxTokens { get; set; } = 100;

    /// <summary>
    /// Interval in seconds for token refill
    /// </summary>
    public int RefillIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Custom identifier extraction function
    /// </summary>
    public Func<string>? CustomIdentifierExtractor { get; set; }
}

/// <summary>
/// Information about current rate limit status
/// </summary>
public class RateLimitInfo
{
    public long AvailableTokens { get; set; }
    public long MaxTokens { get; set; }
    public DateTime ResetTime { get; set; }

    /// <summary>
    /// Gets seconds until rate limit resets
    /// </summary>
    public int SecondsUntilReset => Math.Max(0, (int)(ResetTime - DateTime.UtcNow).TotalSeconds);
}
