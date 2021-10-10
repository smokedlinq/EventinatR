using System;

namespace EventinatR
{
    public interface IEventConverter
    {
        object? Convert(BinaryData data);
    }
}
