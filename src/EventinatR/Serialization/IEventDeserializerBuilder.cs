namespace EventinatR.Serialization
{
    public interface IEventDeserializerBuilder
    {
        IEventDeserializerBuilder Use<T>() where T : IEventDataDeserializer, new();
        IEventDeserializerBuilder Use<T>(T deserializer) where T : IEventDataDeserializer;
    }
}
