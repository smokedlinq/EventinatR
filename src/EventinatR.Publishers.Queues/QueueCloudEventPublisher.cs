using System.Globalization;
using System.Net.Mime;
using Azure.Messaging;
using Azure.Storage.Queues;

namespace EventinatR.Publishers.Queues;

public class QueueCloudEventPublisher : QueueEventPublisher
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _source;

    public QueueCloudEventPublisher(QueueClient queue, string source)
        : base(queue)
        => _source = source ?? throw new ArgumentNullException(nameof(source));

    protected override BinaryData Convert(Event e)
    {
        var cloudEvent = ConvertToCloudEvent(e);
        return BinaryData.FromObjectAsJson(new[] { cloudEvent }, DefaultSerializerOptions);
    }

    protected virtual CloudEvent ConvertToCloudEvent(Event e)
    {
        var cloudEvent = new CloudEvent(_source, e.Data.Type.Name, e.Data.Value, MediaTypeNames.Application.Json, CloudEventDataFormat.Json)
        {
            Subject = e.StreamId.Value,
            Time = e.Timestamp
        };

        cloudEvent.ExtensionAttributes.Add("partitionkey", e.StreamId.Value);
        cloudEvent.ExtensionAttributes.Add("sequence", e.Version.Value.ToString(CultureInfo.InvariantCulture));
        cloudEvent.ExtensionAttributes.Add("sequencetype", "Integer");
        cloudEvent.ExtensionAttributes.Add("transaction", e.Transaction.Version.Value.ToString(CultureInfo.InvariantCulture));

        return cloudEvent;
    }
}
