// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Extensions;

/// <summary>
/// Extension methods for <see cref="DataPoint"/> operations including distance calculations,
/// coordinate transformations, and statistical analysis.
/// </summary>
public static class DataPointExtensions
{
    /// <summary>
    /// Calculates the Euclidean distance between two data points.
    /// </summary>
    /// <param name="point1">The first data point. Cannot be null.</param>
    /// <param name="point2">The second data point. Cannot be null.</param>
    /// <returns>The Euclidean distance between the two points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either point is null.</exception>
    public static double GetDistance(this DataPoint point1, DataPoint point2)
    {
        ArgumentNullException.ThrowIfNull(point1);
        ArgumentNullException.ThrowIfNull(point2);

        var deltaX = point1.X - point2.X;
        var deltaY = point1.Y - point2.Y;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    /// <summary>
    /// Determines whether the specified point is within the given tolerance distance from the target coordinates.
    /// </summary>
    /// <param name="point">The data point to check. Cannot be null.</param>
    /// <param name="x">The target X coordinate.</param>
    /// <param name="y">The target Y coordinate.</param>
    /// <param name="tolerance">The maximum allowed distance. Defaults to 0.1.</param>
    /// <returns>True if the point is within tolerance distance; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when point is null.</exception>
    public static bool IsNear(this DataPoint point, double x, double y, double tolerance = 0.1)
    {
        ArgumentNullException.ThrowIfNull(point);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tolerance);

        var distance = Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2));
        return distance <= tolerance;
    }

    /// <summary>
    /// Creates a new data point with coordinates offset by the specified values.
    /// </summary>
    /// <param name="point">The source data point. Cannot be null.</param>
    /// <param name="offsetX">The X-axis offset to apply.</param>
    /// <param name="offsetY">The Y-axis offset to apply.</param>
    /// <returns>A new <see cref="DataPoint"/> with offset coordinates.</returns>
    /// <exception cref="ArgumentNullException">Thrown when point is null.</exception>
    public static DataPoint Offset(this DataPoint point, double offsetX, double offsetY)
    {
        ArgumentNullException.ThrowIfNull(point);

        return new DataPoint(
            point.X + offsetX,
            point.Y + offsetY,
            point.Label,
            point.Color)
        {
            State = point.State,
            Metadata = point.Metadata != null
                ? new Dictionary<string, object>(point.Metadata)
                : null,
            Timestamp = point.Timestamp
        };
    }

    /// <summary>
    /// Creates a new data point with coordinates scaled by the specified factors.
    /// </summary>
    /// <param name="point">The source data point. Cannot be null.</param>
    /// <param name="scaleX">The X-axis scale factor.</param>
    /// <param name="scaleY">The Y-axis scale factor.</param>
    /// <returns>A new <see cref="DataPoint"/> with scaled coordinates.</returns>
    /// <exception cref="ArgumentNullException">Thrown when point is null.</exception>
    public static DataPoint Scale(this DataPoint point, double scaleX, double scaleY)
    {
        ArgumentNullException.ThrowIfNull(point);

        return new DataPoint(
            point.X * scaleX,
            point.Y * scaleY,
            point.Label,
            point.Color)
        {
            State = point.State,
            Metadata = point.Metadata != null
                ? new Dictionary<string, object>(point.Metadata)
                : null,
            Timestamp = point.Timestamp
        };
    }

    /// <summary>
    /// Applies an offset transformation to each data point in the collection.
    /// </summary>
    /// <param name="points">The collection of data points. Cannot be null.</param>
    /// <param name="offsetX">The X-axis offset to apply to each point.</param>
    /// <param name="offsetY">The Y-axis offset to apply to each point.</param>
    /// <returns>A new list containing the transformed data points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when points is null.</exception>
    public static List<DataPoint> Offset(this IEnumerable<DataPoint> points, double offsetX, double offsetY)
    {
        ArgumentNullException.ThrowIfNull(points);

        return points.Select(p => p.Offset(offsetX, offsetY)).ToList();
    }

    /// <summary>
    /// Applies a scaling transformation to each data point in the collection.
    /// </summary>
    /// <param name="points">The collection of data points. Cannot be null.</param>
    /// <param name="scaleX">The X-axis scale factor to apply to each point.</param>
    /// <param name="scaleY">The Y-axis scale factor to apply to each point.</param>
    /// <returns>A new list containing the transformed data points.</returns>
    /// <exception cref="ArgumentNullException">Thrown when points is null.</exception>
    public static List<DataPoint> Scale(this IEnumerable<DataPoint> points, double scaleX, double scaleY)
    {
        ArgumentNullException.ThrowIfNull(points);

        return points.Select(p => p.Scale(scaleX, scaleY)).ToList();
    }

    /// <summary>
    /// Calculates the bounding box that contains all data points in the collection.
    /// </summary>
    /// <param name="points">The collection of data points.</param>
    /// <returns>A tuple containing (minX, maxX, minY, maxY) representing the bounds.</returns>
    public static (double minX, double maxX, double minY, double maxY) GetBounds(this IEnumerable<DataPoint> points)
    {
        var pointList = points?.ToList() ?? new List<DataPoint>();

        if (pointList.Count == 0)
            return (0, 1, 0, 1);

        return (
            pointList.Min(p => p.X),
            pointList.Max(p => p.X),
            pointList.Min(p => p.Y),
            pointList.Max(p => p.Y)
        );
    }

    /// <summary>
    /// Calculates the average X coordinate across all data points in the collection.
    /// </summary>
    /// <param name="points">The collection of data points.</param>
    /// <returns>The average X coordinate, or 0 if the collection is empty.</returns>
    public static double GetAverageX(this IEnumerable<DataPoint> points)
    {
        var pointList = points?.ToList() ?? new List<DataPoint>();
        return pointList.Count == 0 ? 0 : pointList.Average(p => p.X);
    }

    /// <summary>
    /// Calculates the average Y coordinate across all data points in the collection.
    /// </summary>
    /// <param name="points">The collection of data points.</param>
    /// <returns>The average Y coordinate, or 0 if the collection is empty.</returns>
    public static double GetAverageY(this IEnumerable<DataPoint> points)
    {
        var pointList = points?.ToList() ?? new List<DataPoint>();
        return pointList.Count == 0 ? 0 : pointList.Average(p => p.Y);
    }
}