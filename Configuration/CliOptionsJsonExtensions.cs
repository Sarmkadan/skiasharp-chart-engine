// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharpChartEngine.Configuration;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="CliOptions"/>.
/// Includes methods for converting CliOptions to and from JSON strings.
/// </summary>
public static class CliOptionsJsonExtensions
{
    /// <summary>
    /// JSON serializer options with camelCase naming policy and optimized settings for CLI configuration.
    /// </summary>
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Converts CliOptions to JSON string representation.
    /// </summary>
    /// <param name="value">The CliOptions instance to serialize. Must not be null.</param>
    /// <param name="indented">Whether to format the JSON with indentation for better readability.</param>
    /// <returns>JSON string representation of the CliOptions.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this CliOptions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.ToJsonUnchecked(indented);
    }

    /// <summary>
    /// Deserializes CliOptions from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Must not be null or empty.</param>
    /// <returns>Deserialized CliOptions instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    /// <exception cref="JsonException">Thrown when JSON is invalid or cannot be deserialized to CliOptions.</exception>
    public static CliOptions FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return JsonSerializer.Deserialize<CliOptions>(json, _jsonSerializerOptions) ??
               throw new JsonException("Deserialization returned null for non-nullable type CliOptions");
    }

    /// <summary>
    /// Attempts to deserialize CliOptions from JSON string.
    /// </summary>
    /// <param name="json">JSON string to deserialize. Must not be null or empty.</param>
    /// <param name="value">Output parameter containing the deserialized CliOptions if successful, otherwise null.</param>
    /// <returns>True if deserialization succeeded; false otherwise.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out CliOptions value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<CliOptions>(json, _jsonSerializerOptions) ??
                    throw new JsonException("Deserialization returned null for non-nullable type CliOptions");
            return true;
        }
        catch
        {
            value = null!;
            return false;
        }
    }

    /// <summary>
    /// Internal implementation of ToJson without null check for performance when caller guarantees non-null.
    /// </summary>
    private static string ToJsonUnchecked(this CliOptions value, bool indented)
    {
        var options = new JsonSerializerOptions(_jsonSerializerOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(value, options);
    }
}