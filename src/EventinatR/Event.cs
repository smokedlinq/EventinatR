using System;

namespace EventinatR
{
    public record Event(long Version, DateTimeOffset Timestamp, string Type, BinaryData Data);
}