using System.Threading;
using System.Threading.Tasks;

namespace EventinatR
{
    public abstract class EventStore
    {
        public abstract Task<EventStream> GetStreamAsync(string id, CancellationToken cancellationToken = default);
    }
}
