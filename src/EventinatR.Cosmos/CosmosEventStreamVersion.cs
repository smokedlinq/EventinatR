
namespace EventinatR.Cosmos;

internal record CosmosEventStreamVersion(long Value, string ETag) : EventStreamVersion(Value)
{
}
