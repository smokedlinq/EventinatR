using Azure.Messaging.ServiceBus;
using EventinatR.Publishers.ServiceBus;

namespace EventinatR.Tests.Publishers;

public class ServiceBusEventPublisherTests
{
    private readonly ServiceBusEventPublisher _sut;
    private readonly ServiceBusSender _sender = Substitute.For<ServiceBusSender>();
    private readonly Fixture _fixture = new();

    public ServiceBusEventPublisherTests()
    {
        _sut = new ServiceBusEventPublisher(_sender);
    }

    [Fact]
    public async Task PublishAsync_ShouldCallClientSendEventsAsync_WhenSingleEventIsProvided()
    {
        var events = _fixture.Build<Event>()
            .With(x => x.Data, JsonData.From(new { }))
            .CreateMany(1);

        var batchMessageStore = new List<ServiceBusMessage>();
        var batch = ServiceBusModelFactory.ServiceBusMessageBatch(long.MaxValue, batchMessageStore);

        _sender.CreateMessageBatchAsync(Arg.Any<CancellationToken>()).Returns(batch);

        await _sut.PublishAsync(events);

        await _sender.Received(1).SendMessagesAsync(Arg.Any<ServiceBusMessageBatch>(), Arg.Any<CancellationToken>());

        batchMessageStore.Count.Should().Be(1);
    }
}
