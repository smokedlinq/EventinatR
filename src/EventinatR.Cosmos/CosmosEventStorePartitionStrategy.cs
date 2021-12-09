using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventinatR.Cosmos
{
    public class CosmosEventStorePartitionStrategy
    {
        public virtual PartitionKey GetPartitionKey(EventStreamId id)
        {
            ArgumentNullException.ThrowIfNull(id);
            return new PartitionKey(id.Value);
        }
    }
}
