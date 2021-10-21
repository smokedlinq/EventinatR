using System;
using System.Text.Json.Serialization;
using EventinatR.Serialization;

namespace EventinatR
{
    public record Event(EventStreamId StreamId, EventStreamVersion Version, DateTimeOffset Timestamp, string Type, [property: JsonConverter(typeof(BinaryDataConverter))] BinaryData Data);
}
