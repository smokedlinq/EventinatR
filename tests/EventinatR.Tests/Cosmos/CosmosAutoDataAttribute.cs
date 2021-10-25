using EventinatR.Cosmos;

namespace EventinatR.Tests.Cosmos;

public record TestEvent(int Value);
public record TestState(int State);

internal class CosmosAutoDataAttribute : MoqDataAttribute
{
    public CosmosAutoDataAttribute()
        : base(Customize)
    {
    }

    private static void Customize(IFixture fixture)
    {
    }
}
