// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

using System;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Middleware;

/// <summary>
/// Extension methods for <see cref="RateLimitingMiddleware"/> to provide additional functionality
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Creates a new <see cref="RateLimitingMiddleware"/> instance with the specified logger and policy
    /// </summary>
    /// <param name="logger">The logger instance. Cannot be null.</param>
    /// <param name="policy">Optional rate limit policy configuration</param>
    /// <returns>A new <see cref="RateLimitingMiddleware"/> instance</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static RateLimitingMiddleware WithLogger(this ILogger<RateLimitingMiddleware> logger, RateLimitPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        return new RateLimitingMiddleware(logger, policy);
    }

    /// <summary>
    /// Creates a new <see cref="RateLimitingMiddleware"/> instance with a default console logger
    /// </summary>
    /// <param name="policy">Optional rate limit policy configuration</param>
    /// <returns>A new <see cref="RateLimitingMiddleware"/> instance with console logging</returns>
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
    /// Creates a new <see cref="RateLimitingMiddleware"/> instance with a null logger (no logging)
    /// </summary>
    /// <param name="policy">Optional rate limit policy configuration</param>
    /// <returns>A new <see cref="RateLimitingMiddleware"/> instance with no logging</returns>
    public static RateLimitingMiddleware WithNoLogging(RateLimitPolicy? policy = null)
    {
        var logger = new NullLogger<RateLimitingMiddleware>();
        return new RateLimitingMiddleware(logger, policy);
    }

    /// <summary>
    /// Gets the current rate limit information for a client without consuming tokens
    /// </summary>
    /// <param name="middleware">The middleware instance. Cannot be null.</param>
    /// <param name="clientIdentifier">The client identifier. If null or empty, treated as "anonymous".</param>
    /// <returns><see cref="RateLimitInfo"/> if client exists, null otherwise</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    public static RateLimitInfo? GetCurrentRateLimitInfo(this RateLimitingMiddleware middleware, string? clientIdentifier)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        return middleware.GetRateLimitInfo(clientIdentifier);
    }

    /// <summary>
    /// Checks if a request from the given client should be rate limited
    /// </summary>
    /// <param name="middleware">The middleware instance. Cannot be null.</param>
    /// <param name="clientIdentifier">The client identifier. If null or empty, treated as "anonymous".</param>
    /// <param name="rateLimitInfo">Output parameter containing rate limit information</param>
    /// <returns>True if request is allowed, false if rate limit exceeded</returns>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    public static bool CheckRateLimit(this RateLimitingMiddleware middleware, string? clientIdentifier, out RateLimitInfo? rateLimitInfo)
    {
        ArgumentNullException.ThrowIfNull(middleware);

        return middleware.AllowRequest(clientIdentifier, out rateLimitInfo);
    }

    /// <summary>
    /// Resets the rate limit for a specific client
    /// </summary>
    /// <param name="middleware">The middleware instance. Cannot be null.</param>
    /// <param name="clientIdentifier">The client identifier to reset. If null or empty, treated as "anonymous".</param>
    /// <exception cref="ArgumentNullException"><paramref name="middleware"/> is null.</exception>
    public static void ResetClient(this RateLimitingMiddleware middleware, string? clientIdentifier)
    {
        ArgumentNullException.ThrowIfNull(middleware);

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