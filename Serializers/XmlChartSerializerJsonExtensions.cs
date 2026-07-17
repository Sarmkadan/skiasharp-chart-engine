using System.Text.Json;

namespace Skiasharp.ChartEngine.Serializers
{
    /// <summary>
    /// Provides extension methods for converting between XmlChartSerializer and JSON.
    /// </summary>
    public static class XmlChartSerializerJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamePolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Serializes the XmlChartSerializer instance to a JSON string.
        /// </summary>
        /// <param name="value">The XmlChartSerializer instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>The JSON string representation of the XmlChartSerializer instance.</returns>
        public static string ToJson(this XmlChartSerializer value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        /// <summary>
        /// Deserializes a JSON string to an XmlChartSerializer instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized XmlChartSerializer instance.</returns>
        public static XmlChartSerializer? FromJson(string json)
        {
            return JsonSerializer.Deserialize<XmlChartSerializer>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to an XmlChartSerializer instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized XmlChartSerializer instance.</param>
        /// <returns>True if the deserialization was successful, false otherwise.</returns>
        public static bool TryFromJson(string json, out XmlChartSerializer? value)
        {
            try
            {
                value = FromJson(json);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
