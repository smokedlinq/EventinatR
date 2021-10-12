using System;

namespace EventinatR.Cosmos
{
    public class CosmosEventStoreContainerOptions
    {
        public CosmosEventStoreContainerOptions(string id)
            => Id = id ?? throw new ArgumentNullException(nameof(id));

        public string Id { get; set; }
        public int? Throughput { get; set; }
    }
}
