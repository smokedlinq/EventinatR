using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventinatR.Serialization;
using FluentAssertions;
using Xunit;

namespace EventinatR.Tests.Serialization
{
    public class EventConverterTests
    {
        [Fact]
        public void ReturnsFalseWhenCannotConvert()
        {
            var converter = new EventConverter(builder => { });
            var @event = new Event(new EventStreamId("id"), 0, DateTimeOffset.Now, "data", BinaryData.FromObjectAsJson(new { }));

            converter.TryConvert(@event, out TestData? result).Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void CanConfigureWithDefaultConverter()
        {
            var converter = new EventConverter(builder => builder.Use<TestData>());
            var data = new TestData("value");
            var @event = new Event(new EventStreamId("id"), 0, DateTimeOffset.Now, typeof(TestData).FullName!, BinaryData.FromObjectAsJson(data));

            converter.TryConvert(@event, out TestData? result).Should().BeTrue();
            result.Should().NotBeNull();
            result!.StringProperty.Should().Be(data.StringProperty);
        }

        private record TestData(string StringProperty);
    }
}
