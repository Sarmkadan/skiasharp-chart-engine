// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Color palette for chart series
/// </summary>
public class ColorPalette
{
    private readonly List<string> _colors = new();

    public string Name { get; set; }

    public IReadOnlyList<string> Colors => _colors.AsReadOnly();

    public ColorPalette(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public ColorPalette(string name, IEnumerable<string> colors) : this(name)
    {
        if (colors != null)
        {
            _colors.AddRange(colors);
        }
    }

    public void AddColor(string hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
            throw new ArgumentNullException(nameof(hexColor));

        if (!hexColor.StartsWith("#") || (hexColor.Length != 7 && hexColor.Length != 9))
            throw new ArgumentException("Color must be in #RRGGBB or #RRGGBBAA format");

        _colors.Add(hexColor);
    }

    public string GetColorAtIndex(int index)
    {
        if (_colors.Count == 0)
            throw new InvalidOperationException("Palette contains no colors");

        return _colors[index % _colors.Count];
    }

    public string GetNextColor(ref int colorIndex)
    {
        if (_colors.Count == 0)
            throw new InvalidOperationException("Palette contains no colors");

        var color = _colors[colorIndex % _colors.Count];
        colorIndex++;
        return color;
    }

    public int GetColorCount() => _colors.Count;

    public override string ToString() => $"ColorPalette(Name={Name}, Colors={_colors.Count})";

    public static ColorPalette CreateDefaultPalette()
    {
        return new ColorPalette("Default", new[]
        {
            "#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd",
            "#8c564b", "#e377c2", "#7f7f7f", "#bcbd22", "#17becf"
        });
    }

    public static ColorPalette CreateVibrantPalette()
    {
        return new ColorPalette("Vibrant", new[]
        {
            "#FF6B6B", "#4ECDC4", "#45B7D1", "#FFA07A", "#98D8C8",
            "#F7DC6F", "#BB8FCE", "#85C1E2", "#F8B739", "#52C47F"
        });
    }

    public static ColorPalette CreatePastelPalette()
    {
        return new ColorPalette("Pastel", new[]
        {
            "#FFB3BA", "#FFCCCB", "#FFFFBA", "#BAC7FF", "#FFB3D9",
            "#C7CEEA", "#B5EAD7", "#FFDAC1", "#E0BBE4", "#D4F1F4"
        });
    }

    public static ColorPalette CreateMonochromePalette()
    {
        return new ColorPalette("Monochrome", new[]
        {
            "#2C3E50", "#34495E", "#7F8C8D", "#95A5A6", "#BDC3C7",
            "#ECF0F1", "#DDDDDD", "#CCCCCC", "#BBBBBB", "#AAAAAA"
        });
    }

    public static ColorPalette CreateOceanPalette()
    {
        return new ColorPalette("Ocean", new[]
        {
            "#0B3D91", "#0E5F8F", "#138D89", "#06A77D", "#00A878",
            "#56CCF2", "#2196F3", "#1976D2", "#1565C0", "#0D47A1"
        });
    }
}
