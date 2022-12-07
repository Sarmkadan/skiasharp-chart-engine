// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Manages color schemes and palettes for charts.
/// Provides predefined schemes and color harmony algorithms.
/// </summary>
public class ColorSchemeManager
{
    private readonly Dictionary<string, ColorScheme> _schemes;
    private readonly ILogger<ColorSchemeManager> _logger;

    public ColorSchemeManager(ILogger<ColorSchemeManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _schemes = new Dictionary<string, ColorScheme>(StringComparer.OrdinalIgnoreCase);
        _initializeDefaultSchemes();
    }

    // Register color scheme
    public void RegisterScheme(string name, ColorScheme scheme)
    {
        if (string.IsNullOrWhiteSpace(name) || scheme == null)
            return;

        _schemes[name] = scheme;
        _logger.LogInformation("Color scheme registered: {SchemeName}", name);
    }

    // Get scheme by name
    public ColorScheme GetScheme(string name)
    {
        if (_schemes.TryGetValue(name, out var scheme))
        {
            return scheme;
        }

        _logger.LogWarning("Color scheme not found: {SchemeName}", name);
        return _schemes["default"];
    }

    // Get color at index
    public SKColor GetColor(string schemeName, int index)
    {
        var scheme = GetScheme(schemeName);
        if (scheme?.Colors == null || scheme.Colors.Length == 0)
            return SKColors.Gray;

        return scheme.Colors[index % scheme.Colors.Length];
    }

    // Generate complementary color
    public SKColor GetComplementary(SKColor color)
    {
        var hsl = _rgbToHsl(color.Red, color.Green, color.Blue);
        hsl.h = (hsl.h + 180) % 360;
        return _hslToRgb(hsl.h, hsl.s, hsl.l);
    }

    // Generate analogous colors
    public SKColor[] GetAnalogous(SKColor color, int count = 3)
    {
        var hsl = _rgbToHsl(color.Red, color.Green, color.Blue);
        var colors = new SKColor[count];
        var angleStep = 360.0 / count;

        for (int i = 0; i < count; i++)
        {
            var newHue = (hsl.h + (angleStep * i)) % 360;
            colors[i] = _hslToRgb(newHue, hsl.s, hsl.l);
        }

        return colors;
    }

    // Generate triadic colors
    public SKColor[] GetTriadic(SKColor color)
    {
        var hsl = _rgbToHsl(color.Red, color.Green, color.Blue);
        return new[]
        {
            color,
            _hslToRgb((hsl.h + 120) % 360, hsl.s, hsl.l),
            _hslToRgb((hsl.h + 240) % 360, hsl.s, hsl.l)
        };
    }

    // List available schemes
    public IEnumerable<string> ListAvailableSchemes() => _schemes.Keys;

    private void _initializeDefaultSchemes()
    {
        // Viridis colormap
        _schemes["viridis"] = new ColorScheme
        {
            Name = "Viridis",
            Description = "Perceptually uniform colormap",
            Colors = new[]
            {
                new SKColor(68, 1, 84),
                new SKColor(71, 16, 99),
                new SKColor(72, 35, 116),
                new SKColor(71, 58, 131),
                new SKColor(67, 82, 140),
                new SKColor(59, 106, 143),
                new SKColor(48, 130, 142),
                new SKColor(36, 152, 138),
                new SKColor(25, 172, 132),
                new SKColor(39, 189, 123),
                new SKColor(93, 201, 99),
                new SKColor(253, 231, 37)
            }
        };

        // Material design colors
        _schemes["material"] = new ColorScheme
        {
            Name = "Material Design",
            Description = "Google Material Design colors",
            Colors = new[]
            {
                new SKColor(244, 67, 54),    // Red
                new SKColor(233, 30, 99),    // Pink
                new SKColor(156, 39, 176),   // Purple
                new SKColor(63, 81, 181),    // Indigo
                new SKColor(33, 150, 243),   // Blue
                new SKColor(0, 188, 212),    // Cyan
                new SKColor(0, 150, 136),    // Teal
                new SKColor(76, 175, 80),    // Green
                new SKColor(205, 220, 57),   // Lime
                new SKColor(255, 193, 7),    // Amber
                new SKColor(255, 152, 0),    // Orange
                new SKColor(255, 87, 34)     // Deep Orange
            }
        };

        // Pastel colors
        _schemes["pastel"] = new ColorScheme
        {
            Name = "Pastel",
            Description = "Soft pastel colors",
            Colors = new[]
            {
                new SKColor(255, 179, 186),
                new SKColor(255, 223, 186),
                new SKColor(255, 250, 200),
                new SKColor(186, 255, 201),
                new SKColor(186, 225, 255),
                new SKColor(220, 198, 224)
            }
        };

        _schemes["default"] = _schemes["material"];

        _logger.LogInformation("Default color schemes initialized: {Count} schemes", _schemes.Count);
    }

    private (double h, double s, double l) _rgbToHsl(byte r, byte g, byte b)
    {
        var rf = r / 255.0;
        var gf = g / 255.0;
        var bf = b / 255.0;

        var max = Math.Max(Math.Max(rf, gf), bf);
        var min = Math.Min(Math.Min(rf, gf), bf);
        var l = (max + min) / 2;

        if (max == min)
            return (0, 0, l);

        var d = max - min;
        var s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

        double h = 0;
        if (max == rf) h = ((gf - bf) / d + (gf < bf ? 6 : 0)) / 6;
        else if (max == gf) h = ((bf - rf) / d + 2) / 6;
        else if (max == bf) h = ((rf - gf) / d + 4) / 6;

        return (h * 360, s, l);
    }

    private SKColor _hslToRgb(double h, double s, double l)
    {
        double r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            r = _hueToRgb(p, q, h / 360 + 1 / 3);
            g = _hueToRgb(p, q, h / 360);
            b = _hueToRgb(p, q, h / 360 - 1 / 3);
        }

        return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
    }

    private double _hueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1 / 6) return p + (q - p) * 6 * t;
        if (t < 1 / 2) return q;
        if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
        return p;
    }
}

/// <summary>
/// Represents a color scheme for charts.
/// </summary>
public class ColorScheme
{
    public string Name { get; set; }
    public string Description { get; set; }
    public SKColor[] Colors { get; set; }
}
