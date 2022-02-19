namespace EventinatR.Tests;

public class EventStreamVersionTests
{
    [Fact]
    public void ImplicitCastFromInt64()
    {
        EventStreamVersion sut = 1L;
        sut.Value.Should().Be(1L);
    }

    [Fact]
    public void ImplicitCastFromInt32()
    {
        EventStreamVersion sut = 1;
        sut.Value.Should().Be(1);
    }


    [Fact]
    public void ImplicitCastToInt64()
    {
        long value = (EventStreamVersion)1;
        value.Should().Be(1);
    }

    [Fact]
    public void AddOperator()
    {
        var x = (EventStreamVersion)1;
        var y = (EventStreamVersion)1;
        var result = x + y;
        result.Value.Should().Be(2);
    }

    [Fact]
    public void SubtractOperator()
    {
        var x = (EventStreamVersion)2;
        var y = (EventStreamVersion)1;
        var result = x - y;
        result.Value.Should().Be(1);
    }

    [Fact]
    public void IncrementOperator()
    {
        var version = (EventStreamVersion)1;
        version++;
        version.Value.Should().Be(2);
    }

    [Fact]
    public void DecrementOperator()
    {
        var version = (EventStreamVersion)2;
        version--;
        version.Value.Should().Be(1);
    }

    [Fact]
    public void LessThanOperator()
    {
        var x = (EventStreamVersion)1;
        var y = (EventStreamVersion)2;
        var result = x < y;
        result.Should().BeTrue();
    }

    [Fact]
    public void LessThanOrEqualOperator()
    {
        var x = (EventStreamVersion)1;
        var y = (EventStreamVersion)1;
        var result = x <= y;
        result.Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOperator()
    {
        var x = (EventStreamVersion)2;
        var y = (EventStreamVersion)1;
        var result = x > y;
        result.Should().BeTrue();
    }

    [Fact]
    public void GreaterThanOrEqualOperator()
    {
        var x = (EventStreamVersion)1;
        var y = (EventStreamVersion)1;
        var result = x >= y;
        result.Should().BeTrue();
    }
}
