using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using EventinatR.Publishers.Queues;

namespace EventinatR.Tests.Publishers;

public class QueueEventPublisherTests
{
    [Theory]
    [Fixtures]
    public async Task PublishAsync(Event[] events)
    {
        var queue = Substitute.For<QueueClient>();
        var response = Substitute.For<Response<SendReceipt>>();

        queue.SendMessageAsync(Arg.Any<BinaryData>()).Returns(response);

        var sut = new QueueEventPublisher(queue);

        await sut.PublishAsync(events);

        await queue.Received().SendMessageAsync(Arg.Any<BinaryData>());
    }
}
