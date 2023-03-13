// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharpChartEngine.Caching;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for CachePolicy
/// </summary>
public static class CachePolicyJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a CachePolicy instance to a JSON string
    /// </summary>
    /// <param name="value">The cache policy to serialize</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability</param>
    /// <returns>A JSON string representation of the cache policy</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
    public static string ToJson(this CachePolicy value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a CachePolicy instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <returns>The deserialized CachePolicy instance, or null if JSON is null or empty</returns>
    /// <exception cref="JsonException">Thrown when JSON is malformed or cannot be deserialized</exception>
    public static CachePolicy? FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<CachePolicy>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a CachePolicy instance
    /// </summary>
    /// <param name="json">The JSON string to deserialize</param>
    /// <param name="value">Receives the deserialized CachePolicy if successful</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJson(string json, out CachePolicy? value)
    {
        value = null;

        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<CachePolicy>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Custom JSON converter for CachePolicy to handle TimeSpan? serialization
    /// </summary>
    private sealed class CachePolicyConverter : JsonConverter<CachePolicy>
    {
        public override CachePolicy? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using JsonDocument doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            var policy = new CachePolicy();

            if (root.TryGetProperty("absoluteExpirationRelativeToNow", out var absoluteProp))
            {
                if (absoluteProp.ValueKind != JsonValueKind.Null)
                {
                    policy.AbsoluteExpirationRelativeToNow = TimeSpan.FromTicks(absoluteProp.GetInt64());
                }
            }

            if (root.TryGetProperty("slidingExpiration", out var slidingProp))
            {
                if (slidingProp.ValueKind != JsonValueKind.Null)
                {
                    policy.SlidingExpiration = TimeSpan.FromTicks(slidingProp.GetInt64());
                }
            }

            if (root.TryGetProperty("priority", out var priorityProp))
            {
                policy.Priority = (CachePriority)priorityProp.GetInt32();
            }

            return policy;
        }

        public override void Write(
            Utf8JsonWriter writer,
            CachePolicy value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value.AbsoluteExpirationRelativeToNow.HasValue)
            {
                writer.WriteNumber(
                    "absoluteExpirationRelativeToNow",
                    value.AbsoluteExpirationRelativeToNow.Value.Ticks);
            }
            else
            {
                writer.WriteNull("absoluteExpirationRelativeToNow");
            }

            if (value.SlidingExpiration.HasValue)
            {
                writer.WriteNumber("slidingExpiration", value.SlidingExpiration.Value.Ticks);
            }
            else
            {
                writer.WriteNull("slidingExpiration");
            }

            writer.WriteNumber("priority", (int)value.Priority);

            writer.WriteEndObject();
        }
    }
}
