namespace EventinatR;

public interface IEventStreamReader
{
    IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default);
}
