using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using EventinatR.Publishers.ServiceBus;

namespace EventinatR.Tests.Publishers;

public class ServiceBusCloudEventPublisherTests
{
    private readonly ServiceBusCloudEventPublisher _sut;
    private readonly ServiceBusSender _sender = Substitute.For<ServiceBusSender>();
    private readonly Fixture _fixture = new();

    public ServiceBusCloudEventPublisherTests()
    {
        _sut = new ServiceBusCloudEventPublisher(_sender, "/");
    }

    [Fact]
    public async Task PublishAsync_ShouldProduceCloudEventJson_WhenMessageIsSerialized()
    {
        var events = _fixture.Build<Event>()
            .With(x => x.Data, JsonData.From(new { }))
            .CreateMany(1);

        var batchMessageStore = new List<ServiceBusMessage>();
        var batch = ServiceBusModelFactory.ServiceBusMessageBatch(long.MaxValue, batchMessageStore);

        _sender.CreateMessageBatchAsync(Arg.Any<CancellationToken>()).Returns(batch);

        await _sut.PublishAsync(events);

        var message = batchMessageStore.First();
        CloudEvent.ParseMany(message.Body).Length.Should().Be(1);
    }
}
