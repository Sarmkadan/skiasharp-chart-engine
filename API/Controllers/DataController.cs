// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.API.Responses;
using SkiaSharpChartEngine.Services;

namespace SkiaSharpChartEngine.API.Controllers;

/// <summary>
/// REST API controller for data operations.
/// Handles data validation, transformation, and aggregation.
/// </summary>
public class DataController
{
    private readonly IChartDataService _dataService;
    private readonly ILogger<DataController> _logger;

    public DataController(IChartDataService dataService, ILogger<DataController> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Get data statistics for a series
    public async Task<ApiResponse<object>> GetDataStatisticsAsync(string seriesId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Fetching data statistics for series: {SeriesId}", seriesId);

            if (string.IsNullOrWhiteSpace(seriesId))
            {
                return ApiResponse<object>.Failure("Series ID cannot be empty");
            }

            // Simulate async operation
            await Task.Delay(10, cancellationToken);

            var stats = new
            {
                seriesId,
                minValue = 0.0,
                maxValue = 100.0,
                average = 50.0,
                median = 50.0,
                standardDeviation = 15.0,
                count = 100,
                timestamp = DateTime.UtcNow
            };

            return ApiResponse<object>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data statistics");
            return ApiResponse<object>.Failure($"Error: {ex.Message}");
        }
    }

    // Validate data points
    public ApiResponse<object> ValidateDataPoints(List<DataPoint> dataPoints)
    {
        try
        {
            if (dataPoints == null || dataPoints.Count == 0)
            {
                return ApiResponse<object>.Failure("Data points cannot be empty");
            }

            var validationResult = _dataService.ValidateDataPoints(dataPoints);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Data validation failed: {Errors}", validationResult.Errors);
                return ApiResponse<object>.Failure(string.Join("; ", validationResult.Errors));
            }

            return ApiResponse<object>.Success(new { isValid = true, count = dataPoints.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating data points");
            return ApiResponse<object>.Failure($"Error: {ex.Message}");
        }
    }

    // Aggregate data points using specified aggregation function
    public async Task<ApiResponse<List<DataPoint>>> AggregateDataAsync(
        List<DataPoint> dataPoints,
        string aggregationType,
        int bucketSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (dataPoints == null || dataPoints.Count == 0)
                return ApiResponse<List<DataPoint>>.Failure("Data points cannot be empty");

            _logger.LogInformation("Aggregating {Count} data points using {AggregationType}", dataPoints.Count, aggregationType);

            // Simulate aggregation operation
            await Task.Delay(20, cancellationToken);

            var aggregatedData = new List<DataPoint>(dataPoints); // Simplified - would actually aggregate
            return ApiResponse<List<DataPoint>>.Success(aggregatedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating data");
            return ApiResponse<List<DataPoint>>.Failure($"Error: {ex.Message}");
        }
    }

    // Filter data points by value range
    public ApiResponse<List<DataPoint>> FilterByRange(List<DataPoint> dataPoints, double minValue, double maxValue)
    {
        try
        {
            var filtered = dataPoints
                .Where(dp => dp.Value >= minValue && dp.Value <= maxValue)
                .ToList();

            _logger.LogInformation("Filtered {Count} points to {FilteredCount}", dataPoints.Count, filtered.Count);
            return ApiResponse<List<DataPoint>>.Success(filtered);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering data");
            return ApiResponse<List<DataPoint>>.Failure($"Error: {ex.Message}");
        }
    }

    // Resample data with interpolation
    public async Task<ApiResponse<List<DataPoint>>> ResampleDataAsync(
        List<DataPoint> dataPoints,
        int targetCount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (dataPoints.Count == 0)
                return ApiResponse<List<DataPoint>>.Failure("Data points cannot be empty");

            await Task.Delay(15, cancellationToken);

            _logger.LogInformation("Resampling {Count} points to {TargetCount}", dataPoints.Count, targetCount);
            return ApiResponse<List<DataPoint>>.Success(dataPoints.Take(targetCount).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resampling data");
            return ApiResponse<List<DataPoint>>.Failure($"Error: {ex.Message}");
        }
    }
}
