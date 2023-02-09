// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiaSharpChartEngine.API.Responses;

/// <summary>
/// Extension methods for ApiResponse and PaginatedResponse classes
/// Provides common operations for API responses
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Adds trace ID to the response for better debugging and correlation
    /// </summary>
    /// <param name="response">The API response</param>
    /// <param name="traceId">Trace ID for debugging</param>
    /// <returns>ApiResponse with trace ID set</returns>
    public static ApiResponse<T> WithTraceId<T>(this ApiResponse<T> response, string traceId)
    {
        if (response == null)
            throw new ArgumentNullException(nameof(response));

        if (string.IsNullOrWhiteSpace(traceId))
            throw new ArgumentException("Trace ID cannot be null or empty", nameof(traceId));

        response.TraceId = traceId;
        return response;
    }

    /// <summary>
    /// Adds error message to a failed response
    /// </summary>
    /// <param name="response">The API response</param>
    /// <param name="errorMessage">Error message to set</param>
    /// <returns>ApiResponse with error message set</returns>
    public static ApiResponse<T> WithError<T>(this ApiResponse<T> response, string errorMessage)
    {
        if (response == null)
            throw new ArgumentNullException(nameof(response));

        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be null or empty", nameof(errorMessage));

        response.Message = errorMessage;
        response.Success = false;
        return response;
    }

    /// <summary>
    /// Converts paginated response to standard response by extracting first page data
    /// </summary>
    /// <param name="paginatedResponse">Paginated response to convert</param>
    /// <returns>Standard ApiResponse with first page data</returns>
    public static ApiResponse<List<T>> ToStandardResponse<T>(this PaginatedResponse<T> paginatedResponse)
    {
        if (paginatedResponse == null)
            throw new ArgumentNullException(nameof(paginatedResponse));

        if (!paginatedResponse.Success && paginatedResponse.Data == null)
        {
            return new ApiResponse<List<T>>
            {
                StatusCode = paginatedResponse.StatusCode,
                Success = false,
                Message = paginatedResponse.Message,
                Timestamp = DateTime.UtcNow
            };
        }

        return ApiResponse<List<T>>.Ok(paginatedResponse.Data, paginatedResponse.StatusCode)
            .WithTraceId(paginatedResponse.TraceId);
    }

    /// <summary>
    /// Creates a paginated response from a standard response by wrapping the data
    /// </summary>
    /// <param name="response">Standard response to paginate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="totalItems">Total number of items across all pages</param>
    /// <returns>Paginated response</returns>
    public static PaginatedResponse<T> ToPaginatedResponse<T>(
        this ApiResponse<T> response,
        int pageNumber,
        int pageSize,
        int totalItems)
    {
        if (response == null)
            throw new ArgumentNullException(nameof(response));

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be 1 or greater");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be 1 or greater");

        var paginatedResponse = new PaginatedResponse<T>
        {
            StatusCode = response.StatusCode,
            Success = response.Success,
            Data = response.Data != null ? new List<T> { response.Data } : new List<T>(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            Message = response.Message
        };

        if (response.TraceId != null)
        {
            paginatedResponse.TraceId = response.TraceId;
        }

        return paginatedResponse;
    }

    /// <summary>
    /// Creates a paginated response from a list of items
    /// </summary>
    /// <param name="items">Items to paginate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated response</returns>
    public static PaginatedResponse<T> ToPaginatedResponse<T>(
        this IEnumerable<T> items,
        int pageNumber,
        int pageSize)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be 1 or greater");

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be 1 or greater");

        var totalItems = items.Count();
        var pageData = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return PaginatedResponse<T>.Ok(pageData, pageNumber, pageSize, totalItems);
    }
}