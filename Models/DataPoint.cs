// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.ComponentModel.DataAnnotations;
using SkiaSharpChartEngine.Constants;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Represents a single data point in a chart series
/// </summary>
public class DataPoint
{
    private double _x;
    private double _y;
    private string? _label;
    private string _color = ChartConstants.DefaultAxisColor;

    [Required]
    public double X
    {
        get => _x;
        set
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("X value cannot be NaN or Infinity");
            _x = value;
        }
    }

    [Required]
    public double Y
    {
        get => _y;
        set
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new ArgumentException("Y value cannot be NaN or Infinity");
            _y = value;
        }
    }

    public string? Label
    {
        get => _label;
        set => _label = value?.Trim();
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

    public DataPointState State { get; set; } = DataPointState.Normal;

    public double? CustomRadius { get; set; }

    public Dictionary<string, object>? Metadata { get; set; }

    public DateTime? Timestamp { get; set; }

    public DataPoint() { }

    public DataPoint(double x, double y)
    {
        X = x;
        Y = y;
    }

    public DataPoint(double x, double y, string? label = null, string? color = null)
    {
        X = x;
        Y = y;
        Label = label;
        if (color != null)
            Color = color;
    }

    private static bool IsValidHexColor(string color)
    {
        if (!color.StartsWith("#"))
            return false;

        var hex = color.Substring(1);
        return hex.Length == 6 || hex.Length == 8;
    }

    public override string ToString() => $"DataPoint(X={X}, Y={Y}, Label={Label})";

    public DataPoint Clone()
    {
        return new DataPoint(X, Y, Label, Color)
        {
            State = State,
            CustomRadius = CustomRadius,
            Metadata = Metadata != null ? new Dictionary<string, object>(Metadata) : null,
            Timestamp = Timestamp
        };
    }
}
