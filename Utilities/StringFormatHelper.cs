// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// String formatting and transformation utilities
/// Provides methods for formatting numbers, currencies, and chart labels
/// </summary>
public static class StringFormatHelper
{
    /// <summary>
    /// Formats a number with optional units (K, M, B for thousands, millions, billions)
    /// Used for axis labels with large numbers
    /// </summary>
    public static string FormatNumberWithUnits(double value, int decimalPlaces = 1)
    {
        var absValue = Math.Abs(value);

        return absValue switch
        {
            >= 1_000_000_000 => $"{value / 1_000_000_000:F{decimalPlaces}}B",
            >= 1_000_000 => $"{value / 1_000_000:F{decimalPlaces}}M",
            >= 1_000 => $"{value / 1_000:F{decimalPlaces}}K",
            _ => $"{value:F{decimalPlaces}}"
        };
    }

    /// <summary>
    /// Formats a number as currency
    /// </summary>
    public static string FormatCurrency(double value, string currencySymbol = "$")
    {
        return $"{currencySymbol}{value:N2}";
    }

    /// <summary>
    /// Formats a number as percentage
    /// </summary>
    public static string FormatPercentage(double value, int decimalPlaces = 0)
    {
        return $"{value:F{decimalPlaces}}%";
    }

    /// <summary>
    /// Truncates text to specified length and adds ellipsis
    /// Useful for long chart labels
    /// </summary>
    public static string TruncateWithEllipsis(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text[..(maxLength - 3)] + "...";
    }

    /// <summary>
    /// Converts camelCase to Title Case
    /// Useful for formatting property names as labels
    /// </summary>
    public static string CamelCaseToTitleCase(string camelCase)
    {
        if (string.IsNullOrEmpty(camelCase))
            return camelCase;

        var result = new StringBuilder();
        result.Append(char.ToUpper(camelCase[0]));

        for (int i = 1; i < camelCase.Length; i++)
        {
            if (char.IsUpper(camelCase[i]))
            {
                result.Append(' ');
            }
            result.Append(camelCase[i]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts snake_case to Title Case
    /// Useful for formatting identifiers as labels
    /// </summary>
    public static string SnakeCaseToTitleCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
            return snakeCase;

        var parts = snakeCase.Split('_');
        var titleParts = parts.Select(p =>
            char.ToUpper(p[0]) + (p.Length > 1 ? p[1..] : string.Empty));

        return string.Join(" ", titleParts);
    }

    /// <summary>
    /// Sanitizes string for safe display (removes special characters if needed)
    /// </summary>
    public static string Sanitize(string text, bool allowSpecialChars = true)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        if (allowSpecialChars)
            return text;

        return Regex.Replace(text, @"[^a-zA-Z0-9\s\-.]", "");
    }

    /// <summary>
    /// Pads a string to align with other strings (right-aligned numbers)
    /// </summary>
    public static string PadForAlignment(string text, int width, char padChar = ' ', bool rightAlign = true)
    {
        if (rightAlign)
            return text.PadLeft(width, padChar);

        return text.PadRight(width, padChar);
    }

    /// <summary>
    /// Formats elapsed time in human-readable format
    /// </summary>
    public static string FormatTimespan(TimeSpan timespan)
    {
        return timespan.TotalSeconds switch
        {
            < 1 => $"{timespan.TotalMilliseconds:F0}ms",
            < 60 => $"{timespan.TotalSeconds:F1}s",
            < 3600 => $"{timespan.TotalMinutes:F1}m",
            < 86400 => $"{timespan.TotalHours:F1}h",
            _ => $"{timespan.TotalDays:F1}d"
        };
    }

    /// <summary>
    /// Repeats a string multiple times
    /// Useful for generating separator lines
    /// </summary>
    public static string Repeat(string text, int count)
    {
        if (string.IsNullOrEmpty(text) || count <= 0)
            return string.Empty;

        var sb = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            sb.Append(text);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Generates a CSV line from values
    /// </summary>
    public static string ToCsvLine(params object?[] values)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0) sb.Append(',');

            var value = values[i]?.ToString() ?? string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                sb.Append('"').Append(value.Replace("\"", "\"\"")).Append('"');
            }
            else
            {
                sb.Append(value);
            }
        }
        return sb.ToString();
    }
}
