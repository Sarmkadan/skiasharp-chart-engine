// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.API.Requests;
using SkiaSharpChartEngine.API.Responses;

namespace SkiaSharpChartEngine.API.Controllers;

/// <summary>
/// REST API controller for chart export operations
/// Handles rendering and exporting charts to various formats
/// </summary>
public class ExportController
{
    private readonly ChartEngine _chartEngine;
    private readonly ILogger<ExportController> _logger;
    private readonly List<ExportFormat> _supportedFormats;

    public ExportController(ChartEngine chartEngine, ILogger<ExportController> logger)
    {
        _chartEngine = chartEngine ?? throw new ArgumentNullException(nameof(chartEngine));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _supportedFormats = _chartEngine.GetSupportedExportFormats().ToList();
    }

    /// <summary>
    /// POST /export/render
    /// Renders a chart and returns the image bytes
    /// </summary>
    public async Task<ApiResponse<byte[]>> RenderChartAsync(
        RenderChartRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!request.IsValid(out var validationError))
                return ApiResponse<byte[]>.BadRequest(validationError!);

            var chart = await _chartEngine.GetChartAsync(request.ChartId, cancellationToken);
            if (chart == null)
                return ApiResponse<byte[]>.NotFound($"Chart {request.ChartId} not found");

            var result = await _chartEngine.RenderChartAsync(chart, cancellationToken);
            if (!result.IsSuccess || result.ImageData == null)
                return ApiResponse<byte[]>.InternalError("Failed to render chart");

            _logger.LogInformation("Chart {ChartId} rendered successfully", request.ChartId);
            return ApiResponse<byte[]>.Success(result.ImageData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering chart {ChartId}", request.ChartId);
            return ApiResponse<byte[]>.InternalError(ex.Message);
        }
    }

    /// <summary>
    /// POST /export/batch
    /// Renders multiple charts in batch
    /// </summary>
    public async Task<ApiResponse<BatchExportResult>> BatchRenderAsync(
        BatchRenderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!request.IsValid(out var validationError))
                return ApiResponse<BatchExportResult>.BadRequest(validationError!);

            var renderSettings = request.RenderSettings ?? new RenderChartRequest
            {
                Width = 800,
                Height = 600,
                Dpi = 96f
            };

            var result = new BatchExportResult
            {
                TotalCharts = request.ChartIds.Count,
                SuccessfulRenders = 0,
                FailedRenders = 0,
                RenderResults = new List<IndividualRenderResult>()
            };

            foreach (var chartId in request.ChartIds)
            {
                try
                {
                    var chart = await _chartEngine.GetChartAsync(chartId, cancellationToken);
                    if (chart == null)
                    {
                        result.FailedRenders++;
                        result.RenderResults.Add(new IndividualRenderResult
                        {
                            ChartId = chartId,
                            Success = false,
                            Message = "Chart not found"
                        });
                        continue;
                    }

                    var renderResult = await _chartEngine.RenderChartAsync(chart, cancellationToken);
                    if (renderResult.IsSuccess)
                    {
                        result.SuccessfulRenders++;
                        result.RenderResults.Add(new IndividualRenderResult
                        {
                            ChartId = chartId,
                            Success = true,
                            Message = "Rendered successfully",
                            FileSize = renderResult.ImageData?.Length ?? 0
                        });
                    }
                    else
                    {
                        result.FailedRenders++;
                        result.RenderResults.Add(new IndividualRenderResult
                        {
                            ChartId = chartId,
                            Success = false,
                            Message = renderResult.ErrorMessage
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.FailedRenders++;
                    result.RenderResults.Add(new IndividualRenderResult
                    {
                        ChartId = chartId,
                        Success = false,
                        Message = ex.Message
                    });
                }
            }

            _logger.LogInformation("Batch render completed: {Success}/{Total} successful",
                result.SuccessfulRenders, result.TotalCharts);

            return ApiResponse<BatchExportResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch render");
            return ApiResponse<BatchExportResult>.InternalError(ex.Message);
        }
    }

    /// <summary>
    /// GET /export/formats
    /// Returns list of supported export formats
    /// </summary>
    public ApiResponse<List<ExportFormatInfo>> GetSupportedFormats()
    {
        try
        {
            var formats = _supportedFormats
                .Select(f => new ExportFormatInfo
                {
                    Format = f.ToString(),
                    MimeType = GetMimeType(f),
                    FileExtension = GetFileExtension(f)
                })
                .ToList();

            return ApiResponse<List<ExportFormatInfo>>.Success(formats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving supported formats");
            return ApiResponse<List<ExportFormatInfo>>.InternalError(ex.Message);
        }
    }

    private string GetMimeType(ExportFormat format) => format switch
    {
        ExportFormat.PNG => "image/png",
        ExportFormat.SVG => "image/svg+xml",
        ExportFormat.PDF => "application/pdf",
        ExportFormat.JPEG => "image/jpeg",
        _ => "application/octet-stream"
    };

    private string GetFileExtension(ExportFormat format) => format switch
    {
        ExportFormat.PNG => ".png",
        ExportFormat.SVG => ".svg",
        ExportFormat.PDF => ".pdf",
        ExportFormat.JPEG => ".jpg",
        _ => ".bin"
    };
}

public class BatchExportResult
{
    public int TotalCharts { get; set; }
    public int SuccessfulRenders { get; set; }
    public int FailedRenders { get; set; }
    public List<IndividualRenderResult> RenderResults { get; set; } = new();
}

public class IndividualRenderResult
{
    public string? ChartId { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
    public long FileSize { get; set; }
}

public class ExportFormatInfo
{
    public string? Format { get; set; }
    public string? MimeType { get; set; }
    public string? FileExtension { get; set; }
}
