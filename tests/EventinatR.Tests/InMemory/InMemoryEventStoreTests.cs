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
    public class InMemoryEventStoreTests
    {
        [Theory]
        [InMemoryAutoData]
        public async Task WhenStreamAlreadyExistsGetStreamAsyncReturnsTheStream(InMemoryEventStore store)
        {
            var stream1 = await store.GetStreamAsync("id");
            var stream2 = await store.GetStreamAsync("id");

            stream2.Should().Be(stream1);
        }
    }
}
