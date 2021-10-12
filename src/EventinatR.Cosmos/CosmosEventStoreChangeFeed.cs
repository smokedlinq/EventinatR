using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventinatR.Cosmos.Documents;
using Newtonsoft.Json;

namespace EventinatR.Cosmos
{
    public static class CosmosEventStoreChangeFeed
    {
        public static IEnumerable<Event> ParseEvents(string json)
            => JsonConvert.DeserializeObject<ChangeFeedDocument[]>(json).Where(x => x.IsEvent).Select(x => x.ToEventDocument()!.AsEvent());
    }
}
