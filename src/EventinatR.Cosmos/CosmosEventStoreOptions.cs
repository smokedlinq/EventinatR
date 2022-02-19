using System.Text.Json.Serialization;
using Azure.Core;

namespace EventinatR.Cosmos;

public class CosmosEventStoreOptions
{
    public const string DefaultDatabaseId = "event-store";
    public const string DefaultContainerId = "events";

    public CosmosEventStoreOptions()
    {
        CosmosClientOptions.EnableContentResponseOnWrite = false;
        CosmosClientOptions.Serializer = new CosmosEventStoreSerializer(this);
        SerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
    }

    public JsonSerializerOptions SerializerOptions { get; set; }
    public CosmosClientOptions CosmosClientOptions { get; } = new();
    public CosmosEventStorePartitionStrategy PartitionStrategy { get; set; } = new();

    public string? AccountEndpoint { get; set; }
    public string? AuthKeyOrTokenResource { get; set; }
    public string? ConnectionString { get; set; }
    public string DatabaseId { get; set; } = DefaultDatabaseId;
    public int? Throughput { get; set; }
    public CosmosEventStoreContainerOptions Container { get; } = new(DefaultContainerId);
}
