using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using EventinatR.Publishers.ServiceBus;

namespace EventinatR.Tests.Publishers;

public class ServiceBusCloudEventPublisherTests
{
    [Theory]
    [Fixtures]
    public async Task PublishAsync(Event[] events)
    {
        var sender = Substitute.For<ServiceBusSender>();
        var batchMessageStore = new List<ServiceBusMessage>();
        var batch = ServiceBusModelFactory.ServiceBusMessageBatch(long.MaxValue, batchMessageStore);

        sender.CreateMessageBatchAsync(Arg.Any<CancellationToken>()).Returns(batch);

        sender.SendMessagesAsync(Arg.Any<ServiceBusMessageBatch>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = new ServiceBusCloudEventPublisher(sender, "/local");

        await sut.PublishAsync(events);

        await sender.Received().SendMessagesAsync(Arg.Any<ServiceBusMessageBatch>(), Arg.Any<CancellationToken>());

        batchMessageStore.Count.Should().Be(events.Length);
    }

    [Theory]
    [Fixtures]
    public async Task MessageBodyJson_ShouldParsableByCloudEvent(Event[] events)
    {
        var sender = Substitute.For<ServiceBusSender>();
        var batchMessageStore = new List<ServiceBusMessage>();
        var batch = ServiceBusModelFactory.ServiceBusMessageBatch(long.MaxValue, batchMessageStore);

        sender.CreateMessageBatchAsync(Arg.Any<CancellationToken>()).Returns(batch);

        sender.SendMessagesAsync(Arg.Any<ServiceBusMessageBatch>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = new ServiceBusCloudEventPublisher(sender, "/local");

        await sut.PublishAsync(events);

        var message = batchMessageStore.First();

        CloudEvent.ParseMany(message.Body).Length.Should().Be(1);
    }
}
