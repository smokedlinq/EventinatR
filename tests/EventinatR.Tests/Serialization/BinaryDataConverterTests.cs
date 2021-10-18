using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EventinatR.Serialization;
using FluentAssertions;
using Xunit;

namespace EventinatR.Tests.Serialization
{
    public class BinaryDataConverterTests
    {
        [Fact]
        public void CanConvertFromJsonToObject()
        {
            var json = @"{""Data"":{""StringProperty"":""string"",""Int32Property"":5,""BooleanProperty"":true,""DoubleProperty"":9.95}}";
            var data = JsonSerializer.Deserialize<TestData>(json);
            var obj = data?.Data.ToObjectFromJson<TestObject>();

            obj.Should().NotBeNull();
            obj!.StringProperty.Should().Be("string");
            obj.Int32Property.Should().Be(5);
            obj.BooleanProperty.Should().BeTrue();
            obj.DoubleProperty.Should().Be(9.95);
        }

        [Fact]
        public void CanConvertFromObjectToJson()
        {
            var obj = new TestObject("string", 5, true, 9.95);
            var data = new TestData(BinaryData.FromObjectAsJson(obj));
            var json = JsonSerializer.Serialize(data);

            json.Should().Be(@"{""Data"":{""StringProperty"":""string"",""Int32Property"":5,""BooleanProperty"":true,""DoubleProperty"":9.95}}");
        }

        private record TestData([property: JsonConverter(typeof(BinaryDataConverter))]BinaryData Data);
        private record TestObject(string StringProperty, int Int32Property, bool BooleanProperty,double DoubleProperty);
    }
}
