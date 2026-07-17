// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace SkiaSharpChartEngine.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="StringFormatHelper"/>.
/// <para>
/// This class serializes <see cref="StringFormatHelper"/> instances to JSON and deserializes them back.
/// Since <see cref="StringFormatHelper"/> is a stateless utility class with only static methods,
/// serialization produces a minimal JSON representation indicating the type,
/// and deserialization returns a new instance.
/// </para>
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
    /// <param name="value">The instance to serialize. Must not be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the instance containing type information.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this StringFormatHelper value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(new { Type = nameof(StringFormatHelper) }, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="StringFormatHelper"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Can be null or empty.</param>
    /// <returns>
    /// A new <see cref="StringFormatHelper"/> instance if deserialization succeeds,
    /// or null if <paramref name="json"/> is null or empty.
    /// </returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be parsed.</exception>
    public static StringFormatHelper? FromJson(string? json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        // StringFormatHelper has no instance state to deserialize.
        // Validate JSON format but return a new instance.
        JsonSerializer.Deserialize<object>(json, _jsonOptions);
        return new StringFormatHelper();
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="StringFormatHelper"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Cannot be null or empty.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is null or empty.</exception>
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