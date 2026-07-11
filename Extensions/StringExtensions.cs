// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkiaSharpChartEngine.Extensions;

/// <summary>
/// Extension methods for string manipulation and formatting.
/// Provides utilities for parsing, validation, and text transformation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts a string to kebab-case (dash-separated lowercase).
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The kebab-case representation, or empty string if input is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToKebabCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length == 0)
            return input;

        var pattern = new Regex(@"[A-Z]");
        var kebab = pattern.Replace(input, m => "-" + m.Value.ToLowerInvariant());
        return kebab.TrimStart('-');
    }

    /// <summary>
    /// Converts a string to snake_case (underscore-separated lowercase).
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The snake_case representation, or empty string if input is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToSnakeCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length == 0)
            return input;

        var pattern = new Regex(@"[A-Z]");
        var snake = pattern.Replace(input, m => "_" + m.Value.ToLowerInvariant());
        return snake.TrimStart('_');
    }

    /// <summary>
    /// Converts a string to PascalCase (FirstLetterCapitalized).
    /// </summary>
    /// <param name="input">The string to convert.</param>
    /// <returns>The PascalCase representation, or empty string if input is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToPascalCase(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Length == 0)
            return input;

        var textInfo = CultureInfo.InvariantCulture.TextInfo;

        // Split on spaces, hyphens, and underscores
        var words = Regex.Split(input, @"[\s\-_]+")
            .Where(w => !string.IsNullOrEmpty(w))
            .Select(w => textInfo.ToTitleCase(w.ToLowerInvariant()));

        return string.Concat(words);
    }

    /// <summary>
    /// Safely parses a string to a double with a fallback value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <param name="defaultValue">The value to return if parsing fails.</param>
    /// <returns>The parsed double value or the default value if parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static double ToDoubleOrDefault(this string input, double defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(input);

        return double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : defaultValue;
    }

    /// <summary>
    /// Safely parses a string to an integer with a fallback value.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <param name="defaultValue">The value to return if parsing fails.</param>
    /// <returns>The parsed integer value or the default value if parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static int ToIntOrDefault(this string input, int defaultValue = 0)
    {
        ArgumentNullException.ThrowIfNull(input);

        return int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : defaultValue;
    }

    /// <summary>
    /// Truncates a string to a maximum length with an optional suffix.
    /// </summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string.</param>
    /// <param name="suffix">The suffix to append when truncating. Defaults to "...".</param>
    /// <returns>The truncated string, or the original string if it's shorter than maxLength.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is less than 0.</exception>
    public static string Truncate(this string input, int maxLength, string suffix = "...")
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        ArgumentNullException.ThrowIfNull(suffix);

        if (input.Length <= maxLength)
            return input;

        if (suffix.Length >= maxLength)
            return input.Substring(0, maxLength);

        return input.Substring(0, maxLength - suffix.Length) + suffix;
    }

    /// <summary>
    /// Checks if a string contains only digits.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>True if the string contains only digits; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static bool IsNumeric(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return input.All(char.IsDigit);
    }

    /// <summary>
    /// Checks if a string is a valid hexadecimal color.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid hex color; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static bool IsValidHexColor(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var hexPattern = new Regex(@"^#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$");
        return hexPattern.IsMatch(input);
    }

    /// <summary>
    /// Extracts all numeric sequences from a string.
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <returns>A string containing all numeric sequences concatenated together.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ExtractNumbers(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return string.Concat(Regex.Matches(input, @"[0-9]+").Cast<Match>().Select(m => m.Value));
    }

    /// <summary>
    /// Removes all whitespace characters from a string.
    /// </summary>
    /// <param name="input">The string to process.</param>
    /// <returns>The string with all whitespace removed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string RemoveWhitespace(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return Regex.Replace(input, @"\s+", "");
    }

    /// <summary>
    /// Capitalizes the first letter of a string.
    /// </n>
    /// <param name="input">The string to capitalize.</param>
    /// <returns>The string with the first character capitalized.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="input"/> is empty.</exception>
    public static string CapitalizeFirstLetter(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrEmpty(input);

        return char.ToUpperInvariant(input[0]) + input.Substring(1);
    }

    /// <summary>
    /// Checks if a string contains any digit characters.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>True if the string contains any digits; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static bool ContainsDigit(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return input.Any(char.IsDigit);
    }

    /// <summary>
    /// Repeats a string a specified number of times.
    /// </summary>
    /// <param name="input">The string to repeat.</param>
    /// <param name="count">The number of times to repeat the string.</param>
    /// <returns>The repeated string, or empty string if count is 0 or less.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string Repeat(this string input, int count)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        return count == 0
            ? string.Empty
            : string.Concat(Enumerable.Repeat(input, count));
    }

    /// <summary>
    /// Reverses a string.
    /// </summary>
    /// <param name="input">The string to reverse.</param>
    /// <returns>The reversed string, or empty string if input is null or empty.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string Reverse(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new string(input.Reverse().ToArray());
    }

    /// <summary>
    /// Checks if a string is a palindrome (case-insensitive, ignoring non-alphanumeric characters).
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>True if the string is a palindrome; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static bool IsPalindrome(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var clean = Regex.Replace(input.ToLowerInvariant(), @"[^a-z0-9]", "");
        return clean == clean.Reverse();
    }
}
