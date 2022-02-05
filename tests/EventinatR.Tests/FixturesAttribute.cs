namespace EventinatR.Tests;

internal class FixturesAttribute : AutoDataAttribute
{
    public FixturesAttribute()
        : base(Customize)
    {
    }

    public FixturesAttribute(Action<IFixture> customize)
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

        fixture.Customize(new AutoNSubstituteCustomization());

        fixture.Register(() =>
        {
            var version = fixture.Create<EventStreamVersion>();
            var e = new Event(
                fixture.Create<EventStreamId>(),
                version,
                new EventStreamTransaction(version, 1),
                DateTimeOffset.Now,
                JsonData.From(fixture.Create<string>()));
            return e;
        });

        return fixture;
    }
}
