using System;
using System.Collections;
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
        public record EventData(int Value);
        public record class EventDataCollectionWithNull(IEnumerable<EventData> Events);
        public record EventState(int State);

        [Theory, InMemoryAutoData]
        public async Task WhenStreamIsEmptyThenReadAsyncShouldNotThrow(EventStream stream)
        {
            var act = stream.Invoking(x => x.ReadAsync().ToListAsync().AsTask());

            await act.Should().NotThrowAsync();
        }

        [Theory, InMemoryAutoData]
        public async Task WhenStreamIsEmptyThenReadAsyncShouldReturnNoEvents(EventStream stream)
        {
            var hasEvents = await stream.ReadAsync().AnyAsync();
            hasEvents.Should().BeFalse();
        }

        [Theory, InMemoryAutoData]
        public async Task WhenStreamIsEmptyThenAppendAsyncShouldNotThrow(EventStream stream, EventData[] events)
        {
            var act = stream.Invoking(x => x.AppendAsync(events));
            await act.Should().NotThrowAsync();
        }

        [Theory, InMemoryAutoData]
        public async Task WhenStreamIsEmptyThenAppendAsyncWithNullItemShouldThrow(EventStream stream, EventDataCollectionWithNull data)
        {
            var act = stream.Invoking(x => x.AppendAsync(data.Events));
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Theory, InMemoryAutoData]
        public async Task WhenStreamHasEventsThenReadAsyncShouldReturnAllEvents(EventStream stream, EventData data)
        {
            _ = await stream.AppendAsync(new[] { data });

            var events = await stream.ReadAsync().ToListAsync();

            events.Count.Should().Be(1);
        }

        [Theory, InMemoryAutoData]
        public async Task WhenReadAsyncThenLastEventVersionShouldBeTheVersionReturnedByLastAppendAsync(EventStream stream, EventData data)
        {
            var version = await stream.AppendAsync(new[] { data });

            var @event = await stream.ReadAsync().LastAsync();

            @event.Version.Should().Be(version);
        }

        [Theory, InMemoryAutoData]
        public async Task WhenAppendAsyncThenVersionShouldNotBeNone(EventStream stream, EventData data)
        {
            var version = await stream.AppendAsync(new[] { data });
            version.Should().NotBe(EventStreamVersion.None);
        }

        [Theory, InMemoryAutoData]
        public async Task WhenWriteSnapshotToEmptyStreamShouldNotThrow(EventStream stream, EventState state)
        {
            var act = stream.Invoking(x => x.WriteSnapshotAsync(state, EventStreamVersion.None));
            await act.Should().NotThrowAsync();
        }

        [Theory, InMemoryAutoData]
        public async Task WhenReadSnapshotFromEmptyStreamShouldNotThrow(EventStream stream)
        {
            var act = stream.Invoking(x => x.ReadSnapshotAsync<EventState>());
            await act.Should().NotThrowAsync();
        }

        [Theory, InMemoryAutoData]
        public async Task WhenReadSnapshotFromEmptyStreamStateShouldBeNull(EventStream stream)
        {
            var snapshot = await stream.ReadSnapshotAsync<EventState>();
            snapshot.State.Should().BeNull();
        }

        [Theory, InMemoryAutoData]
        public async Task WhenReadSnapshotFromEmptyStreamVersionShouldBeNone(EventStream stream)
        {
            var snapshot = await stream.ReadSnapshotAsync<EventState>();
            snapshot.Version.Should().Be(EventStreamVersion.None);
        }

        [Theory, InMemoryAutoData]
        public async Task WhenReadSnapshotFromStreamThenShouldNotThrow(EventStream stream, EventState state)
        {
            await stream.WriteSnapshotAsync(state, EventStreamVersion.None);
            var act = stream.Invoking(x => x.ReadSnapshotAsync<EventState>());
            await act.Should().NotThrowAsync();
        }

        [Theory, InMemoryAutoData]
        public async Task WhenReadSnapshotFromStreamThenShouldStateShouldNotBeNull(EventStream stream, EventState state)
        {
            await stream.WriteSnapshotAsync(state, EventStreamVersion.None);
            var snapshot = await stream.ReadSnapshotAsync<EventState>();
            snapshot.State.Should().NotBeNull();
        }

        [Theory, InMemoryAutoData]
        public async Task WhenReadSnapshotCreatedFromLastEventThenReadAsyncShouldReturnNoEvents(EventStream stream, EventData[] events, EventState state)
        {
            var version = await stream.AppendAsync(events);
            await stream.WriteSnapshotAsync(state, version);
            var snapshot = await stream.ReadSnapshotAsync<EventState>();
            var snapshotEvents = await snapshot.ReadAsync().ToListAsync();
            snapshotEvents.Count.Should().Be(0);
        }

        [Theory, InMemoryAutoData]
        public async Task WhenReadSnapshotCreatedBeforeLastEventThenReadAsyncShouldReturnAfterSnapshotVersion(EventStream stream, EventData[] events, EventState state)
        {
            var version = await stream.AppendAsync(events);
            await stream.WriteSnapshotAsync(state, version);
            version = await stream.AppendAsync(events);
            var snapshot = await stream.ReadSnapshotAsync<EventState>();
            var snapshotEvents = await snapshot.ReadAsync().ToListAsync();
            snapshotEvents.Count.Should().Be(events.Length);
        }
    }
}
