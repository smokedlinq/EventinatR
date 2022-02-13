using Azure.Storage.Queues;

namespace EventinatR.Publishers.Queues;

public class QueueEventPublisher
{
    private readonly QueueClient _queue;

    public QueueEventPublisher(QueueClient queue)
        => _queue = queue ?? throw new ArgumentNullException(nameof(queue));

    public virtual async Task PublishAsync(IEnumerable<Event> events, CancellationToken cancellationToken = default)
    {
        foreach (var e in events)
        {
            if (ShouldPublish(e))
            {
                var message = Convert(e);
                await SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    protected virtual Task SendMessageAsync(BinaryData message, CancellationToken cancellationToken)
        => _queue.SendMessageAsync(message, cancellationToken: cancellationToken);

    protected virtual bool ShouldPublish(Event e)
        => true;

    protected virtual BinaryData Convert(Event e)
        => BinaryData.FromObjectAsJson(e);
}
