// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Extension methods for <see cref="DataPoint"/> providing additional spatial helpers.
/// </summary>
public static class DataPointExtensions
{
    /// <summary>
    /// Determines whether the data point's X and Y coordinates fall within the given bounds (inclusive).
    /// </summary>
    /// <param name="point">The data point to check. Cannot be null.</param>
    /// <param name="minX">Minimum allowed X value.</param>
    /// <param name="maxX">Maximum allowed X value.</param>
    /// <param name="minY">Minimum allowed Y value.</param>
    /// <param name="maxY">Maximum allowed Y value.</param>
    /// <returns>True if <c>minX &lt;= X &lt;= maxX</c> and <c>minY &lt;= Y &lt;= maxY</c>; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="point"/> is null.</exception>
    public static bool IsWithinRange(this DataPoint point, double minX, double maxX, double minY, double maxY)
    {
        ArgumentNullException.ThrowIfNull(point);

        return point.X >= minX && point.X <= maxX
            && point.Y >= minY && point.Y <= maxY;
    }
}
