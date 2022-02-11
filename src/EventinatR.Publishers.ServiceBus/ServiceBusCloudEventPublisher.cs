using System.Globalization;
using System.Net.Mime;
using Azure.Messaging;
using Azure.Messaging.ServiceBus;

namespace EventinatR.Publishers.ServiceBus;

public class ServiceBusCloudEventPublisher
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ServiceBusSender _sender;
    private readonly string _source;

    public ServiceBusCloudEventPublisher(ServiceBusSender sender, string source)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

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

    protected virtual bool ShouldPublish(Event @event)
        => true;

    protected virtual BinaryData Convert(Event @event)
    {
        var cloudEvent = new CloudEvent(_source, @event.Data.Type.Name, @event.Data.Value, MediaTypeNames.Application.Json, CloudEventDataFormat.Json)
        {
            Subject = @event.StreamId.Value,
            Time = @event.Timestamp
        };

        cloudEvent.ExtensionAttributes.Add("partitionkey", @event.StreamId.Value);
        cloudEvent.ExtensionAttributes.Add("sequence", @event.Version.Value.ToString(CultureInfo.InvariantCulture));
        cloudEvent.ExtensionAttributes.Add("sequencetype", "Integer");
        cloudEvent.ExtensionAttributes.Add("transaction", @event.Transaction.Version.Value.ToString(CultureInfo.InvariantCulture));

        return BinaryData.FromObjectAsJson(new[] { cloudEvent }, DefaultSerializerOptions);
    }
}
