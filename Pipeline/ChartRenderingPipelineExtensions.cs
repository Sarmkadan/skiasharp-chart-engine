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
        /// <returns>The same pipeline instance, enabling fluent chaining.</returns>
        public static ChartRenderingPipeline AddStages(
            this ChartRenderingPipeline pipeline,
            params IPipelineStage[] stages)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            if (stages == null) throw new ArgumentNullException(nameof(stages));

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
        /// <returns>The same pipeline instance, enabling fluent chaining.</returns>
        public static ChartRenderingPipeline AddInterceptors(
            this ChartRenderingPipeline pipeline,
            params IPipelineInterceptor[] interceptors)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            if (interceptors == null) throw new ArgumentNullException(nameof(interceptors));

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
        /// <returns>
        /// A read‑only list of <see cref="StageExecutionResult"/> objects whose
        /// <c>Success</c> flag is true.
        /// </returns>
        public static async Task<IReadOnlyList<StageExecutionResult>> ExecuteAndGetSuccessfulStagesAsync(
            this ChartRenderingPipeline pipeline,
            Chart chart,
            CancellationToken cancellationToken = default)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            if (chart == null) throw new ArgumentNullException(nameof(chart));

            var result = await pipeline.ExecuteAsync(chart, new PipelineContext(), cancellationToken)
                                      .ConfigureAwait(false);

            return result.StageResults
                         .Where(sr => sr.Success)
                         .ToList()
                         .AsReadOnly();
        }

        /// <summary>
        /// Executes the pipeline and returns the result for a specific stage name,
        /// or <c>null</c> if the stage was not executed.
        /// </summary>
        /// <param name="pipeline">The pipeline to execute.</param>
        /// <param name="chart">The chart to render.</param>
        /// <param name="stageName">The name of the stage whose result is required.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>
        /// The <see cref="StageExecutionResult"/> for the requested stage, or
        /// <c>null</c> if the stage was not found or did not run.
        /// </returns>
        public static async Task<StageExecutionResult?> ExecuteAndGetStageResultAsync(
            this ChartRenderingPipeline pipeline,
            Chart chart,
            string stageName,
            CancellationToken cancellationToken = default)
        {
            if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
            if (chart == null) throw new ArgumentNullException(nameof(chart));
            if (string.IsNullOrWhiteSpace(stageName))
                throw new ArgumentException("Stage name must be provided.", nameof(stageName));

            var result = await pipeline.ExecuteAsync(chart, new PipelineContext(), cancellationToken)
                                      .ConfigureAwait(false);

            return result.StageResults.FirstOrDefault(sr => sr.StageName.Equals(stageName, StringComparison.Ordinal));
        }
    }
}
