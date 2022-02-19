using System.Globalization;
using System.Net.Mime;
using Azure.Messaging;
using Azure.Messaging.ServiceBus;

namespace EventinatR.Publishers.ServiceBus;

public class ServiceBusCloudEventPublisher : ServiceBusEventPublisher
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _source;

    public ServiceBusCloudEventPublisher(ServiceBusSender sender, string source)
        : base(sender)
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
