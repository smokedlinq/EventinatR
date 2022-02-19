using EventinatR.Cosmos;
using Microsoft.Azure.Cosmos;

namespace EventinatR.Tests.Cosmos;
public class CosmosEventStoreTests
{
    private readonly CosmosEventStore _sut;
    private readonly CosmosEventStoreClient _client = Substitute.For<CosmosEventStoreClient>();
    private readonly CosmosEventStoreOptions _options = new();
    private readonly Container _container = Substitute.For<Container>();
    private readonly Fixture _fixture = new Fixture();

    public CosmosEventStoreTests()
    {
        _sut = new CosmosEventStore(_client, _options);
    }

    [Fact]
    public async Task GetEventStreamAsync_ShouldReturnNotNull_WhenCalled()
    {
        _client.GetContainerAsync().Returns(_container);
        var id = _fixture.Create<EventStreamId>();

        var result = await _sut.GetStreamAsync(id);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DisposeAsync_ShouldDispose_WhenClientIsCreated()
    {
        _client.GetContainerAsync().Returns(_container);
        var id = _fixture.Create<EventStreamId>();

        _ = await _sut.GetStreamAsync(id);
        await _sut.DisposeAsync();

        await _client.Received().DisposeAsync();
    }
}
