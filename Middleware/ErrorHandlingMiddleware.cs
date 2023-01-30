// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Exceptions;

namespace SkiaSharpChartEngine.Middleware;

/// <summary>
/// Global error handling middleware for consistent error responses
/// Catches exceptions and formats them appropriately
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly Func<Exception, Task>? _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly Dictionary<Type, (int StatusCode, string Message)> _exceptionMapping;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _exceptionMapping = InitializeExceptionMapping();
    }

    /// <summary>
    /// Processes exceptions and returns formatted error responses
    /// Maps different exception types to appropriate HTTP status codes
    /// </summary>
    public async Task InvokeAsync(Exception exception)
    {
        try
        {
            if (exception == null)
                return;

            _logger.LogError(exception, "Unhandled exception occurred");

            var (statusCode, message) = MapException(exception);

            var errorResponse = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                Details = exception.Message,
                Timestamp = DateTime.UtcNow,
                ExceptionType = exception.GetType().Name
            };

            // In a real ASP.NET Core app, we would write this to the response
            // For this library context, we just log it
            _logger.LogInformation("Error response generated: {StatusCode} - {Message}",
                statusCode, message);

            if (exception is not ChartEngineException)
            {
                errorResponse.Details = "An internal error occurred. Please try again later.";
            }

            // Propagate for higher-level handling
            throw new MiddlewareException(errorResponse, exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in error handling middleware");
            throw;
        }
    }

    private (int StatusCode, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => (400, "Invalid argument"),
            ArgumentException => (400, "Bad request"),
            ChartEngineException => (422, "Chart engine error"),
            InvalidOperationException => (409, "Invalid operation"),
            NotImplementedException => (501, "Not implemented"),
            TimeoutException => (504, "Request timeout"),
            _ => (500, "Internal server error")
        };
    }

    private Dictionary<Type, (int StatusCode, string Message)> InitializeExceptionMapping()
    {
        return new Dictionary<Type, (int, string)>
        {
            { typeof(ArgumentNullException), (400, "Null argument provided") },
            { typeof(ArgumentException), (400, "Invalid argument") },
            { typeof(InvalidOperationException), (409, "Invalid operation") },
            { typeof(NotImplementedException), (501, "Feature not implemented") },
            { typeof(TimeoutException), (504, "Operation timeout") },
            { typeof(UnauthorizedAccessException), (401, "Unauthorized") },
            { typeof(KeyNotFoundException), (404, "Resource not found") }
        };
    }
}

/// <summary>
/// Error response model returned by error handling middleware
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public string? Details { get; set; }
    public string? ExceptionType { get; set; }
    public DateTime Timestamp { get; set; }
    public string? TraceId { get; set; }
}

/// <summary>
/// Exception thrown by error handling middleware to wrap formatted errors
/// </summary>
public class MiddlewareException : Exception
{
    public ErrorResponse ErrorResponse { get; }

    public MiddlewareException(ErrorResponse errorResponse, Exception innerException)
        : base(errorResponse.Message, innerException)
    {
        ErrorResponse = errorResponse;
    }
}
