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
using SkiaSharpChartEngine.API.Requests;
using SkiaSharpChartEngine.API.Responses;

namespace SkiaSharpChartEngine.API.Controllers;

/// <summary>
/// REST API controller for chart operations
/// Handles CRUD operations and chart rendering
/// </summary>
public class ChartController
{
    private readonly ChartEngine _chartEngine;
    private readonly ILogger<ChartController> _logger;

    public ChartController(ChartEngine chartEngine, ILogger<ChartController> logger)
    {
        _chartEngine = chartEngine ?? throw new ArgumentNullException(nameof(chartEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// GET /charts/{id}
    /// Retrieves a chart by ID
    /// </summary>
    public async Task<ApiResponse<ChartDto>> GetChartAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return ApiResponse<ChartDto>.BadRequest("Chart ID is required");

            var chart = await _chartEngine.GetChartAsync(id, cancellationToken);
            if (chart == null)
                return ApiResponse<ChartDto>.NotFound($"Chart with ID {id} not found");

            var dto = MapToDto(chart);
            return ApiResponse<ChartDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chart {ChartId}", id);
            return ApiResponse<ChartDto>.InternalError(ex.Message);
        }
    }

    /// <summary>
    /// POST /charts
    /// Creates a new chart
    /// </summary>
    public async Task<ApiResponse<string>> CreateChartAsync(
        CreateChartRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return ApiResponse<string>.BadRequest("Request body is required");

            if (string.IsNullOrEmpty(request.Title))
                return ApiResponse<string>.BadRequest("Chart title is required");

            var chart = new Chart
            {
                Id = Guid.NewGuid().ToString(),
                Title = request.Title,
                ChartType = request.ChartType,
                Configuration = request.Configuration,
                CreatedAt = DateTime.UtcNow
            };

            var chartId = await _chartEngine.SaveChartAsync(chart, cancellationToken);
            _logger.LogInformation("Chart created with ID {ChartId}", chartId);

            return ApiResponse<string>.Success(chartId, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chart");
            return ApiResponse<string>.InternalError(ex.Message);
        }
    }

    /// <summary>
    /// PUT /charts/{id}
    /// Updates an existing chart
    /// </summary>
    public async Task<ApiResponse<bool>> UpdateChartAsync(
        string id,
        UpdateChartRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return ApiResponse<bool>.BadRequest("Chart ID is required");

            if (request == null)
                return ApiResponse<bool>.BadRequest("Request body is required");

            var chart = await _chartEngine.GetChartAsync(id, cancellationToken);
            if (chart == null)
                return ApiResponse<bool>.NotFound($"Chart with ID {id} not found");

            if (!string.IsNullOrEmpty(request.Title))
                chart.Title = request.Title;

            if (request.Configuration != null)
                chart.Configuration = request.Configuration;

            var success = await _chartEngine.UpdateChartAsync(chart, cancellationToken);
            _logger.LogInformation("Chart {ChartId} updated", id);

            return ApiResponse<bool>.Success(success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating chart {ChartId}", id);
            return ApiResponse<bool>.InternalError(ex.Message);
        }
    }

    /// <summary>
    /// DELETE /charts/{id}
    /// Deletes a chart
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteChartAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
                return ApiResponse<bool>.BadRequest("Chart ID is required");

            var success = await _chartEngine.DeleteChartAsync(id, cancellationToken);
            if (!success)
                return ApiResponse<bool>.NotFound($"Chart with ID {id} not found");

            _logger.LogInformation("Chart {ChartId} deleted", id);
            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting chart {ChartId}", id);
            return ApiResponse<bool>.InternalError(ex.Message);
        }
    }

    private ChartDto MapToDto(Chart chart)
    {
        return new ChartDto
        {
            Id = chart.Id,
            Title = chart.Title,
            ChartType = chart.ChartType,
            Configuration = chart.Configuration,
            CreatedAt = chart.CreatedAt
        };
    }
}

public class ChartDto
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public ChartType ChartType { get; set; }
    public ChartConfiguration? Configuration { get; set; }
    public DateTime CreatedAt { get; set; }
}
