// =============================================================================
// Extensions for ChartRenderingPipeline
// Provides convenient helpers for adding multiple stages/interceptors and
// retrieving stage results after execution.
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharpChartEngine.Models;
using SkiaSharpChartEngine.Pipeline; // Ensure IPipelineStage and IPipelineInterceptor are in scope

namespace SkiaSharpChartEngine.Pipeline
{
    /// <summary>
    /// Extension methods that make working with <see cref="ChartRenderingPipeline"/>
    /// more ergonomic.
    /// </summary>
    public static class ChartRenderingPipelineExtensions
    {
        /// <summary>
        /// Adds multiple pipeline stages in a single call.
        /// </summary>
        /// <param name="pipeline">The pipeline to extend.</param>
        /// <param name="stages">The stages to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="stages"/> is <see langword="null"/>.</exception>
        /// <returns>The same pipeline instance, enabling fluent chaining.</returns>
        public static ChartRenderingPipeline AddStages(
            this ChartRenderingPipeline pipeline,
            params IPipelineStage[] stages)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentNullException.ThrowIfNull(stages);

            foreach (var stage in stages)
            {
                pipeline.AddStage(stage);
            }

            return pipeline;
        }

        /// <summary>
        /// Adds multiple pipeline interceptors in a single call.
        /// </summary>
        /// <param name="pipeline">The pipeline to extend.</param>
        /// <param name="interceptors">The interceptors to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="interceptors"/> is <see langword="null"/>.</exception>
        /// <returns>The same pipeline instance, enabling fluent chaining.</returns>
        public static ChartRenderingPipeline AddInterceptors(
            this ChartRenderingPipeline pipeline,
            params IPipelineInterceptor[] interceptors)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentNullException.ThrowIfNull(interceptors);

            foreach (var interceptor in interceptors)
            {
                pipeline.AddInterceptor(interceptor);
            }

            return pipeline;
        }

        /// <summary>
        /// Executes the pipeline and returns only the successful stage results.
        /// </summary>
        /// <param name="pipeline">The pipeline to execute.</param>
        /// <param name="chart">The chart to render.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="chart"/> is <see langword="null"/>.</exception>
        /// <returns>A read-only list of <see cref="StageExecutionResult"/> objects whose <c>Success</c> flag is true.</returns>
        public static async Task<IReadOnlyList<StageExecutionResult>> ExecuteAndGetSuccessfulStagesAsync(
            this ChartRenderingPipeline pipeline,
            Chart chart,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentNullException.ThrowIfNull(chart);

            var result = await pipeline.ExecuteAsync(chart, new PipelineContext(), cancellationToken)
                .ConfigureAwait(false);

            return result.StageResults.Where(sr => sr.Success).ToList().AsReadOnly();
        }

        /// <summary>
        /// Executes the pipeline and returns the result for a specific stage name,
        /// or <c>null</c> if the stage was not executed.
        /// </summary>
        /// <param name="pipeline">The pipeline to execute.</param>
        /// <param name="chart">The chart to render.</param>
        /// <param name="stageName">The name of the stage whose result is required.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pipeline"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="chart"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="stageName"/> is <see langword="null"/>, empty, or consists only of whitespace.</exception>
        /// <returns>The <see cref="StageExecutionResult"/> for the requested stage, or <c>null</c> if the stage was not found or did not run.</returns>
        public static async Task<StageExecutionResult?> ExecuteAndGetStageResultAsync(
            this ChartRenderingPipeline pipeline,
            Chart chart,
            string stageName,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(pipeline);
            ArgumentNullException.ThrowIfNull(chart);
            ArgumentException.ThrowIfNullOrEmpty(stageName);

            var result = await pipeline.ExecuteAsync(chart, new PipelineContext(), cancellationToken)
                .ConfigureAwait(false);

            return result.StageResults.FirstOrDefault(sr => sr.StageName.Equals(stageName, StringComparison.Ordinal));
        }
    }
}
