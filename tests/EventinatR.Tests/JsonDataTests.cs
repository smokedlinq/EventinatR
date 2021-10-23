namespace EventinatR.Tests
{
    public class JsonDataTests
    {
        public abstract record BaseEvent;
        public record TestEvent(int Value) : BaseEvent;
        public record LooksLikeTestEvent(int Value);

        [Fact]
        public void CanConvertFromObject()
        {
            var value = new TestEvent(1);
            var data = JsonData.From(value);

            data.Type.Assembly.Should().Be(value.GetType().Assembly.GetName().FullName);
            data.Value.ToString().Should().Be(@"{""value"":1}");
        }

        [Theory]
        [MoqData]
        public void CanConvertToObject(TestEvent value)
        {
            var data = JsonData.From(value);
            var obj = data.As<TestEvent>();

            obj.Should().Be(value);
        }

        [Theory]
        [MoqData]
        public void CannotConvertToObjectWhenTargetTypeIsNotFound(TestEvent value)
        {
            var data = JsonData.From(value);
            var obj = data.As<string>();

            obj.Should().BeNull();
        }

        [Theory]
        [MoqData]
        public void CanConvertToObjectWithBaseTarget(TestEvent value)
        {
            var data = JsonData.From(value);
            var obj = data.As<BaseEvent>();

            obj.Should().NotBeNull();
        }

        [Theory]
        [MoqData]
        public void CanConvertToObjectThatLooksTheSameWhenTypeNotFound(TestEvent value)
        {
            var data = JsonData.From(value);
            data = data with
            {
                Type = new JsonDataType("NotAType", "NotAnAssembly")
            };
            var obj = data.As<LooksLikeTestEvent>();

            obj.Should().NotBeNull();
        }

        [Theory]
        [MoqData]
        public void CanConvertToJsonDocument(TestEvent value)
        {
            var data = JsonData.From(value);
            var doc = data.As<JsonDocument>();

            doc.Should().NotBeNull();
        }

        [Theory]
        [MoqData]
        public void CanConvertToJsonElement(TestEvent value)
        {
            var data = JsonData.From(value);
            var element = data.As<JsonElement>();

            element.Should().NotBeNull();
        }
    }
}
