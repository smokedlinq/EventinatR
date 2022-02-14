
using System.Globalization;
using System.Text.Json.Serialization;
using EventinatR.Serialization;

namespace EventinatR.Cosmos;

[JsonConverter(typeof(EventStreamVersionJsonConverter))]
internal record CosmosEventStreamVersion(long Value, string ETag) : EventStreamVersion(Value)
{
    public override string ToString()
        => Value.ToString("D0", CultureInfo.CurrentCulture);
}
