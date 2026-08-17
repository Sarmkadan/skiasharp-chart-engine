using SkiaSharp;
using System.Globalization;

namespace SkiasharpChartEngine.Extensions;

/// <summary>
/// Extension methods for <see cref="SKColor"/>.
/// </summary>
public static class SKColorExtensions
{
    /// <summary>
    /// Creates a new <see cref="SKColor"/> with the specified opacity.
    /// </summary>
    /// <param name="color">The base color.</param>
    /// <param name="opacity">The opacity value, clamped between 0 and 1.</param>
    /// <returns>A new <see cref="SKColor"/> with the applied opacity.</returns>
    public static SKColor WithOpacity(this SKColor color, float opacity)
    {
        var clampedOpacity = Math.Clamp(opacity, 0f, 1f);
        return new SKColor(color.Red, color.Green, color.Blue, (byte)(clampedOpacity * 255));
    }

    /// <summary>
    /// Darkens the color by the specified amount.
    /// </summary>
    /// <param name="color">The base color.</param>
    /// <param name="amount">The amount to darken, between 0 and 1.</param>
    /// <returns>A new, darkened <see cref="SKColor"/>.</returns>
    public static SKColor Darken(this SKColor color, float amount)
    {
        var factor = 1.0f - Math.Clamp(amount, 0f, 1f);
        return new SKColor(
            (byte)(color.Red * factor),
            (byte)(color.Green * factor),
            (byte)(color.Blue * factor),
            color.Alpha);
    }

    /// <summary>
    /// Lightens the color by the specified amount.
    /// </summary>
    /// <param name="color">The base color.</param>
    /// <param name="amount">The amount to lighten, between 0 and 1.</param>
    /// <returns>A new, lightened <see cref="SKColor"/>.</returns>
    public static SKColor Lighten(this SKColor color, float amount)
    {
        var factor = Math.Clamp(amount, 0f, 1f);
        return new SKColor(
            (byte)(color.Red + (255 - color.Red) * factor),
            (byte)(color.Green + (255 - color.Green) * factor),
            (byte)(color.Blue + (255 - color.Blue) * factor),
            color.Alpha);
    }

    /// <summary>
    /// Converts the <see cref="SKColor"/> to its hex representation.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>A hex string, e.g., #RRGGBB or #AARRGGBB if alpha is not 255.</returns>
    public static string ToHex(this SKColor color)
    {
        if (color.Alpha == 255)
        {
            return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
        }
        return $"#{color.Alpha:X2}{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
    }

    /// <summary>
    /// Attempts to parse a hex string into an <see cref="SKColor"/>.
    /// Supported formats: #RRGGBB, #AARRGGBB, RRGGBB, AARRGGBB.
    /// </summary>
    /// <param name="hex">The hex string to parse.</param>
    /// <param name="color">The resulting <see cref="SKColor"/>.</param>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    public static bool TryParseHex(string hex, out SKColor color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(hex)) return false;

        var s = hex.StartsWith("#") ? hex[1..] : hex;

        if (s.Length == 6) // RRGGBB
        {
            if (uint.TryParse(s, NumberStyles.HexNumber, null, out var val))
            {
                color = new SKColor((byte)(val >> 16), (byte)(val >> 8), (byte)val, 255);
                return true;
            }
        }
        else if (s.Length == 8) // AARRGGBB
        {
            if (uint.TryParse(s, NumberStyles.HexNumber, null, out var val))
            {
                color = new SKColor((byte)(val >> 16), (byte)(val >> 8), (byte)val, (byte)(val >> 24));
                return true;
            }
        }

        return false;
    }
}
