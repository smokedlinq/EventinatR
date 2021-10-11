using System;

namespace EventinatR.Serialization
{
    public interface IEventDataDeserializer
    {
        bool CanDeserialize(Event @event);
        object? Deserialize(BinaryData data);
    }
}
