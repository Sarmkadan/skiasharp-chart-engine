// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq; // Added for .All() extension method

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Utility class for color operations and conversions
/// </summary>
public static class ColorHelper
{
    private static readonly string[] DefaultColors = new[]
    {
        "#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd",
        "#8c564b", "#e377c2", "#7f7f7f", "#bcbd22", "#17becf",
        "#aec7e8", "#ffbb78", "#98df8a", "#ff9896", "#c5b0d5"
    };

    public static string GetColorAtIndex(int index)
    {
        return DefaultColors[index % DefaultColors.Length];
    }

    public static List<string> GetDefaultColorPalette()
    {
        return new List<string>(DefaultColors);
    }

    public static string HexToRgb(string hexColor)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
            throw new ArgumentNullException(nameof(hexColor));

        if (!hexColor.StartsWith("#"))
            throw new ArgumentException("Color must be in hex format (#RRGGBB)");

        var hex = hexColor.Substring(1);
        if (hex.Length != 6 && hex.Length != 8)
            throw new ArgumentException("Color must be in #RRGGBB or #RRGGBBAA format");

        var r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        var g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        var b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        if (hex.Length == 8)
        {
            var a = int.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            return $"rgba({r}, {g}, {b}, {a / 255.0:F2})";
        }

        return $"rgb({r}, {g}, {b})";
    }

    public static string RgbToHex(int r, int g, int b)
    {
        if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255)
            throw new ArgumentException("RGB values must be between 0 and 255");

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public static string LightenColor(string hexColor, float factor = 0.2f)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
            throw new ArgumentNullException(nameof(hexColor));

        var hex = hexColor.StartsWith("#") ? hexColor.Substring(1) : hexColor;
        var r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        var g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        var b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        r = (int)Math.Min(255, r + (255 - r) * factor);
        g = (int)Math.Min(255, g + (255 - g) * factor);
        b = (int)Math.Min(255, b + (255 - b) * factor);

        return RgbToHex(r, g, b);
    }

    public static string DarkenColor(string hexColor, float factor = 0.2f)
    {
        if (string.IsNullOrWhiteSpace(hexColor))
            throw new ArgumentNullException(nameof(hexColor));

        var hex = hexColor.StartsWith("#") ? hexColor.Substring(1) : hexColor;
        var r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        var g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        var b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        r = (int)(r * (1 - factor));
        g = (int)(g * (1 - factor));
        b = (int)(b * (1 - factor));

        return RgbToHex(r, g, b);
    }

    public static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        if (!color.StartsWith("#"))
            return false;

        var hex = color.Substring(1);
        if (hex.Length != 6 && hex.Length != 8)
            return false;

        // Fix: Ensure all characters are valid hexadecimal digits (0-9, A-F, a-f).
        return hex.All(c => Uri.IsHexDigit(c));
    }
}
