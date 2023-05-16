// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.ObjectPool;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// String formatting and transformation utilities.
/// Provides methods for formatting numbers, currencies, and chart labels.
/// </summary>
public class StringFormatHelper
{
    // Shared pool reduces StringBuilder allocations in high-frequency call paths.
    private static readonly ObjectPool<StringBuilder> _sbPool =
        new DefaultObjectPoolProvider().CreateStringBuilderPool(initialCapacity: 256, maximumRetainedCapacity: 4096);

    /// <summary>
    /// Formats a number with optional units (K, M, B for thousands, millions, billions).
    /// Used for axis labels with large numbers.
    /// </summary>
    public static string FormatNumberWithUnits(double value, int decimalPlaces = 1)
    {
        var absValue = Math.Abs(value);

        var fmt = $"F{decimalPlaces}";
        return absValue switch
        {
            >= 1_000_000_000 => (value / 1_000_000_000).ToString(fmt, CultureInfo.InvariantCulture) + "B",
            >= 1_000_000 => (value / 1_000_000).ToString(fmt, CultureInfo.InvariantCulture) + "M",
            >= 1_000 => (value / 1_000).ToString(fmt, CultureInfo.InvariantCulture) + "K",
            _ => value.ToString(fmt, CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Formats a number as currency.
    /// </summary>
    public static string FormatCurrency(double value, string currencySymbol = "$")
    {
        return currencySymbol + value.ToString("N2", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a number as percentage.
    /// </summary>
    public static string FormatPercentage(double value, int decimalPlaces = 0)
    {
        return value.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture) + "%";
    }

    /// <summary>
    /// Truncates text to specified length and adds ellipsis.
    /// Useful for long chart labels.
    /// </summary>
    public static string TruncateWithEllipsis(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text[..(maxLength - 3)] + "...";
    }

    /// <summary>
    /// Converts camelCase to Title Case using string.Create to avoid StringBuilder allocation.
    /// </summary>
    public static string CamelCaseToTitleCase(string camelCase)
    {
        if (string.IsNullOrEmpty(camelCase))
            return camelCase;

        // Count extra spaces needed so string.Create can pre-size the result.
        int extraSpaces = 0;
        for (int i = 1; i < camelCase.Length; i++)
            if (char.IsUpper(camelCase[i])) extraSpaces++;

        if (extraSpaces == 0)
        {
            // Only need to capitalize first character.
            if (char.IsUpper(camelCase[0])) return camelCase;
            return string.Create(camelCase.Length, camelCase, static (span, src) =>
            {
                src.AsSpan().CopyTo(span);
                span[0] = char.ToUpper(span[0]);
            });
        }

        return string.Create(camelCase.Length + extraSpaces, camelCase, static (span, src) =>
        {
            span[0] = char.ToUpper(src[0]);
            int pos = 1;
            for (int i = 1; i < src.Length; i++)
            {
                if (char.IsUpper(src[i]))
                    span[pos++] = ' ';
                span[pos++] = src[i];
            }
        });
    }

    /// <summary>
    /// Converts snake_case to Title Case using string.Create — avoids Split + LINQ allocations.
    /// </summary>
    public static string SnakeCaseToTitleCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
            return snakeCase;

        // Output length equals input length: every '_' becomes ' ', every other char is kept.
        return string.Create(snakeCase.Length, snakeCase, static (span, src) =>
        {
            bool capitalizeNext = true;
            int pos = 0;
            foreach (char c in src.AsSpan())
            {
                if (c == '_')
                {
                    span[pos++] = ' ';
                    capitalizeNext = true;
                }
                else
                {
                    span[pos++] = capitalizeNext ? char.ToUpper(c) : c;
                    capitalizeNext = false;
                }
            }
        });
    }

    /// <summary>
    /// Sanitizes string for safe display (removes special characters if needed).
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
    /// Pads a string to align with other strings (right-aligned numbers).
    /// </summary>
    public static string PadForAlignment(string text, int width, char padChar = ' ', bool rightAlign = true)
    {
        if (rightAlign)
            return text.PadLeft(width, padChar);

        return text.PadRight(width, padChar);
    }

    /// <summary>
    /// Formats elapsed time in human-readable format.
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
    /// Repeats a string multiple times using a pooled StringBuilder.
    /// </summary>
    public static string Repeat(string text, int count)
    {
        if (string.IsNullOrEmpty(text) || count <= 0)
            return string.Empty;

        var sb = _sbPool.Get();
        try
        {
            sb.EnsureCapacity(text.Length * count);
            for (int i = 0; i < count; i++)
                sb.Append(text);
            return sb.ToString();
        }
        finally
        {
            sb.Clear();
            _sbPool.Return(sb);
        }
    }

    /// <summary>
    /// Generates a CSV line from values using a pooled StringBuilder.
    /// </summary>
    public static string ToCsvLine(params object?[] values)
    {
        var sb = _sbPool.Get();
        try
        {
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
        finally
        {
            sb.Clear();
            _sbPool.Return(sb);
        }
    }
}
