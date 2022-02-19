using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using EventinatR.Cosmos;
using EventinatR.Cosmos.Documents;
using Microsoft.Azure.Cosmos;

namespace EventinatR.Tests.Cosmos;
public class CosmosEventStreamTests
{
    private readonly CosmosEventStream _sut;
    private readonly Container _container = Substitute.For<Container>();
    private readonly Fixture _fixture = new Fixture();

    public CosmosEventStreamTests()
    {
        _sut = new CosmosEventStream(_container, new PartitionKey("pk"), "id", new JsonSerializerOptions());
    }

    [Fact]
    public async Task GetVersionAsync_ShouldReturnNone_WhenDocumentDoesNotExist()
    {
        _container.ReadItemAsync<StreamDocument>(Arg.Any<string>(), Arg.Any<PartitionKey>())
            .Returns<Task<ItemResponse<StreamDocument>>>(_ => throw new CosmosException(string.Empty, HttpStatusCode.NotFound, 0, string.Empty, 0));

        var result = await _sut.GetVersionAsync();

        result.Should().Be(EventStreamVersion.None);
    }

    [Fact]
    public async Task GetVersionAsync_ShouldReturnVersion_WhenDocumentExist()
    {
        var document = _fixture.Build<StreamDocument>()
            .With(x => x.Version, new EventStreamVersion(1))
            .Create();
        var response = Substitute.For<ItemResponse<StreamDocument>>();

        response.StatusCode.Returns(HttpStatusCode.OK);
        response.Resource.Returns(document);

        _container.ReadItemAsync<StreamDocument>(Arg.Any<string>(), Arg.Any<PartitionKey>())
            .Returns(response);

        var result = await _sut.GetVersionAsync();

        result.Value.Should().Be(1);
    }

    [Fact]
    public async Task ReadAsync_ShouldBeAnEmptyList_WhenStreamDoesNotExist()
    {
        var iterator = Substitute.For<FeedIterator<EventDocument>>();
        var response = Substitute.For<FeedResponse<EventDocument>>();

        response.GetEnumerator().Returns(Array.Empty<EventDocument>().AsEnumerable().GetEnumerator());

        iterator.HasMoreResults.Returns(true, false);
        iterator.ReadNextAsync().Returns(response);

        _container.GetItemQueryIterator<EventDocument>(Arg.Any<QueryDefinition>(), Arg.Any<string>(), Arg.Any<QueryRequestOptions>())
            .Returns(iterator);

        var events = await _sut.ReadAsync().ToListAsync();

        events.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadAsync_ShouldBeContainAnEvent_WhenStreamExists()
    {
        var iterator = Substitute.For<FeedIterator<EventDocument>>();
        var response = Substitute.For<FeedResponse<EventDocument>>();
        var document = _fixture.Create<EventDocument>();

        response.GetEnumerator().Returns(new[] { document }.AsEnumerable().GetEnumerator());

        iterator.HasMoreResults.Returns(true, false);
        iterator.ReadNextAsync().Returns(response);

        _container.GetItemQueryIterator<EventDocument>(Arg.Any<QueryDefinition>(), Arg.Any<string>(), Arg.Any<QueryRequestOptions>())
            .Returns(iterator);

        var events = await _sut.ReadAsync().ToListAsync();

        events.Should().NotBeEmpty();
        events.Count.Should().Be(1);
    }
}
