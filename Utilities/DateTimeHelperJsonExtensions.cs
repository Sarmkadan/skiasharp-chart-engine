// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// System.Text.Json serialization extensions for DateTimeHelper
/// Provides JSON serialization/deserialization methods for DateTimeHelper enum values
/// </summary>
public static class DateTimeHelperJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a DateTimePeriod enum value to JSON string
    /// </summary>
    /// <param name="value">The DateTimePeriod enum value to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation of the DateTimePeriod enum value</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this DateTimePeriod value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a DateTimePeriod enum value
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>Deserialized DateTimePeriod enum value, or null if JSON is null or empty</returns>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized</exception>
    public static DateTimePeriod? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<DateTimePeriod>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a DateTimePeriod enum value
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter receiving the deserialized value</param>
    /// <returns>True if deserialization succeeded; false otherwise</returns>
    public static bool TryFromJson(string json, out DateTimePeriod? value)
    {
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