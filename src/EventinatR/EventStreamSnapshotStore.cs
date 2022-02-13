namespace EventinatR;

public abstract class EventStreamSnapshotStore
{
    public abstract Task<EventStreamSnapshot<T>> GetAsync<T>(CancellationToken cancellationToken = default);

    public abstract Task<EventStreamSnapshot<T>> SaveAsync<T>(T state, EventStreamVersion version, CancellationToken cancellationToken = default);
}
