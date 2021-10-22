namespace EventinatR.Serialization;

public static class UseEventConverter
{
    public static IEventConverterBuilder Use(this IEventConverterBuilder builder, params EventConverter[] converters)
    {
        foreach (var converter in converters)
        {
            foreach (var item in converter.Converters.Reverse())
            {
                builder.Use(item);
            }
        }

        return builder;
    }
}
