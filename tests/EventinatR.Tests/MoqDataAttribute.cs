namespace EventinatR.Tests;

internal abstract class MoqDataAttribute : AutoDataAttribute
{
    public MoqDataAttribute(Action<IFixture> customize)
        : base(() =>
        {
            var fixture = new Fixture();

            fixture.Customize(new AutoMoqCustomization());

            customize(fixture);

            return fixture;
        })
    {
    }
}
