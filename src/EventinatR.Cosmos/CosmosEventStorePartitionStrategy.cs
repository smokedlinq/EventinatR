namespace EventinatR.Cosmos;

public class CosmosEventStorePartitionStrategy
{
    public virtual PartitionKey GetPartitionKey(EventStreamId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        return new PartitionKey(id.Value);
    }
}
