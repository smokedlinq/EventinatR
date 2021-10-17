namespace EventinatR.Serialization
{
    public interface IEventConverterBuilder
    {
        IEventConverterBuilder Use<T>() where T : IEventDataConverter, new();
        IEventConverterBuilder Use<T>(T converter) where T : IEventDataConverter;
    }
}
