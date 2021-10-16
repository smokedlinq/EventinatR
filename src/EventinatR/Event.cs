using System;
using System.Text.Json.Serialization;
using EventinatR.Serialization;

namespace EventinatR
{
    public record Event(EventStreamId StreamId, long Version, DateTimeOffset Timestamp, string Type, [property: JsonConverter(typeof(BinaryDataConverter))] BinaryData Data);
}
