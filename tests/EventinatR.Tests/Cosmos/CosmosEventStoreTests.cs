using EventinatR.Cosmos;
using Microsoft.Azure.Cosmos;

namespace EventinatR.Tests.Cosmos;
public class CosmosEventStoreTests
{
    private readonly CosmosEventStore _sut;
    private readonly CosmosEventStoreOptions _options = Substitute.For<CosmosEventStoreOptions>();
    private readonly CosmosClient _client = Substitute.For<CosmosClient>();
    private readonly Container _container = Substitute.For<Container>();
    private readonly Fixture _fixture = new Fixture();

    public CosmosEventStoreTests()
    {
        _sut = new CosmosEventStore(null, _options);
    }

    [Fact]
    public async Task GetEventStreamAsync_ShouldReturnNotNull_WhenCalled()
    {
        _client.GetContainer(Arg.Any<string>(), Arg.Any<string>()).Returns(_container);
        _options.CreateAndInitializeCosmosClientAsync().Returns(_client);
        var id = _fixture.Create<EventStreamId>();

        var result = await _sut.GetStreamAsync(id);

        result.Should().NotBeNull();
    }
}
