namespace EventinatR.Tests;

internal class MoqDataAttribute : AutoDataAttribute
{
    public MoqDataAttribute()
        : base(Customize)
    {
    }

    public MoqDataAttribute(Action<IFixture> customize)
        : base(() =>
        {
            var fixture = Customize();
            customize(fixture);
            return fixture;
        })
    {
    }

    private static IFixture Customize()
    {
        var fixture = new Fixture();

        fixture.Customize(new AutoMoqCustomization());

        return fixture;
    }
}
