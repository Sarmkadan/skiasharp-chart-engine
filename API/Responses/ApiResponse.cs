// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SkiaSharpChartEngine.API.Responses;

/// <summary>
/// Standard API response wrapper for all endpoints
/// Provides consistent response format with status codes and messages
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error message if request failed
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Timestamp when response was generated
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Request trace ID for debugging
    /// </summary>
    public string? TraceId { get; set; }

    public ApiResponse()
    {
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> Success(T data, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            Success = true,
            Data = data,
            Message = "Success"
        };
    }

    /// <summary>
    /// Creates a bad request response (400)
    /// </summary>
    public static ApiResponse<T> BadRequest(string message)
    {
        return new ApiResponse<T>
        {
            StatusCode = 400,
            Success = false,
            Message = message
        };
    }

    /// <summary>
    /// Creates an unauthorized response (401)
    /// </summary>
    public static ApiResponse<T> Unauthorized(string message = "Unauthorized")
    {
        return new ApiResponse<T>
        {
            StatusCode = 401,
            Success = false,
            Message = message
        };
    }

    /// <summary>
    /// Creates a forbidden response (403)
    /// </summary>
    public static ApiResponse<T> Forbidden(string message = "Forbidden")
    {
        return new ApiResponse<T>
        {
            StatusCode = 403,
            Success = false,
            Message = message
        };
    }

    /// <summary>
    /// Creates a not found response (404)
    /// </summary>
    public static ApiResponse<T> NotFound(string message)
    {
        return new ApiResponse<T>
        {
            StatusCode = 404,
            Success = false,
            Message = message
        };
    }

    /// <summary>
    /// Creates a conflict response (409)
    /// </summary>
    public static ApiResponse<T> Conflict(string message)
    {
        return new ApiResponse<T>
        {
            StatusCode = 409,
            Success = false,
            Message = message
        };
    }

    /// <summary>
    /// Creates an internal server error response (500)
    /// </summary>
    public static ApiResponse<T> InternalError(string message)
    {
        return new ApiResponse<T>
        {
            StatusCode = 500,
            Success = false,
            Message = message
        };
    }

    /// <summary>
    /// Creates a service unavailable response (503)
    /// </summary>
    public static ApiResponse<T> ServiceUnavailable(string message = "Service unavailable")
    {
        return new ApiResponse<T>
        {
            StatusCode = 503,
            Success = false,
            Message = message
        };
    }
}

/// <summary>
/// Paginated API response for list endpoints
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Page data
    /// </summary>
    public List<T> Data { get; set; } = new();

    /// <summary>
    /// Current page number
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Error message if failed
    /// </summary>
    public string? Message { get; set; }

    public PaginatedResponse()
    {
    }

    /// <summary>
    /// Creates a successful paginated response
    /// </summary>
    public static PaginatedResponse<T> Success(List<T> data, int pageNumber, int pageSize, int totalItems)
    {
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        return new PaginatedResponse<T>
        {
            StatusCode = 200,
            Success = true,
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Message = "Success"
        };
    }

    /// <summary>
    /// Creates an error paginated response
    /// </summary>
    public static PaginatedResponse<T> Error(int statusCode, string message)
    {
        return new PaginatedResponse<T>
        {
            StatusCode = statusCode,
            Success = false,
            Message = message
        };
    }
}
