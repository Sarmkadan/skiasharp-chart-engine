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
    // Convert string to kebab-case (dash-separated lowercase)
    public static string ToKebabCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var pattern = new Regex(@"[A-Z]");
        var kebab = pattern.Replace(input, m => "-" + m.Value.ToLower());
        return kebab.TrimStart('-');
    }

    // Convert string to snake_case (underscore-separated lowercase)
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var pattern = new Regex(@"[A-Z]");
        var snake = pattern.Replace(input, m => "_" + m.Value.ToLower());
        return snake.TrimStart('_');
    }

    // Convert string to PascalCase (FirstLetterCapitalized)
    public static string ToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var cultureInfo = CultureInfo.CurrentCulture;
        var textInfo = cultureInfo.TextInfo;

        // Split on spaces, hyphens, and underscores
        var words = Regex.Split(input, @"[\s\-_]+")
            .Where(w => !string.IsNullOrEmpty(w))
            .Select(w => textInfo.ToTitleCase(w.ToLower()));

        return string.Concat(words);
    }

    // Safely parse double with fallback
    public static double ToDoubleOrDefault(this string input, double defaultValue = 0)
    {
        if (double.TryParse(input, CultureInfo.InvariantCulture, out var result))
            return result;

        return defaultValue;
    }

    // Safely parse integer with fallback
    public static int ToIntOrDefault(this string input, int defaultValue = 0)
    {
        if (int.TryParse(input, CultureInfo.InvariantCulture, out var result))
            return result;

        return defaultValue;
    }

    // Truncate string to maximum length
    public static string Truncate(this string input, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        return input.Substring(0, maxLength - suffix.Length) + suffix;
    }

    // Check if string is numeric
    public static bool IsNumeric(this string input)
    {
        return !string.IsNullOrEmpty(input) && input.All(char.IsDigit);
    }

    // Check if string is valid hex color
    public static bool IsValidHexColor(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        var hexPattern = new Regex(@"^#?([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$");
        return hexPattern.IsMatch(input);
    }

    // Extract numbers from string
    public static string ExtractNumbers(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return new Regex(@"[0-9]+").Match(input).Value;
    }

    // Remove whitespace
    public static string RemoveWhitespace(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return Regex.Replace(input, @"\s+", "");
    }

    // Capitalize first letter
    public static string CapitalizeFirstLetter(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1);
    }

    // Check if string contains any digit
    public static bool ContainsDigit(this string input)
    {
        return !string.IsNullOrEmpty(input) && input.Any(char.IsDigit);
    }

    // Repeat string n times
    public static string Repeat(this string input, int count)
    {
        if (string.IsNullOrEmpty(input) || count <= 0)
            return string.Empty;

        return string.Concat(Enumerable.Repeat(input, count));
    }

    // Reverse string
    public static string Reverse(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return new string(input.Reverse().ToArray());
    }

    // Check if string is palindrome
    public static bool IsPalindrome(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        var clean = Regex.Replace(input.ToLower(), @"[^a-z0-9]", "");
        return clean == clean.Reverse();
    }
}
