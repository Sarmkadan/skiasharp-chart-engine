// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SkiaSharpChartEngine.Models;

namespace SkiaSharpChartEngine.Extensions;

/// <summary>
/// Extension methods for DataPoint operations
/// </summary>
public static class DataPointExtensions
{
    public static double GetDistance(this DataPoint point1, DataPoint point2)
    {
        if (point1 == null || point2 == null)
            throw new ArgumentNullException("Both points must be non-null");

        var deltaX = point1.X - point2.X;
        var deltaY = point1.Y - point2.Y;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }

    public static bool IsNear(this DataPoint point, double x, double y, double tolerance = 0.1)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        var distance = Math.Sqrt(
            Math.Pow(point.X - x, 2) +
            Math.Pow(point.Y - y, 2)
        );

        return distance <= tolerance;
    }

    public static DataPoint Offset(this DataPoint point, double offsetX, double offsetY)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        return new DataPoint(point.X + offsetX, point.Y + offsetY, point.Label, point.Color)
        {
            State = point.State,
            Metadata = point.Metadata != null ? new Dictionary<string, object>(point.Metadata) : null,
            Timestamp = point.Timestamp
        };
    }

    public static DataPoint Scale(this DataPoint point, double scaleX, double scaleY)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        return new DataPoint(point.X * scaleX, point.Y * scaleY, point.Label, point.Color)
        {
            State = point.State,
            Metadata = point.Metadata != null ? new Dictionary<string, object>(point.Metadata) : null,
            Timestamp = point.Timestamp
        };
    }

    public static List<DataPoint> Offset(this IEnumerable<DataPoint> points, double offsetX, double offsetY)
    {
        if (points == null)
            throw new ArgumentNullException(nameof(points));

        return points.Select(p => p.Offset(offsetX, offsetY)).ToList();
    }

    public static List<DataPoint> Scale(this IEnumerable<DataPoint> points, double scaleX, double scaleY)
    {
        if (points == null)
            throw new ArgumentNullException(nameof(points));

        return points.Select(p => p.Scale(scaleX, scaleY)).ToList();
    }

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

    public static double GetAverageX(this IEnumerable<DataPoint> points)
    {
        var pointList = points?.ToList() ?? new List<DataPoint>();
        return pointList.Count == 0 ? 0 : pointList.Average(p => p.X);
    }

    public static double GetAverageY(this IEnumerable<DataPoint> points)
    {
        var pointList = points?.ToList() ?? new List<DataPoint>();
        return pointList.Count == 0 ? 0 : pointList.Average(p => p.Y);
    }
}
