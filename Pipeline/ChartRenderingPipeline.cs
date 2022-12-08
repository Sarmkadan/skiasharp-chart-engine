// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Pipeline;

/// <summary>
/// Pipeline for processing chart rendering requests
/// Executes stages sequentially with middleware support
/// </summary>
public class ChartRenderingPipeline
{
    private readonly ILogger<ChartRenderingPipeline> _logger;
    private readonly List<IPipelineStage> _stages;
    private readonly List<IPipelineInterceptor> _interceptors;

    public ChartRenderingPipeline(ILogger<ChartRenderingPipeline> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stages = new List<IPipelineStage>();
        _interceptors = new List<IPipelineInterceptor>();
    }

    /// <summary>
    /// Registers a pipeline stage
    /// </summary>
    public ChartRenderingPipeline AddStage(IPipelineStage stage)
    {
        if (stage != null)
            _stages.Add(stage);

        return this;
    }

    /// <summary>
    /// Registers a pipeline interceptor
    /// </summary>
    public ChartRenderingPipeline AddInterceptor(IPipelineInterceptor interceptor)
    {
        if (interceptor != null)
            _interceptors.Add(interceptor);

        return this;
    }

    /// <summary>
    /// Executes the complete rendering pipeline
    /// </summary>
    public async Task<PipelineResult> ExecuteAsync(
        Chart chart,
        PipelineContext context,
        CancellationToken cancellationToken = default)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (context == null)
            context = new PipelineContext();

        var stopwatch = Stopwatch.StartNew();
        var result = new PipelineResult { ChartId = chart.Id };

        try
        {
            _logger.LogInformation("Starting rendering pipeline for chart {ChartId}", chart.Id);

            // Execute before interceptors
            foreach (var interceptor in _interceptors)
            {
                try
                {
                    await interceptor.OnBeforeAsync(context, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in before interceptor {Type}", interceptor.GetType().Name);
                    throw;
                }
            }

            // Execute stages
            foreach (var stage in _stages)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var stageStopwatch = Stopwatch.StartNew();

                try
                {
                    _logger.LogDebug("Executing pipeline stage: {StageName}", stage.Name);

                    var stageResult = await stage.ExecuteAsync(chart, context, cancellationToken);

                    stageStopwatch.Stop();
                    result.StageResults.Add(new StageExecutionResult
                    {
                        StageName = stage.Name,
                        Success = stageResult.IsSuccess,
                        Message = stageResult.Message,
                        DurationMs = stageStopwatch.ElapsedMilliseconds,
                        Output = stageResult.Output
                    });

                    if (!stageResult.IsSuccess)
                    {
                        _logger.LogError("Stage {StageName} failed: {Message}", stage.Name, stageResult.Message);
                        result.Success = false;
                        result.ErrorMessage = stageResult.Message;
                        break;
                    }

                    _logger.LogDebug("Stage {StageName} completed in {Duration}ms",
                        stage.Name, stageStopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    stageStopwatch.Stop();
                    _logger.LogError(ex, "Error in pipeline stage {StageName}", stage.Name);
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                    result.Exception = ex;
                    break;
                }
            }

            // Execute after interceptors
            foreach (var interceptor in _interceptors.Reverse<IPipelineInterceptor>())
            {
                try
                {
                    await interceptor.OnAfterAsync(result, context, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in after interceptor {Type}", interceptor.GetType().Name);
                }
            }

            stopwatch.Stop();
            result.TotalDurationMs = stopwatch.ElapsedMilliseconds;

            if (result.Success)
            {
                _logger.LogInformation("Rendering pipeline completed successfully in {Duration}ms",
                    stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Critical error in rendering pipeline");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Exception = ex;
            result.TotalDurationMs = stopwatch.ElapsedMilliseconds;
            return result;
        }
    }
}

/// <summary>
/// Interface for pipeline stages
/// </summary>
public interface IPipelineStage
{
    string Name { get; }
    Task<PipelineStageResult> ExecuteAsync(Chart chart, PipelineContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Interface for pipeline interceptors
/// </summary>
public interface IPipelineInterceptor
{
    Task OnBeforeAsync(PipelineContext context, CancellationToken cancellationToken);
    Task OnAfterAsync(PipelineResult result, PipelineContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Pipeline execution context
/// </summary>
public class PipelineContext
{
    public Dictionary<string, object> Data { get; } = new();
    public DateTime StartTime { get; } = DateTime.UtcNow;

    public void Set<T>(string key, T value) => Data[key] = value!;

    public T? Get<T>(string key)
    {
        if (Data.TryGetValue(key, out var value))
            return (T?)value;
        return default;
    }
}

/// <summary>
/// Pipeline stage result
/// </summary>
public class PipelineStageResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public object? Output { get; set; }

    public static PipelineStageResult Success(object? output = null) => new()
    {
        IsSuccess = true,
        Output = output
    };

    public static PipelineStageResult Failure(string message) => new()
    {
        IsSuccess = false,
        Message = message
    };
}

/// <summary>
/// Pipeline execution result
/// </summary>
public class PipelineResult
{
    public string? ChartId { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public List<StageExecutionResult> StageResults { get; } = new();
    public long TotalDurationMs { get; set; }

    public int SuccessfulStages => StageResults.Count(s => s.Success);
    public int FailedStages => StageResults.Count(s => !s.Success);
}

/// <summary>
/// Individual stage execution result
/// </summary>
public class StageExecutionResult
{
    public string StageName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Message { get; set; }
    public long DurationMs { get; set; }
    public object? Output { get; set; }
}
