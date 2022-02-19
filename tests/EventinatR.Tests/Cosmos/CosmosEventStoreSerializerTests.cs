using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using EventinatR.Cosmos;
using EventinatR.Cosmos.Documents;

namespace EventinatR.Tests.Cosmos;

public class CosmosEventStoreSerializerTests
{
    private readonly CosmosEventStoreSerializer _sut;
    private readonly Fixture _fixture = new();

    public CosmosEventStoreSerializerTests()
    {
        _sut = new CosmosEventStoreSerializer(new CosmosEventStoreOptions());
    }

    [Fact]
    public void ToStream_ShouldProduceExpectedJson_WhenEventDocumentIsSerialized()
    {
        var data = JsonData.From(new object());
        var evt = _fixture.Build<Event>()
            .With(x => x.Data, data)
            .With(x => x.Version, new EventStreamVersion(1))
            .With(x => x.Timestamp, new DateTimeOffset(2022, 2, 19, 22, 55, 0, TimeSpan.Zero))
            .Create();
        var document = EventDocument.FromEvent("stream", "id", evt);

        var json = JsonNode.Parse(_sut.ToStream(document))!;

        json["type"]?.GetValue<string>().Should().Be(DocumentTypes.Event);
        json["stream"]?.GetValue<string>().Should().Be("stream");
        json["id"]?.GetValue<string>().Should().Be("id");
        json["version"]?.GetValue<long>().Should().Be(1);
        json["timestamp"]?.GetValue<string>().Should().Be("2022-02-19T22:55:00+00:00");
        json["data"]?["type"]?["name"]?.GetValue<string>().Should().Be("System.Object");
        json["data"]?["type"]?["assembly"]?.GetValue<string>().Should().NotBeEmpty();
        json["data"]?["value"]?.Should().NotBeNull();
    }

    [Fact]
    public void ToStream_ShouldProduceExpectedJson_WhenStreamDocumentIsSerialized()
    {
        var document = new StreamDocument("stream", "id", 1);

        var json = JsonNode.Parse(_sut.ToStream(document))!;

        json["type"]?.GetValue<string>().Should().Be(DocumentTypes.Stream);
        json["stream"]?.GetValue<string>().Should().Be("stream");
        json["id"]?.GetValue<string>().Should().Be("id");
        json["version"]?.GetValue<long>().Should().Be(1);
    }

    [Fact]
    public void ToStream_ShouldProduceExpectedJson_WhenSnapshotDocumentIsSerialized()
    {
        var state = JsonData.From(new object());
        var document = new SnapshotDocument("stream", "id", 1, state);

        var json = JsonNode.Parse(_sut.ToStream(document))!;

        json["type"]?.GetValue<string>().Should().Be(DocumentTypes.Snapshot);
        json["stream"]?.GetValue<string>().Should().Be("stream");
        json["id"]?.GetValue<string>().Should().Be("id");
        json["version"]?.GetValue<long>().Should().Be(1);
        json["state"]?["type"]?["name"]?.GetValue<string>().Should().Be("System.Object");
        json["state"]?["type"]?["assembly"]?.GetValue<string>().Should().NotBeEmpty();
        json["state"]?["value"]?.Should().NotBeNull();
    }
}
