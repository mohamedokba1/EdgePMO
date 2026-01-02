using System.Text.Json;
using System.Text.Json.Serialization;

namespace EdgePMO.API.Settings
{
    public class ContentConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString() ?? string.Empty;

                case JsonTokenType.StartArray:
                    List<string>? list = new List<string>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            list.Add(reader.GetString() ?? string.Empty);
                        }
                    }
                    return list;

                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value is string stringValue)
            {
                writer.WriteStringValue(stringValue);
            }
            else if (value is List<string> listValue)
            {
                writer.WriteStartArray();
                foreach (var item in listValue)
                {
                    writer.WriteStringValue(item);
                }
                writer.WriteEndArray();
            }
            else
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }
    }
}
