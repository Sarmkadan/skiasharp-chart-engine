// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Aggregated data container for a chart series, providing statistical
/// summaries and transformation utilities over raw <see cref="DataPoint"/> collections.
/// </summary>
public class SeriesData : IDisposable
{
    private readonly ILogger<SeriesData> _logger;
    private readonly List<DataPoint> _points = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="SeriesData"/>.
    /// </summary>
    /// <param name="logger">Logger instance for diagnostics.</param>
    public SeriesData(ILogger<SeriesData> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the read-only collection of data points in this series.
    /// </summary>
    public IReadOnlyList<DataPoint> Points => _points.AsReadOnly();

    /// <summary>
    /// Adds a collection of data points, discarding duplicates by X value.
    /// </summary>
    /// <param name="points">The data points to add.</param>
    /// <returns>The number of points actually inserted.</returns>
    public int AddRange(IEnumerable<DataPoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);
        var existingX = new HashSet<double>(_points.Select(p => p.X));
        int inserted = 0;

        foreach (var point in points)
        {
            if (existingX.Add(point.X))
            {
                _points.Add(point);
                inserted++;
            }
        }

        _logger.LogDebug("Added {Inserted} points, {Skipped} duplicates skipped",
            inserted, _points.Count - inserted);
        return inserted;
    }

    /// <summary>
    /// Computes the min, max, mean, and standard deviation for the Y values.
    /// </summary>
    /// <returns>A tuple of (Min, Max, Mean, StdDev), or null if no data.</returns>
    public (double Min, double Max, double Mean, double StdDev)? ComputeStatistics()
    {
        if (_points.Count == 0)
            return null;

        double min = double.MaxValue, max = double.MinValue, sum = 0;
        foreach (var p in _points)
        {
            if (p.Y < min) min = p.Y;
            if (p.Y > max) max = p.Y;
            sum += p.Y;
        }

        double mean = sum / _points.Count;
        double variance = _points.Sum(p => (p.Y - mean) * (p.Y - mean)) / _points.Count;

        return (min, max, mean, Math.Sqrt(variance));
    }

    /// <summary>
    /// Returns a subset of points whose X values fall within the specified range (inclusive).
    /// </summary>
    /// <param name="xMin">Lower bound of X.</param>
    /// <param name="xMax">Upper bound of X.</param>
    /// <returns>Filtered list of data points.</returns>
    public List<DataPoint> SliceByRange(double xMin, double xMax)
    {
        if (xMin > xMax)
            throw new ArgumentException("xMin must be <= xMax");

        return _points.Where(p => p.X >= xMin && p.X <= xMax).ToList();
    }

    /// <summary>
    /// Normalizes all Y values to the 0-1 range based on min/max.
    /// </summary>
    /// <returns>A new list of data points with normalized Y values.</returns>
    public List<DataPoint> NormalizeY()
    {
        var stats = ComputeStatistics();
        if (stats is null)
            return new List<DataPoint>();

        double range = stats.Value.Max - stats.Value.Min;
        if (range == 0)
        {
            _logger.LogWarning("All Y values are identical; normalization yields zeros");
            return _points.Select(p =>
            {
                var dp = new DataPoint { X = p.X, Y = 0 };
                return dp;
            }).ToList();
        }

        return _points.Select(p =>
        {
            var dp = new DataPoint { X = p.X, Y = (p.Y - stats.Value.Min) / range };
            return dp;
        }).ToList();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _points.Clear();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
