using System;
using System.Runtime.Serialization;
using Microsoft.Azure.Cosmos;

namespace EventinatR.CosmosDB
{
    public class CosmosEventStreamAppendException : CosmosEventStoreException
    {
        public TransactionalBatchResponse? Response
            => Data[nameof(TransactionalBatchResponse)] as TransactionalBatchResponse;

        public CosmosEventStreamAppendException(TransactionalBatchResponse response)
            : base($"Append transaction failed with {response?.StatusCode}: {response?.ErrorMessage}")
            => Data[nameof(TransactionalBatchResponse)] = response ?? throw new ArgumentNullException(nameof(response));

        protected CosmosEventStreamAppendException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
