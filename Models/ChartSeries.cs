// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices; // Added for MethodImpl
using SkiaSharpChartEngine.Constants;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Represents a data series in a chart
/// </summary>
public class ChartSeries
{
    private readonly List<DataPoint> _dataPoints = new();
    private string _name = string.Empty;
    private string _color = "#1F77B4";

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name
    {
        get => _name;
        set => _name = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
    }

    [StringLength(9, MinimumLength = 7)]
    public string Color
    {
        get => _color;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || !IsValidHexColor(value))
                throw new ArgumentException("Invalid hex color format");
            _color = value;
        }
    }

    public List<DataPoint> DataPoints
    {
        get => _dataPoints;
    }

    public ChartType SeriesType { get; set; } = ChartType.LineChart;

    public float LineWidth { get; set; } = 2.0f;

    public bool IsVisible { get; set; } = true;

    public double? YAxisMin { get; set; }

    public double? YAxisMax { get; set; }

    public string? Description { get; set; }

    public int? ZIndex { get; set; }

    public Dictionary<string, object>? CustomProperties { get; set; }

    public ChartSeries() { }

    public ChartSeries(string name)
    {
        Name = name;
    }

    public ChartSeries(string name, string color, ChartType seriesType = ChartType.LineChart)
    {
        Name = name;
        Color = color;
        SeriesType = seriesType;
    }

    public void AddDataPoint(DataPoint point)
    {
        if (point == null)
            throw new ArgumentNullException(nameof(point));

        _dataPoints.Add(point);
    }

    public void AddDataPoint(double x, double y, string? label = null)
    {
        _dataPoints.Add(new DataPoint(x, y, label));
    }

    public void AddDataPoints(IEnumerable<DataPoint> points)
    {
        if (points == null)
            throw new ArgumentNullException(nameof(points));

        _dataPoints.AddRange(points);
    }

    public void RemoveDataPoint(int index)
    {
        if (index < 0 || index >= _dataPoints.Count)
            throw new IndexOutOfRangeException($"Index {index} is out of range");

        _dataPoints.RemoveAt(index);
    }

    public void ClearDataPoints()
    {
        _dataPoints.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Fix: Added for performance optimization
    public int GetDataPointCount() => _dataPoints.Count;

    public (double min, double max) GetYAxisRange()
    {
        if (_dataPoints.Count == 0)
            return (0, 1);

        var min = _dataPoints.Min(p => p.Y);
        var max = _dataPoints.Max(p => p.Y);
        return (min, max);
    }

    public (double min, double max) GetXAxisRange()
    {
        if (_dataPoints.Count == 0)
            return (0, 1);

        var min = _dataPoints.Min(p => p.X);
        var max = _dataPoints.Max(p => p.X);
        return (min, max);
    }

    private static bool IsValidHexColor(string color)
    {
        if (!color.StartsWith("#"))
            return false;

        var hex = color.Substring(1);
        return hex.Length == 6 || hex.Length == 8;
    }

    public ChartSeries Clone()
    {
        var cloned = new ChartSeries(Name, Color, SeriesType)
        {
            LineWidth = LineWidth,
            IsVisible = IsVisible,
            YAxisMin = YAxisMin,
            YAxisMax = YAxisMax,
            Description = Description,
            ZIndex = ZIndex,
            CustomProperties = CustomProperties != null ? new Dictionary<string, object>(CustomProperties) : null
        };

        foreach (var point in _dataPoints)
        {
            cloned.AddDataPoint(point.Clone());
        }

        return cloned;
    }

    public override string ToString() => $"ChartSeries(Name={Name}, Points={_dataPoints.Count})";
}
