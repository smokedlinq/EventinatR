using EventinatR.InMemory;

namespace EventinatR.Tests.InMemory;

public class InMemoryEventStreamSnapshotStoreTests
{
    private readonly InMemoryEventStreamSnapshotStore _sut;
    private readonly Fixture _fixture = new();

    public InMemoryEventStreamSnapshotStoreTests()
    {
        var stream = new InMemoryEventStream(_fixture.Create<EventStreamId>());
        _sut = new InMemoryEventStreamSnapshotStore(stream);
    }

    [Fact]
    public async Task ReadSnapshotAsync_ShouldReturnStateAsNull_WhenSnapshotDoesNotExist()
    {
        var snapshot = await _sut.GetAsync<object>();
        snapshot.Should().NotBeNull();
        snapshot.State.Should().BeNull();
    }
}
