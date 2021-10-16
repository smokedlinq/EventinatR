using System;

namespace EventinatR.Serialization
{
    public interface IEventDataConverter
    {
        bool CanConverter(Event @event);
        object? Convert(BinaryData data);
    }
}
