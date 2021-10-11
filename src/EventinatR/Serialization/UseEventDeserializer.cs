using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventinatR.Serialization
{
    public static class UseEventDeserializer
    {
        public static IEventDeserializerBuilder Use(this IEventDeserializerBuilder builder, params EventDeserializer[] deserializers)
        {
            foreach (var deserializer in deserializers)
            {
                foreach (var item in deserializer.Deserializers.Reverse())
                {
                    builder.Use(item);
                }
            }

            return builder;
        }
    }
}
