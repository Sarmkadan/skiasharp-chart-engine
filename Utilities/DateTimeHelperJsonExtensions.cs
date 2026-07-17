// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="DateTimePeriod"/> enum values.
/// </summary>
public static class DateTimeHelperJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a <see cref="DateTimePeriod"/> enum value to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="DateTimePeriod"/> enum value to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>JSON string representation of the <see cref="DateTimePeriod"/> enum value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this DateTimePeriod value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="DateTimePeriod"/> enum value.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns>Deserialized <see cref="DateTimePeriod"/> enum value, or <see langword="null"/> if <paramref name="json"/> is <see langword="null"/> or empty.</returns>
    /// <exception cref="JsonException">Thrown when <paramref name="json"/> is invalid or cannot be deserialized to a valid <see cref="DateTimePeriod"/> value.</exception>
    public static DateTimePeriod? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<DateTimePeriod>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="DateTimePeriod"/> enum value.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output parameter receiving the deserialized value.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out DateTimePeriod? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<DateTimePeriod>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}