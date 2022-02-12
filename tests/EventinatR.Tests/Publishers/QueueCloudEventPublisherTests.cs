using Azure;
using Azure.Messaging;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using EventinatR.Publishers.Queues;

namespace EventinatR.Tests.Publishers;

public class QueueCloudEventPublisherTests
{
    [Theory]
    [Fixtures]
    public async Task PublishAsync(Event[] events)
    {
        var queue = Substitute.For<QueueClient>();
        var response = Substitute.For<Response<SendReceipt>>();

        queue.SendMessageAsync(Arg.Any<BinaryData>()).Returns(response);

        var sut = new QueueCloudEventPublisher(queue, "/source");

        await sut.PublishAsync(events);

        await queue.Received().SendMessageAsync(Arg.Any<BinaryData>());
    }

    [Theory]
    [Fixtures]
    public async Task MessageBodyJson_ShouldParsableByCloudEvent(Event[] events)
    {
        var queue = Substitute.For<QueueClient>();
        var response = Substitute.For<Response<SendReceipt>>();
        var messages = new List<BinaryData>();

        queue.SendMessageAsync(Arg.Any<BinaryData>()).Returns(x =>
        {
            var message = x.Arg<BinaryData>();
            messages.Add(message);
            return response;
        });

        var sut = new QueueCloudEventPublisher(queue, "/source");

        await sut.PublishAsync(events);

        var message = messages.First();

        CloudEvent.ParseMany(message).Length.Should().Be(1);
    }
}
