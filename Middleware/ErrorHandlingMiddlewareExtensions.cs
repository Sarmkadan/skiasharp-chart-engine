// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Exceptions;

namespace SkiaSharpChartEngine.Middleware;

/// <summary>
/// Extension methods for ErrorHandlingMiddleware to provide additional functionality
/// </summary>
public static class ErrorHandlingMiddlewareExtensions
{
    /// <summary>
    /// Creates a new ErrorHandlingMiddleware instance with the specified logger
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <returns>A new ErrorHandlingMiddleware instance</returns>
    public static ErrorHandlingMiddleware WithLogger(this ILogger<ErrorHandlingMiddleware> logger)
    {
        if (logger == null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        return new ErrorHandlingMiddleware(logger);
    }

    /// <summary>
    /// Creates a new ErrorHandlingMiddleware instance with a default console logger
    /// </summary>
    /// <returns>A new ErrorHandlingMiddleware instance with console logging</returns>
    public static ErrorHandlingMiddleware WithConsoleLogger()
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger<ErrorHandlingMiddleware>();
        return new ErrorHandlingMiddleware(logger);
    }

    /// <summary>
    /// Creates a new ErrorHandlingMiddleware instance with a null logger (no logging)
    /// </summary>
    /// <returns>A new ErrorHandlingMiddleware instance with no logging</returns>
    public static ErrorHandlingMiddleware WithNoLogging()
    {
        var logger = new NullLogger<ErrorHandlingMiddleware>();
        return new ErrorHandlingMiddleware(logger);
    }

    /// <summary>
    /// Creates a new ErrorHandlingMiddleware instance with a logger that captures log messages
    /// </summary>
    /// <param name="logMessages">Output parameter that will contain captured log messages</param>
    /// <returns>A new ErrorHandlingMiddleware instance with capturing logger</returns>
    public static ErrorHandlingMiddleware WithCapturingLogger(out List<string> logMessages)
    {
        logMessages = new List<string>();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddProvider(new CapturingLoggerProvider(logMessages));
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        var logger = loggerFactory.CreateLogger<ErrorHandlingMiddleware>();
        return new ErrorHandlingMiddleware(logger);
    }

    /// <summary>
    /// Processes an exception and returns the MiddlewareException that would be thrown
    /// Useful for testing error handling without actually throwing exceptions
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="exception">The exception to process</param>
    /// <returns>The MiddlewareException that would be thrown by InvokeAsync</returns>
    public static async Task<MiddlewareException> ProcessExceptionAsync(this ErrorHandlingMiddleware middleware, Exception exception)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        try
        {
            await middleware.InvokeAsync(exception);
        }
        catch (MiddlewareException me)
        {
            return me;
        }

        throw new InvalidOperationException("InvokeAsync should always throw MiddlewareException");
    }

    /// <summary>
    /// Creates an ErrorResponse from an exception without throwing
    /// Useful for testing error responses without side effects
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="exception">The exception to process</param>
    /// <returns>The generated ErrorResponse</returns>
    public static async Task<ErrorResponse> CreateErrorResponseAsync(this ErrorHandlingMiddleware middleware, Exception exception)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        var (statusCode, message) = middleware.MapException(exception);
        var errorResponse = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Details = exception is ChartEngineException ? exception.Message : "An internal error occurred. Please try again later.",
            Timestamp = DateTime.UtcNow,
            ExceptionType = exception.GetType().Name,
            TraceId = Guid.NewGuid().ToString()
        };

        return errorResponse;
    }

    /// <summary>
    /// Maps an exception to its corresponding status code and message
    /// </summary>
    /// <param name="middleware">The middleware instance</param>
    /// <param name="exception">The exception to map</param>
    /// <returns>Tuple containing status code and message</returns>
    public static (int StatusCode, string Message) MapException(this ErrorHandlingMiddleware middleware, Exception exception)
    {
        if (middleware == null)
        {
            throw new ArgumentNullException(nameof(middleware));
        }

        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return middleware.MapException(exception);
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

/// <summary>
/// Logger provider that captures log messages for testing
/// </summary>
internal sealed class CapturingLoggerProvider : ILoggerProvider
{
    private readonly List<string> _logMessages;

    public CapturingLoggerProvider(List<string> logMessages)
    {
        _logMessages = logMessages ?? throw new ArgumentNullException(nameof(logMessages));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new CapturingLogger(categoryName, _logMessages);
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}

/// <summary>
/// Logger that captures messages instead of writing them
/// </summary>
internal sealed class CapturingLogger : ILogger
{
    private readonly string _categoryName;
    private readonly List<string> _logMessages;

    public CapturingLogger(string categoryName, List<string> logMessages)
    {
        _categoryName = categoryName;
        _logMessages = logMessages;
    }

    public IDisposable? BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _logMessages.Add($"[{_categoryName}] [{logLevel}] {message}");
    }
}