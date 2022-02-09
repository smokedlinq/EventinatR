using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EventinatR;

public class JsonDataOptions
{
    public JsonDataOptions(JsonSerializerOptions? serializerOptions = null)
    {
         SerializerOptions = serializerOptions ?? new JsonSerializerOptions()
         {
             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
             DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
             DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
         };
    }

    public JsonSerializerOptions SerializerOptions { get; }

    public Dictionary<string, Type> Types { get; } = new(StringComparer.OrdinalIgnoreCase);
}
