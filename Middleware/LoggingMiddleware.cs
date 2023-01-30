// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Middleware;

/// <summary>
/// Middleware for logging API requests and responses
/// Tracks request duration, status, and details for observability
/// </summary>
public class LoggingMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs request and response information with timing metrics
    /// Helps with performance monitoring and debugging
    /// </summary>
    public void LogRequest(LoggingContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Request started - TraceId: {TraceId}, Method: {Method}, Path: {Path}, Timestamp: {Timestamp}",
                context.TraceId,
                context.Method,
                context.Path,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging request");
        }
    }

    /// <summary>
    /// Logs response with status code and elapsed time
    /// </summary>
    public void LogResponse(LoggingContext context, int statusCode, long? responseSize = null)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var elapsed = context.Stopwatch?.Elapsed ?? TimeSpan.Zero;

        try
        {
            var logLevel = statusCode >= 500 ? LogLevel.Error :
                          statusCode >= 400 ? LogLevel.Warning :
                          LogLevel.Information;

            _logger.Log(
                logLevel,
                "Request completed - TraceId: {TraceId}, Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, " +
                "ElapsedMs: {ElapsedMs}, ResponseSize: {ResponseSize} bytes",
                context.TraceId,
                context.Method,
                context.Path,
                statusCode,
                elapsed.TotalMilliseconds,
                responseSize ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging response");
        }
    }

    /// <summary>
    /// Logs detailed request body (useful for debugging)
    /// Be cautious with sensitive data
    /// </summary>
    public void LogRequestBody(string traceId, string method, string body)
    {
        try
        {
            if (string.IsNullOrEmpty(body))
                return;

            // Truncate large payloads to avoid excessive logging
            var truncatedBody = body.Length > 1000 ? body[..1000] + "... (truncated)" : body;

            _logger.LogDebug(
                "Request body - TraceId: {TraceId}, Method: {Method}, Body: {Body}",
                traceId,
                method,
                truncatedBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging request body");
        }
    }

    /// <summary>
    /// Logs error details for failed requests
    /// </summary>
    public void LogError(string traceId, string method, string path, Exception exception)
    {
        try
        {
            _logger.LogError(exception,
                "Request error - TraceId: {TraceId}, Method: {Method}, Path: {Path}",
                traceId,
                method,
                path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging error");
        }
    }
}

/// <summary>
/// Context object for logging middleware
/// Maintains request lifecycle information
/// </summary>
public class LoggingContext
{
    public string TraceId { get; set; } = Guid.NewGuid().ToString("N");
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? QueryString { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public string? RemoteIpAddress { get; set; }
    public Stopwatch? Stopwatch { get; set; }

    public LoggingContext()
    {
        Stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Gets a summary of the logging context
    /// </summary>
    public string GetSummary()
    {
        return $"{Method} {Path}{QueryString} (TraceId: {TraceId})";
    }
}
