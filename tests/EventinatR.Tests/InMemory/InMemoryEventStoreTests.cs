using EventinatR.InMemory;

namespace EventinatR.Tests.InMemory;

public class InMemoryEventStoreTests
{
    private readonly InMemoryEventStore _sut;

    public InMemoryEventStoreTests()
    {
        _sut = new InMemoryEventStore();
    }

    [Fact]
    public async Task GetStreamAsync_ShouldReturnTheSameStream_WhenStreamAlreadyExists()
    {
        var stream1 = await _sut.GetStreamAsync("id");

        var stream2 = await _sut.GetStreamAsync("id");

        stream2.Should().Be(stream1);
    }
}
