using Azure;
using Azure.Messaging;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using EventinatR.Publishers.Queues;

namespace EventinatR.Tests.Publishers;

public class QueueCloudEventPublisherTests
{
    private readonly QueueCloudEventPublisher _sut;
    private readonly QueueClient _queue = Substitute.For<QueueClient>();
    private readonly Response<SendReceipt> _response = Substitute.For<Azure.Response<SendReceipt>>();
    private readonly Fixture _fixture = new();

    public QueueCloudEventPublisherTests()
    {
        _sut = new QueueCloudEventPublisher(_queue, "/");
    }

    [Fact]
    public async Task PublishAsync_ShouldProduceCloudEventJson_WhenMessageIsSerialized()
    {
        var events = _fixture.Build<Event>()
            .With(x => x.Data, JsonData.From(new { }))
            .CreateMany(1);
        var messages = new List<BinaryData>();

        _queue.SendMessageAsync(Arg.Any<BinaryData>()).Returns(x =>
        {
            var message = x.Arg<BinaryData>();
            messages.Add(message);
            return _response;
        });

        await _sut.PublishAsync(events);

        var message = messages.First();
        CloudEvent.ParseMany(message).Length.Should().Be(1);
    }
}
