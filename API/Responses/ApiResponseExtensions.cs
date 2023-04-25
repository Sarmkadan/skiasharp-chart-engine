// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

using SkiaSharpChartEngine.API.Responses;

namespace SkiaSharpChartEngine.API.Responses;

/// <summary>
/// Extension methods for <see cref="ApiResponse{T}"/> and <see cref="PaginatedResponse{T}"/> classes.
/// Provides common operations for API responses including trace correlation and pagination.
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Adds trace ID to the response for better debugging and correlation.
    /// </summary>
    /// <typeparam name="T">Type of data in the response</typeparam>
    /// <param name="response">The API response</param>
    /// <param name="traceId">Trace ID for debugging and request correlation</param>
    /// <returns>ApiResponse with trace ID set</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="traceId"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
    public static ApiResponse<T> WithTraceId<T>(this ApiResponse<T> response, string traceId)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        response.TraceId = traceId;
        return response;
    }

    /// <summary>
    /// Adds error message to a failed response.
    /// </summary>
    /// <typeparam name="T">Type of data in the response</typeparam>
    /// <param name="response">The API response</param>
    /// <param name="errorMessage">Error message to set</param>
    /// <returns>ApiResponse with error message set</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentException"><paramref name="errorMessage"/> is <see langword="null"/>, empty, or consists only of whitespace</exception>
    public static ApiResponse<T> WithError<T>(this ApiResponse<T> response, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        response.Message = errorMessage;
        response.Success = false;
        return response;
    }

    /// <summary>
    /// Converts paginated response to standard response by extracting first page data.
    /// </summary>
    /// <typeparam name="T">Type of data in the response</typeparam>
    /// <param name="paginatedResponse">Paginated response to convert</param>
    /// <returns>Standard ApiResponse with first page data</returns>
    /// <exception cref="ArgumentNullException"><paramref name="paginatedResponse"/> is <see langword="null"/></exception>
    public static ApiResponse<List<T>> ToStandardResponse<T>(this PaginatedResponse<T> paginatedResponse)
    {
        ArgumentNullException.ThrowIfNull(paginatedResponse);

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
    /// Creates a paginated response from a standard response by wrapping the data.
    /// </summary>
    /// <typeparam name="T">Type of data in the response</typeparam>
    /// <param name="response">Standard response to paginate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="totalItems">Total number of items across all pages</param>
    /// <returns>Paginated response</returns>
    /// <exception cref="ArgumentNullException"><paramref name="response"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageNumber"/> is less than 1</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageSize"/> is less than 1</exception>
    public static PaginatedResponse<T> ToPaginatedResponse<T>(
        this ApiResponse<T> response,
        int pageNumber,
        int pageSize,
        int totalItems)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

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
    /// Creates a paginated response from a list of items.
    /// </summary>
    /// <typeparam name="T">Type of items to paginate</typeparam>
    /// <param name="items">Items to paginate</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <returns>Paginated response</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items"/> is <see langword="null"/></exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageNumber"/> is less than 1</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pageSize"/> is less than 1</exception>
    public static PaginatedResponse<T> ToPaginatedResponse<T>(
        this IEnumerable<T> items,
        int pageNumber,
        int pageSize)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        var totalItems = items.Count();
        var pageData = items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return PaginatedResponse<T>.Ok(pageData, pageNumber, pageSize, totalItems);
    }
}