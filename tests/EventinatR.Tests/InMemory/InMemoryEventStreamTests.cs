using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventinatR.InMemory;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventinatR.Tests.InMemory
{
    public class InMemoryEventStreamTests
    {
        [Theory]
        [InMemoryAutoData]
        public async Task WhenStreamAlreadyExistsGetStreamAsyncReturnsTheStream(InMemoryEventStore store)
        {
            var stream1 = await store.GetStreamAsync("id");
            var stream2 = await store.GetStreamAsync("id");

            stream2.Should().Be(stream1);
        }

        [Theory]
        [InMemoryAutoData]
        public async Task WhenStreamIsEmptyReadAsyncDoesNotThrow(EventStream stream)
        {
            var act = stream.Invoking(x => x.ReadAsync().ToListAsync().AsTask());

            await act.Should().NotThrowAsync();
        }

        [Theory]
        [InMemoryAutoData]
        public async Task WhenStreamIsEmptyReadAsyncReturnsNoEvents(EventStream stream)
        {
            var hasEvents = await stream.ReadAsync().AnyAsync();
            hasEvents.Should().BeFalse();
        }

        [Theory]
        [InMemoryAutoData]
        public async Task WhenStreamIsEmptyAppendAsyncDoesNotThrow(EventStream stream, EventData[] events)
        {
            var act = stream.Invoking(x => x.AppendAsync(events));
            await act.Should().NotThrowAsync();
        }

        [Theory]
        [InMemoryAutoData]
        public async Task WhenStreamIsEmptyAppendAsyncWithNullItemShouldThrow(EventStream stream, EventDataCollectionWithNull data)
        {
            var act = stream.Invoking(x => x.AppendAsync(data.Events));
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Theory]
        [InMemoryAutoData]
        public async Task WhenStreamHasEventsReadAsyncReturnsAllEvents(EventStream stream, EventData data)
        {
            _ = await stream.AppendAsync(data);

            var events = await stream.ReadAsync().ToListAsync();

            events.Count.Should().Be(1);
        }

        [Theory]
        [InMemoryAutoData]
        public async Task AfterAppendAsyncWhenReadAsyncThenTheVersionIsEqual(EventStream stream, EventData data)
        {
            var version = await stream.AppendAsync(data);

            var @event = await stream.ReadAsync().LastAsync();

            @event.Version.Should().Be(version);
        }

        public record EventData(int Value);
        public record EventDataCollectionWithNull(IEnumerable<EventData> Events);
    }
}
