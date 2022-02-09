using System.Text.Json.Serialization;

namespace EventinatR.Serialization;

internal class EventStreamIdJsonConverter : JsonConverter<EventStreamId>
{
    public override EventStreamId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetString() ?? EventStreamId.None;

    public override void Write(Utf8JsonWriter writer, EventStreamId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Value);
}
