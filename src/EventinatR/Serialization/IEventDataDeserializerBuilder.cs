namespace EventinatR.Serialization
{
    public interface IEventDataDeserializerBuilder
    {
        IEventDataDeserializerBuilder Use<T>() where T : IEventDataConverter, new();
        IEventDataDeserializerBuilder Use<T>(T deserializer) where T : IEventDataConverter;
    }
}
