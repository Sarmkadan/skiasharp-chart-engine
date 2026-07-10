// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="StringFormatHelper"/>.
/// </summary>
public static class StringFormatHelperJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Serializes the <see cref="StringFormatHelper"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this StringFormatHelper value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        // StringFormatHelper has no instance state to serialize.
        // Return a JSON representation indicating the type.
        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(new { Type = nameof(StringFormatHelper) }, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="StringFormatHelper"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
    public static StringFormatHelper? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        // StringFormatHelper has no instance state, so we return a new instance.
        // The JSON is validated but not used for deserialization.
        try
        {
            JsonSerializer.Deserialize<object>(json, _jsonOptions);
            return new StringFormatHelper();
        }
        catch (JsonException)
        {
            throw;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="StringFormatHelper"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out StringFormatHelper? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            JsonSerializer.Deserialize<object>(json, _jsonOptions);
            value = new StringFormatHelper();
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}