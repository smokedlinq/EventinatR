using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using EventinatR.InMemory;
using static EventinatR.Tests.InMemory.InMemoryEventStreamTests;

namespace EventinatR.Tests.InMemory
{
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

            fixture.Register<EventDataCollectionWithNull>(()
                => new(new EventData?[]
                {
                    new(1),
                    null,
                    new(2)
                }!));
        }
    }
}
