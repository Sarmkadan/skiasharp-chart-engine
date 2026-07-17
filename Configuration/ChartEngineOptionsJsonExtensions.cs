// ChartEngineOptionsJsonExtensions.cs
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SkiaSharpChartEngine.Configuration;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ChartEngineOptions"/>.
/// </summary>
public static class ChartEngineOptionsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver()
	};

	/// <summary>
	/// Serializes the <see cref="ChartEngineOptions"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The options instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the options.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this ChartEngineOptions value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="ChartEngineOptions"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized options instance, or null if the JSON is null or empty.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	public static ChartEngineOptions? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}

		return JsonSerializer.Deserialize<ChartEngineOptions>(json, _jsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="ChartEngineOptions"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized options instance if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	public static bool TryFromJson(string json, out ChartEngineOptions? value)
	{
		value = null;

		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<ChartEngineOptions>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}