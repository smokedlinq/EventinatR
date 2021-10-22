using System.Text.Json.Serialization;

namespace EventinatR.Cosmos;

public static class CosmosEventStoreChangeFeed
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public static IEnumerable<Event> ParseEvents(string json)
        => JsonSerializer.Deserialize<ChangeFeedDocument[]>(json, SerializerOptions)?.Where(x => x.IsEvent)?.Select(x => x.ToEventDocument()!.ToEvent()) ?? Array.Empty<Event>();
}
