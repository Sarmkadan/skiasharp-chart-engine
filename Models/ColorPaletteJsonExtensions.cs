// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharpChartEngine.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ColorPalette"/>
/// </summary>
public static class ColorPaletteJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes the <see cref="ColorPalette"/> to a JSON string
    /// </summary>
    /// <param name="value">The color palette to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>A JSON string representation of the color palette</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null</exception>
    public static string ToJson(this ColorPalette value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ColorPalette"/> instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>A deserialized color palette, or null if JSON is null or empty</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized</exception>
    public static ColorPalette? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ColorPalette>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ColorPalette"/> instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized color palette, or null on failure</param>
    /// <returns>True if deserialization succeeded; otherwise, false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null</exception>
    public static bool TryFromJson(string json, out ColorPalette? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ColorPalette>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
