using System.Text.Json;

namespace Skiasharp.ChartEngine.Serializers
{
    public static class XmlChartSerializerJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamePolicy = JsonNamingPolicy.CamelCase,
        };

        public static string ToJson(this XmlChartSerializer value, bool indented = false)
        {
            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static XmlChartSerializer? FromJson(string json)
        {
            return JsonSerializer.Deserialize<XmlChartSerializer>(json, _jsonSerializerOptions);
        }

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