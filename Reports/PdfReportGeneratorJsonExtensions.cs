// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SkiaSharpChartEngine.Reports;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for
/// <see cref="PdfReportGenerator"/> instances.
/// </summary>
public static class PdfReportGeneratorJsonExtensions
{
	/// <summary>
	/// Default JSON serialization options used by all extension methods.
	/// Uses camelCase property naming and invariant culture for consistent serialization.
	/// </summary>
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
	};

	/// <summary>
	/// Serializes the <see cref="PdfReportGenerator"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.
	/// Uses invariant culture for consistent serialization across platforms.</param>
	/// <returns>A JSON string representation of the instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this PdfReportGenerator value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="PdfReportGenerator"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.
	/// Uses invariant culture for consistent parsing across platforms.</param>
	/// <returns>A deserialized <see cref="PdfReportGenerator"/> instance, or null if deserialization fails.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static PdfReportGenerator? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		try
		{
			return JsonSerializer.Deserialize<PdfReportGenerator>(json, _jsonOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="PdfReportGenerator"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.
	/// Uses invariant culture for consistent parsing across platforms.</param>
	/// <param name="value">Receives the deserialized instance if successful, otherwise null.</param>
	/// <returns>True if deserialization succeeded; otherwise false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string json, out PdfReportGenerator? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = null;

		try
		{
			value = JsonSerializer.Deserialize<PdfReportGenerator>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}
