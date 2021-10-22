using System.Runtime.Serialization;

namespace EventinatR.Cosmos;

public class CosmosEventStoreException : Exception
{
    public CosmosEventStoreException()
    {
    }

    public CosmosEventStoreException(string? message)
        : base(message)
    {
    }

    public CosmosEventStoreException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected CosmosEventStoreException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
