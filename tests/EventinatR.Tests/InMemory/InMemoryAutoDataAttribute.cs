using EventinatR.InMemory;

namespace EventinatR.Tests.InMemory;

public record TestEvent(int Value);
public record class TestEventCollectionWithNull(IEnumerable<TestEvent> Events);
public record TestState(int State);

internal class InMemoryAutoDataAttribute : MoqDataAttribute
{
    public InMemoryAutoDataAttribute()
        : base(Customize)
    {
    }

    private static void Customize(IFixture fixture)
    {
        fixture.Register<EventStore>(()
            => new InMemoryEventStore());

        fixture.Register<EventStream>(()
            => fixture.Create<EventStore>().GetStreamAsync(fixture.Create<EventStreamId>()).Result);

        fixture.Register<TestEventCollectionWithNull>(()
            => new(new TestEvent?[]
            {
                    new(1),
                    null,
                    new(2)
            }!));
    }
}
