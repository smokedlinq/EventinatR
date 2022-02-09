using System.Text.Json.Serialization;

namespace EventinatR.Serialization;

internal class EventStreamVersionJsonConverter : JsonConverter<EventStreamVersion>
{
    public override EventStreamVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetInt64();

    public override void Write(Utf8JsonWriter writer, EventStreamVersion value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value.Value);
}
