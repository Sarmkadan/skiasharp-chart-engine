// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Services;

/// <summary>
/// Orchestrates the chart rendering pipeline with validation, caching, and processing stages.
/// Implements pipeline pattern for flexible rendering workflows.
/// </summary>
public class RenderPipelineService
{
    private readonly List<IPipelineStage> _stages;
    private readonly ILogger<RenderPipelineService> _logger;

    public RenderPipelineService(ILogger<RenderPipelineService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stages = new List<IPipelineStage>();
    }

    // Add stage to pipeline
    public void AddStage(IPipelineStage stage)
    {
        if (stage != null)
        {
            _stages.Add(stage);
            _logger.LogInformation("Pipeline stage added: {StageName}", stage.Name);
        }
    }

    // Execute rendering pipeline
    public async Task<PipelineResult> ExecuteAsync(Chart chart)
    {
        try
        {
            if (chart == null)
                throw new ArgumentNullException(nameof(chart));

            _logger.LogInformation("Starting render pipeline for chart: {ChartId}", chart.Id);

            var stopwatch = Stopwatch.StartNew();
            var result = new PipelineResult
            {
                ChartId = chart.Id,
                StartedAt = DateTime.UtcNow,
                StageResults = new List<StageResult>()
            };

            var currentChart = chart;

            foreach (var stage in _stages)
            {
                try
                {
                    var stageStopwatch = Stopwatch.StartNew();
                    var stageResult = await stage.ExecuteAsync(currentChart);
                    stageStopwatch.Stop();

                    result.StageResults.Add(new StageResult
                    {
                        StageName = stage.Name,
                        Success = stageResult.Success,
                        Message = stageResult.Message,
                        ElapsedMs = stageStopwatch.ElapsedMilliseconds
                    });

                    if (!stageResult.Success)
                    {
                        _logger.LogWarning("Pipeline stage failed: {StageName}", stage.Name);
                        result.Success = false;
                        result.Error = stageResult.Message;
                        break;
                    }

                    currentChart = stageResult.Chart ?? currentChart;
                    _logger.LogDebug("Pipeline stage completed: {StageName} ({ElapsedMs}ms)", stage.Name, stageStopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing pipeline stage: {StageName}", stage.Name);
                    result.Success = false;
                    result.Error = ex.Message;
                    break;
                }
            }

            stopwatch.Stop();
            result.CompletedAt = DateTime.UtcNow;
            result.TotalElapsedMs = stopwatch.ElapsedMilliseconds;
            result.Success = result.Success && result.StageResults.TrueForAll(s => s.Success);

            _logger.LogInformation("Pipeline completed: {ChartId}, Success: {Success}, Total time: {ElapsedMs}ms",
                chart.Id, result.Success, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing rendering pipeline");
            return new PipelineResult
            {
                ChartId = chart?.Id ?? "unknown",
                Success = false,
                Error = ex.Message,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };
        }
    }

    // Get pipeline stages
    public IReadOnlyList<IPipelineStage> GetStages() => _stages.AsReadOnly();

    // Clear all stages
    public void Clear()
    {
        _stages.Clear();
        _logger.LogInformation("All pipeline stages cleared");
    }
}

/// <summary>
/// Interface for pipeline stages.
/// </summary>
public interface IPipelineStage
{
    string Name { get; }
    Task<PipelineStageResult> ExecuteAsync(Chart chart);
}

/// <summary>
/// Result from a pipeline stage.
/// </summary>
public class PipelineStageResult
{
    public bool Success { get; set; } = true;
    public string Message { get; set; }
    public Chart Chart { get; set; }
}

/// <summary>
/// Overall pipeline execution result.
/// </summary>
public class PipelineResult
{
    public string ChartId { get; set; }
    public bool Success { get; set; } = true;
    public string Error { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public long TotalElapsedMs { get; set; }
    public List<StageResult> StageResults { get; set; } = new List<StageResult>();
}

/// <summary>
/// Individual stage execution result.
/// </summary>
public class StageResult
{
    public string StageName { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
    public long ElapsedMs { get; set; }
}
