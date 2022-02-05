namespace EventinatR.Tests.Cosmos;

public record TestEvent(int Value);
public record TestState(int State);

internal class CosmosFixturesAttribute : FixturesAttribute
{
    public CosmosFixturesAttribute()
        : base(Customize)
    {
    }

    private static void Customize(IFixture fixture)
    {
    }
}
