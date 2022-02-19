namespace EventinatR.Tests;

public class EventStreamIdTests
{
    [Fact]
    public void ImplicitCastFromString()
    {
        EventStreamId sut = "id";
        sut.Value.Should().Be("id");
    }


    [Fact]
    public void ValueToString()
    {
        EventStreamId sut = "id";
        string value = sut.ToString();
        value.Should().Be("id");
    }

    [Fact]
    public void ConvertTo()
    {
        var sut = EventStreamId.ConvertTo<EventStreamId>();
        sut.Value.Should().Be("event-stream-id");
    }

    [Fact]
    public void Concat()
    {
        EventStreamId x = "1";
        EventStreamId y = "2";
        var result = x + y;
        result.Value.Should().Be("1:2");
    }
}
