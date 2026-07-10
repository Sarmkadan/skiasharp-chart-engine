// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Middleware;

/// <summary>
/// Extension methods for RateLimitingMiddleware to provide additional functionality
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Creates a new RateLimitingMiddleware instance with the specified logger and policy
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="policy">Optional rate limit policy configuration</param>
    /// <returns>A new RateLimitingMiddleware instance</returns>
    public static RateLimitingMiddleware WithLogger(this ILogger<RateLimitingMiddleware> logger, RateLimitPolicy? policy = null)
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        return new RateLimitingMiddleware(logger, policy);
    }

    /// <summary>
    /// Creates a new RateLimitingMiddleware instance with a default console logger
    /// </summary>
    /// <param name="policy">Optional rate limit policy configuration</param>
    /// <returns>A new RateLimitingMiddleware instance with console logging</returns>
    public static RateLimitingMiddleware WithConsoleLogger(RateLimitPolicy? policy = null)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        var logger = loggerFactory.CreateLogger<RateLimitingMiddleware>();
        return new RateLimitingMiddleware(logger, policy);
    }

    /// <summary>
    /// Creates a new RateLimitingMiddleware instance with a null logger (no logging)
    /// </summary>
    /// <param name="policy">Optional rate limit policy configuration</param>
    /// <returns>A new RateLimitingMiddleware instance with no logging</returns>
    public static RateLimitingMiddleware WithNoLogging(RateLimitPolicy? policy = null)
    {
        var logger = new NullLogger<RateLimitingMiddleware>();
        return new RateLimitingMiddleware(logger, policy);
    }

    /// <summary>
    /// Gets the current rate limit information for a client without consuming tokens
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="clientIdentifier">The client identifier</param>
    /// <returns>RateLimitInfo if client exists, null otherwise</returns>
    public static RateLimitInfo? GetCurrentRateLimitInfo(this RateLimitingMiddleware middleware, string clientIdentifier)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        return middleware.GetRateLimitInfo(clientIdentifier);
    }

    /// <summary>
    /// Checks if a request from the given client should be rate limited
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="clientIdentifier">The client identifier</param>
    /// <param name="rateLimitInfo">Output parameter containing rate limit information</param>
    /// <returns>True if request is allowed, false if rate limit exceeded</returns>
    public static bool CheckRateLimit(this RateLimitingMiddleware middleware, string clientIdentifier, out RateLimitInfo? rateLimitInfo)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        return middleware.AllowRequest(clientIdentifier, out rateLimitInfo);
    }

    /// <summary>
    /// Resets the rate limit for a specific client
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="clientIdentifier">The client identifier to reset</param>
    public static void ResetClient(this RateLimitingMiddleware middleware, string clientIdentifier)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        middleware.ResetClientLimit(clientIdentifier);
    }
}

/// <summary>
/// Null logger implementation for cases where no logging is desired
/// </summary>
/// <typeparam name="T">Logger type</typeparam>
internal sealed class NullLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => false;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // No logging
    }
}