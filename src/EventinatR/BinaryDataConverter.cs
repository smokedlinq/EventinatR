using System.Text.Json.Serialization;

namespace EventinatR;

internal class BinaryDataConverter : JsonConverter<BinaryData>
{
    public override BinaryData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(JsonDocument.ParseValue(ref reader).RootElement.GetRawText());

    public override void Write(Utf8JsonWriter writer, BinaryData value, JsonSerializerOptions options)
        => JsonDocument.Parse(value.ToString()).WriteTo(writer);
}
