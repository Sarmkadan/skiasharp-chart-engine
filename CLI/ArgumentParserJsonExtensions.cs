// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharpChartEngine.CLI;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for ArgumentParser.
/// </summary>
public static sealed class ArgumentParserJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the ArgumentParser instance to a JSON string.
    /// </summary>
    /// <param name="value">The ArgumentParser instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the ArgumentParser.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToJson(this ArgumentParser value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an ArgumentParser instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An ArgumentParser instance if successful; otherwise null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or consists only of white-space characters.</exception>
    public static ArgumentParser? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<ArgumentParser>(json, _jsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an ArgumentParser instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized ArgumentParser instance if successful.</param>
    /// <returns>True if deserialization succeeded; false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or consists only of white-space characters.</exception>
    public static bool TryFromJson(string json, out ArgumentParser? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<ArgumentParser>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}