using System;
using System.Linq;

namespace EventinatR.Serialization
{
    public static class UseEventDataDeserializer
    {
        public static IEventDataDeserializerBuilder Use(this IEventDataDeserializerBuilder builder, params EventDataDeserializer[] deserializers)
        {
            foreach (var deserializer in deserializers)
            {
                foreach (var item in deserializer.Converters.Reverse())
                {
                    builder.Use(item);
                }
            }

            return builder;
        }
    }
}
