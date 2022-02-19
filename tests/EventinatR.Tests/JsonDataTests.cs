namespace EventinatR.Tests;

public class JsonDataTests
{
    public abstract record BaseEvent;
    public record TestEvent(int Value) : BaseEvent;
    public record LooksLikeTestEvent(int Value);
    public record NotATestThatIsConvertable;

    private readonly TestEvent _value;
    private readonly Fixture _fixture = new();

    public JsonDataTests()
    {
        _value = _fixture.Create<TestEvent>();
    }

    [Fact]
    public void CanConvertFromObject()
    {
        var value = new TestEvent(1);
        var data = JsonData.From(value);

        data.Type.Assembly.Should().Be(value.GetType().Assembly.GetName().FullName);
        data.Value.ToString().Should().Be(/*lang=json,strict*/ @"{""value"":1}");
    }

    [Fact]
    public void CanConvertToObject()
    {
        var data = JsonData.From(_value);
        var obj = data.As<TestEvent>();

        obj.Should().Be(_value);
    }

    [Fact]
    public void CannotConvertToObjectWhenTargetTypeIsNotFound()
    {
        var data = JsonData.From(_value);
        var obj = data.As<NotATestThatIsConvertable>();

        obj.Should().BeNull();
    }

    [Fact]
    public void CanConvertToObjectWithBaseTarget()
    {
        var data = JsonData.From(_value);
        var obj = data.As<BaseEvent>();

        obj.Should().NotBeNull();
    }

    [Fact]
    public void CanConvertToObjectThatLooksTheSameWhenTypeNotFound()
    {
        var data = JsonData.From(_value);
        data = data with
        {
            Type = new JsonDataType("NotAType", "NotAnAssembly")
        };
        var obj = data.As<LooksLikeTestEvent>();

        obj.Should().NotBeNull();
    }

    [Fact]
    public void CanConvertToJsonDocument()
    {
        var data = JsonData.From(_value);
        var doc = data.As<JsonDocument>();

        doc.Should().NotBeNull();
    }

    [Fact]
    public void CanConvertToJsonElement()
    {
        var data = JsonData.From(_value);
        var element = data.As<JsonElement>();

        element.Should().NotBeNull();
    }

    [Fact]
    public void CanConvertToBinaryData()
    {
        var data = JsonData.From(_value);
        var obj = data.As<BinaryData>();

        obj.Should().NotBeNull();
    }

    [Fact]
    public void CanConvertToString()
    {
        var data = JsonData.From(_value);
        var obj = data.As<string>();

        obj.Should().NotBeNull();
    }

    [Fact]
    public void CanConvertToByteArray()
    {
        var data = JsonData.From(_value);
        var array = data.As<byte[]>();

        array.Should().NotBeNull();
        array!.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CanConvertToReadOnlyMemoryOfByte()
    {
        var data = JsonData.From(_value);
        var memory = data.As<ReadOnlyMemory<byte>>();

        memory.Should().NotBeNull();
        memory!.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CanConvertToStream()
    {
        var data = JsonData.From(_value);
        var stream = data.As<Stream>();

        stream.Should().NotBeNull();
    }
}
