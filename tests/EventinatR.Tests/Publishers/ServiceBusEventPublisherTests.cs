using Azure.Messaging.ServiceBus;
using EventinatR.Publishers.ServiceBus;

namespace EventinatR.Tests.Publishers;

public class ServiceBusEventPublisherTests
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

        var sut = new ServiceBusEventPublisher(sender);

        await sut.PublishAsync(events);

        await sender.Received().SendMessagesAsync(Arg.Any<ServiceBusMessageBatch>(), Arg.Any<CancellationToken>());

        batchMessageStore.Count.Should().Be(events.Length);
    }
}
