using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventinatR.Cosmos;

namespace EventinatR.Tests.Cosmos;

public class CosmosEventStoreChangeFeedTests
{
    [Fact]
    public void ParseEvents_ShouldReturnEvents_WhenChangeFeedJsonHasEventDocument()
    {
        var json = @"[
            {
                ""type"": ""event"",
                ""stream"": ""stream"",
                ""version"": 1,
                ""transaction"": { ""version"": 1, ""count"": 1 },
                ""timestamp"": ""2021-02-18T21:50:00.000Z"",
                ""data"": {
                    ""type"": { ""name"": ""System.Object"", ""assembly"": ""System.Private.CoreLib"" },
                    ""value"": { }
                }
            }
        ]";

        var events = CosmosEventStoreChangeFeed.ParseEvents(json);

        events.Should().HaveCount(1);
    }

    [Fact]
    public void ParseEvents_ShouldReturnNoEvents_WhenChangeFeedJsonHasStreamDocument()
    {
        var json = @"[
            {
                ""type"": ""stream"",
                ""stream"": ""stream"",
                ""version"": 1
            }
        ]";

        var events = CosmosEventStoreChangeFeed.ParseEvents(json);

        events.Should().BeEmpty();
    }

    [Fact]
    public void ParseEvents_ShouldReturnOnlyEvent_WhenChangeFeedJsonHasMultipleDocumentTypes()
    {
        var json = @"[
            {
                ""type"": ""stream"",
                ""stream"": ""stream"",
                ""version"": 1
            },
            {
                ""type"": ""event"",
                ""stream"": ""stream"",
                ""version"": 1,
                ""transaction"": { ""version"": 1, ""count"": 1 },
                ""timestamp"": ""2021-02-18T21:50:00.000Z"",
                ""data"": {
                    ""type"": { ""name"": ""System.Object"", ""assembly"": ""System.Private.CoreLib"" },
                    ""value"": { }
                }
            }
        ]";

        var events = CosmosEventStoreChangeFeed.ParseEvents(json);

        events.Should().HaveCount(1);
    }
}
