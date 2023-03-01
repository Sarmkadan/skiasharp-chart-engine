using System;
using SkiaSharp;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Extension methods for <see cref="ColorSchemeManager"/> providing additional color utility functions.
/// </summary>
public static class ColorSchemeManagerExtensions
{
    /// <summary>
    /// Gets a color from a scheme with optional alpha/transparency value.
    /// </summary>
    /// <param nameof the color=""/>
    /// <param name="schemeName">Name of the color scheme.</param>
    /// <param name="index">Index of the color in the scheme.</param>
    /// <param name="alpha">Alpha/transparency value (0-255). If null, uses the color's original alpha.</param>
    /// <returns>The color from the scheme with the specified alpha value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="schemeName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="schemeName"/> is empty or whitespace.</exception>
    public static SKColor GetColorWithAlpha(this ColorSchemeManager manager, string schemeName, int index, byte? alpha = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(schemeName);

        var color = manager.GetColor(schemeName, index);
        return alpha.HasValue ? new SKColor(color.Red, color.Green, color.Blue, alpha.Value) : color;
    }

    /// <summary>
    /// Gets an array of colors representing a gradient between two colors in a scheme.
    /// </summary>
    /// <param name="manager">The <see cref="ColorSchemeManager"/> instance.</param>
    /// <param name="schemeName">Name of the color scheme.</param>
    /// <param name="startIndex">Index of the start color in the scheme.</param>
    /// <param name="endIndex">Index of the end color in the scheme.</param>
    /// <param name="steps">Number of gradient steps (including start and end colors). Must be at least 2.</param>
    /// <returns>An array of colors forming a gradient from start to end color.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="schemeName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="schemeName"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="steps"/> is less than 2.</exception>
    public static SKColor[] GetGradientColors(this ColorSchemeManager manager, string schemeName, int startIndex, int endIndex, int steps = 10)
    {
        ArgumentException.ThrowIfNullOrEmpty(schemeName);
        if (steps < 2)
            throw new ArgumentOutOfRangeException(nameof(steps), "Steps must be at least 2.");

        var startColor = manager.GetColor(schemeName, startIndex);
        var endColor = manager.GetColor(schemeName, endIndex);

        var gradient = new SKColor[steps];
        for (int i = 0; i < steps; i++)
        {
            float t = i / (float)(steps - 1);
            byte r = (byte)(startColor.Red + t * (endColor.Red - startColor.Red));
            byte g = (byte)(startColor.Green + t * (endColor.Green - startColor.Green));
            byte b = (byte)(startColor.Blue + t * (endColor.Blue - startColor.Blue));
            byte a = (byte)(startColor.Alpha + t * (endColor.Alpha - startColor.Alpha));
            gradient[i] = new SKColor(r, g, b, a);
        }

        return gradient;
    }

    /// <summary>
    /// Checks if a color scheme with the specified name exists in the manager.
    /// </summary>
    /// <param name="manager">The <see cref="ColorSchemeManager"/> instance.</param>
    /// <param name="schemeName">Name of the color scheme to check.</param>
    /// <returns>True if the scheme exists; otherwise, false.</returns>
    public static bool HasScheme(this ColorSchemeManager manager, string schemeName)
    {
        if (string.IsNullOrWhiteSpace(schemeName))
            return false;

        return manager.ListAvailableSchemes().Any(name => string.Equals(name, schemeName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a color scheme with a fallback to a specified alternative scheme if the requested scheme is not found.
    /// </summary>
    /// <param name="manager">The <see cref="ColorSchemeManager"/> instance.</param>
    /// <param name="schemeName">Name of the color scheme to retrieve.</param>
    /// <param name="fallbackSchemeName">Name of the fallback scheme to use if the requested scheme is not found.</param>
    /// <returns>The requested color scheme if found; otherwise, the fallback scheme.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="schemeName"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="schemeName"/> or <paramref name="fallbackSchemeName"/> is empty or whitespace.</exception>
    public static ColorScheme GetSchemeOrFallback(this ColorSchemeManager manager, string schemeName, string fallbackSchemeName = "default")
    {
        ArgumentException.ThrowIfNullOrEmpty(schemeName);
        ArgumentException.ThrowIfNullOrEmpty(fallbackSchemeName);

        if (manager.HasScheme(schemeName))
        {
            return manager.GetScheme(schemeName);
        }

        return manager.GetScheme(fallbackSchemeName);
    }
}