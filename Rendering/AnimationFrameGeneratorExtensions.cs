using System;
using System.Collections.Generic;

namespace Rendering
{
    /// <summary>
    /// Provides extension methods for <see cref="AnimationFrameGenerator"/>.
    /// </summary>
    public static class AnimationFrameGeneratorExtensions
    {
        /// <summary>
        /// Gets the current progress expressed as a percentage (0‑100).
        /// </summary>
        /// <param name="generator">The <see cref="AnimationFrameGenerator"/> instance.</param>
        /// <returns>The progress multiplied by 100.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="generator"/> is <c>null</c>.</exception>
        public static double GetProgressPercentage(this AnimationFrameGenerator generator) =>
            ArgumentNullException.ThrowIfNull(generator) is null
                ? throw new ArgumentNullException(nameof(generator))
                : generator.Progress * 100.0;

        /// <summary>
        /// Retrieves the chart that corresponds to the current <see cref="AnimationFrameGenerator.FrameNumber"/>.
        /// </summary>
        /// <param name="generator">The <see cref="AnimationFrameGenerator"/> instance.</param>
        /// <returns>
        /// The <see cref="Chart"/> at the current frame index, or <c>null</c> if the index is out of range
        /// or no frames have been generated.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="generator"/> is <c>null</c>.</exception>
        public static Chart? GetCurrentChart(this AnimationFrameGenerator generator)
        {
            ArgumentNullException.ThrowIfNull(generator);

            var frames = generator.GenerateFrames;
            if (frames == null || frames.Count == 0)
                return null;

            var index = generator.FrameNumber;
            return index >= 0 && index < frames.Count ? frames[index] : null;
        }

        /// <summary>
        /// Returns the collection of generated animation frames as a read‑only list.
        /// </summary>
        /// <param name="generator">The <see cref="AnimationFrameGenerator"/> instance.</param>
        /// <returns>An <see cref="IReadOnlyList{AnimationFrame}"/> representing the generated frames.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="generator"/> is <c>null</c>.</exception>
        public static IReadOnlyList<AnimationFrame> GetAnimationFrames(this AnimationFrameGenerator generator) =>
            ArgumentNullException.ThrowIfNull(generator) is null
                ? throw new ArgumentNullException(nameof(generator))
                : generator.GenerateDataFrames;

        /// <summary>
        /// Returns the numeric values produced by the generator as a read‑only list.
        /// </summary>
        /// <param name="generator">The <see cref="AnimationFrameGenerator"/> instance.</param>
        /// <returns>An <see cref="IReadOnlyList{Double}"/> of the generated values.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="generator"/> is <c>null</c>.</exception>
        public static IReadOnlyList<double> GetValues(this AnimationFrameGenerator generator) =>
            ArgumentNullException.ThrowIfNull(generator) is null
                ? throw new ArgumentNullException(nameof(generator))
                : generator.Values;
    }
}
