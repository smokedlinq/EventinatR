using System.Net;
using EventinatR.Cosmos;
using EventinatR.Cosmos.Documents;
using Microsoft.Azure.Cosmos;

namespace EventinatR.Tests.Cosmos;

public class CosmosEventStreamSnapshotStoreTests
{
    private readonly CosmosEventStreamSnapshotStore _sut;
    private readonly Container _container = Substitute.For<Container>();
    private readonly Fixture _fixture = new();

    public CosmosEventStreamSnapshotStoreTests()
    {
        _sut = new CosmosEventStreamSnapshotStore("stream", _container, new PartitionKey("pk"), new JsonSerializerOptions());
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNullState_WhenSnapshotDoesNotExist()
    {
        _container.ReadItemAsync<StreamDocument>(Arg.Any<string>(), Arg.Any<PartitionKey>())
               .Returns<Task<ItemResponse<StreamDocument>>>(_ => throw new CosmosException(string.Empty, HttpStatusCode.NotFound, 0, string.Empty, 0));

        var snapshot = await _sut.GetAsync<object>();

        snapshot.State.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldReturnState_WhenSnapshotExist()
    {
        var response = Substitute.For<ItemResponse<SnapshotDocument>>();
        var state = JsonData.From(new object());
        var resource = _fixture.Build<SnapshotDocument>()
            .With(x => x.State, state)
            .Create();

        response.StatusCode.Returns(HttpStatusCode.OK);
        response.Resource.Returns(resource);

        _container.ReadItemAsync<SnapshotDocument>(Arg.Any<string>(), Arg.Any<PartitionKey>())
            .Returns(response);

        var snapshot = await _sut.GetAsync<object>();

        snapshot.State.Should().NotBeNull();
    }

    [Fact]
    public async Task SaveAsync_ShouldCallUpsertItemOfSnapshotDocument_WhenStateIsProvided()
    {
        var state = new object();

        _ = await _sut.SaveAsync(state, 1);

        await _container.Received(1).UpsertItemAsync(Arg.Any<SnapshotDocument>(), Arg.Any<PartitionKey>(), Arg.Any<ItemRequestOptions>());
    }
}
