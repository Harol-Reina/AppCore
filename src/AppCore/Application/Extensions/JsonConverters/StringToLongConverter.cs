using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppCore.Application.Extensions.JsonConverters;

public sealed class StringToLongConverter : JsonConverter<long> {
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType == JsonTokenType.String && long.TryParse(reader.GetString(), out var value))
            return value;
        if (reader.TokenType == JsonTokenType.Number)
            return reader.GetInt64();
        throw new JsonException($"Cannot convert {reader.GetString()} to long.");
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options) {
        writer.WriteStringValue(value.ToString());
    }
}
