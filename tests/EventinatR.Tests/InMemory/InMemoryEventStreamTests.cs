using EventinatR.InMemory;

namespace EventinatR.Tests.InMemory;

public class InMemoryEventStreamTests
{
    private readonly InMemoryEventStream _sut;
    private readonly Fixture _fixture = new();

    public InMemoryEventStreamTests()
    {
        _sut = new InMemoryEventStream(_fixture.Create<EventStreamId>());
    }

    [Fact]
    public async Task ReadAsync_ShouldNotThrow_WhenStreamIsEmpty()
    {
        var act = _sut.Invoking(x => x.ReadAsync().ToListAsync().AsTask());
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ReadAsync_ShouldNotReturnNull_WhenStreamIsEmpty()
    {
        var events = await _sut.ReadAsync().ToListAsync();
        events.Should().NotBeNull();
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnAllEvents_WhenStreamHasEvents()
    {
        var events = _fixture.CreateMany<object>(1);
        await _sut.AppendAsync(events);

        var streamEvents = await _sut.ReadAsync().ToListAsync();

        streamEvents.Count.Should().Be(1);
    }

    [Fact]
    public async Task AppendAsync_ShouldNotThrow_WhenStreamIsEmpty()
    {
        var events = _fixture.CreateMany<object>();
        var act = _sut.Invoking(x => x.AppendAsync(events));
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task AppendAsync_ShouldThrow_WhenCollectionContainsNullItem()
    {
        var events = _fixture.CreateMany<object>().Append(null).Cast<object>();
        var act = _sut.Invoking(x => x.AppendAsync(events));
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AppendAsync_ShouldWriteSnapshot_WhenStateIsProvided()
    {
        var events = _fixture.CreateMany<object>();
        var state = new object();

        await _sut.AppendAsync(events, state);

        var snapshot = await _sut.Snapshots.GetAsync<object>();

        snapshot.Should().NotBeNull();
        snapshot.State.Should().NotBeNull();
    }
}
