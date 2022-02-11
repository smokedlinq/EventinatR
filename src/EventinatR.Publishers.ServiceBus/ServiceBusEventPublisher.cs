using Azure.Messaging.ServiceBus;

namespace EventinatR.Publishers.ServiceBus;

public class ServiceBusEventPublisher
{
    private readonly ServiceBusSender _sender;

    public ServiceBusEventPublisher(ServiceBusSender sender)
        => _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    public virtual async Task PublishAsync(IEnumerable<Event> events, CancellationToken cancellationToken = default)
    {
        using var batch = await _sender.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false);

        foreach (var e in events)
        {
            if (!ShouldPublish(e))
            {
                continue;
            }

            var body = Convert(e);
            var message = new ServiceBusMessage(body)
            {
                SessionId = e.StreamId.Value
            };

            if (!batch.TryAddMessage(message))
            {
                throw new InvalidOperationException($"The number of events is too large to send in a single batch; maximum size is {batch.MaxSizeInBytes} bytes; try reducing the number of events in the batch.");
            }
        }

        await _sender.SendMessagesAsync(batch, cancellationToken).ConfigureAwait(false);
    }

    protected virtual bool ShouldPublish(Event e)
        => true;

    protected virtual BinaryData Convert(Event e)
        => BinaryData.FromObjectAsJson(e);
}
