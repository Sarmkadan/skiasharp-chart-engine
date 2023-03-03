// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkiaSharpChartEngine.Integration;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="HttpChartClient"/>
/// </summary>
public static class HttpChartClientJsonExtensions
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
    /// Serializes the <see cref="HttpChartClient"/> instance to a JSON string.
    /// Serializes only the base URL configuration.
    /// </summary>
    /// <param name="value">The HTTP chart client instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the HTTP chart client.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this HttpChartClient value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var config = new HttpChartClientConfiguration
        {
            BaseUrl = GetBaseUrl(value)
        };

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(config, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="HttpChartClient"/> instance.
    /// Note: The logger and serializer dependencies must be provided separately.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized HTTP chart client instance, or null if deserialization fails.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static HttpChartClient? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var config = JsonSerializer.Deserialize<HttpChartClientConfiguration>(json, _jsonSerializerOptions);
            return config?.ToHttpChartClient();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="HttpChartClient"/> instance.
    /// Note: The logger and serializer dependencies must be provided separately.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized HTTP chart client instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out HttpChartClient? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var config = JsonSerializer.Deserialize<HttpChartClientConfiguration>(json, _jsonSerializerOptions);
            value = config?.ToHttpChartClient();
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Represents the serializable configuration of an HttpChartClient
    /// </summary>
    private sealed class HttpChartClientConfiguration
    {
        public string? BaseUrl { get; set; }

        public HttpChartClient ToHttpChartClient()
        {
            if (string.IsNullOrEmpty(BaseUrl))
            {
                throw new InvalidOperationException(
                    "BaseUrl cannot be null or empty when deserializing HttpChartClient.");
            }

            return new HttpChartClient(BaseUrl, null!, null!);
        }
    }

    /// <summary>
    /// Gets the base URL from an HttpChartClient instance using reflection.
    /// </summary>
    private static string GetBaseUrl(HttpChartClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        var field = typeof(HttpChartClient).GetField(
            "_baseUrl",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return field?.GetValue(client) as string ?? string.Empty;
    }
}