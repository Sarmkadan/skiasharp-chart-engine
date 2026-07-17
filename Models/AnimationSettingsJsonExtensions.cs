// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for AnimationSettings
/// </summary>
public static class AnimationSettingsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the AnimationSettings instance to a JSON string
    /// </summary>
    /// <param name="value">The AnimationSettings instance to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the AnimationSettings</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static string ToJson(this AnimationSettings value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, new JsonSerializerOptions(_jsonOptions) { WriteIndented = indented });
    }

    /// <summary>
    /// Deserializes an AnimationSettings instance from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized AnimationSettings instance</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or whitespace</exception>
    /// <exception cref="JsonException"><paramref name="json"/> contains invalid JSON data</exception>
    public static AnimationSettings FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<AnimationSettings>(json, _jsonOptions)
            ?? throw new JsonException("Deserialized AnimationSettings is null");
    }

    /// <summary>
    /// Attempts to deserialize an AnimationSettings instance from a JSON string
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized AnimationSettings instance if successful</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/>, empty, or whitespace</exception>
    public static bool TryFromJson(string json, out AnimationSettings? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<AnimationSettings>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}