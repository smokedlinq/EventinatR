using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using EventinatR.Publishers.EventGrid;

namespace EventinatR.Tests.Publishers;

public class CloudEventPublisherTests
{
    private readonly CloudEventPublisher _sut;
    private readonly EventGridPublisherClient _client = Substitute.For<EventGridPublisherClient>();
    private readonly Response _response = Substitute.For<Azure.Response>();
    private readonly Fixture _fixture = new();

    public CloudEventPublisherTests()
    {
        _sut = new CloudEventPublisher(_client, "/");
    }

    [Fact]
    public async Task PublishAsync_ShouldCallClientSendEventsAsync_WhenSingleEventIsProvided()
    {
        var events = _fixture.CreateMany<Event>(1);

        _client.SendEventsAsync(Arg.Any<IEnumerable<CloudEvent>>(), Arg.Any<CancellationToken>())
            .Returns(_response);

        await _sut.PublishAsync(events);

        await _client.Received(1).SendEventsAsync(Arg.Any<IEnumerable<CloudEvent>>(), Arg.Any<CancellationToken>());
    }
}
