// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharpChartEngine.Integration;

/// <summary>
/// Provides extension methods for serializing and deserializing <see cref="WebhookHandler"/> instances to and from JSON.
/// </summary>
/// <remarks>
/// This static class cannot be inherited.
/// </remarks>
public static sealed class WebhookHandlerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes the <see cref="WebhookHandler"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The webhook handler instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the webhook handler.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this WebhookHandler value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="WebhookHandler"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized webhook handler instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized to <see cref="WebhookHandler"/>.</exception>
    public static WebhookHandler FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<WebhookHandler>(json, _jsonSerializerOptions) ?? throw new JsonException("Deserialization returned null");
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="WebhookHandler"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">When this method returns, contains the deserialized webhook handler instance if deserialization succeeds; otherwise, null.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized to <see cref="WebhookHandler"/>.</exception>
    public static bool TryFromJson(string json, out WebhookHandler value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<WebhookHandler>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = default;
            return false;
        }
    }
}