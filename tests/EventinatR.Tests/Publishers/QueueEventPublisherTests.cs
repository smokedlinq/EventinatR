using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using EventinatR.Publishers.Queues;

namespace EventinatR.Tests.Publishers;

public class QueueEventPublisherTests
{
    private readonly QueueEventPublisher _sut;
    private readonly QueueClient _queue = Substitute.For<QueueClient>();
    private readonly Response<SendReceipt> _response = Substitute.For<Azure.Response<SendReceipt>>();
    private readonly Fixture _fixture = new();

    public QueueEventPublisherTests()
    {
        _sut = new QueueEventPublisher(_queue);
    }

    [Fact]
    public async Task PublishAsync_ShouldCallClientSendEventsAsync_WhenSingleEventIsProvided()
    {
        var events = _fixture.Build<Event>()
            .With(x => x.Data, JsonData.From(new { }))
            .CreateMany(1);

        _queue.SendMessageAsync(Arg.Any<BinaryData>())
            .Returns(_response);

        await _sut.PublishAsync(events);

        await _queue.Received(1).SendMessageAsync(Arg.Any<BinaryData>());
    }
}
