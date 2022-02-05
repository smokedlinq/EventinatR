using Azure.Messaging;
using Azure.Messaging.EventGrid;
using EventinatR.Publishers.EventGrid;

namespace EventinatR.Tests.Publishers;

public class CloudEventPublisherTests
{
    [Theory]
    [Fixtures]
    public async Task PublishAsync(Event[] events)
    {
        var client = Substitute.For<EventGridPublisherClient>();
        var response = Substitute.For<Azure.Response>();

        client.SendEventsAsync(Arg.Any<IEnumerable<CloudEvent>>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var sut = new CloudEventPublisher(client, "http://localhost");

        await sut.PublishAsync(events);

        await client.Received().SendEventsAsync(Arg.Any<IEnumerable<CloudEvent>>(), Arg.Any<CancellationToken>());
    }
}
