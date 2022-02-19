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

    [Fact]
    public void FromStream_ShouldReturnEvent_WhenJsonIsEventDocument()
    {
        var json = /*lang=json,strict*/ @"{
            ""type"": ""event"",
            ""stream"": ""stream"",
            ""id"": ""id"",
            ""version"": 1,
            ""transaction"": { ""version"": 1, ""count"": 1 },
            ""timestamp"": ""2021-02-18T21:50:00.000Z"",
            ""data"": {
                ""type"": { ""name"": ""System.Object"", ""assembly"": ""System.Private.CoreLib"" },
                ""value"": { }
            }
        }";
        var stream = BinaryData.FromString(json).ToStream();

        var document = _sut.FromStream<EventDocument>(stream);

        document.Type.Should().Be(DocumentTypes.Event);
        document.Stream.Value.Should().Be("stream");
        document.Id.Value.Should().Be("id");
        document.Version.Value.Should().Be(1);
        document.Timestamp.Should().Be(new DateTimeOffset(2021, 2, 18, 21, 50, 0, TimeSpan.Zero));
        document.Data.Type.Name.Should().Be("System.Object");
        document.Data.Type.Assembly.Should().Be("System.Private.CoreLib");
        document.Data.Value.Should().NotBeNull();
    }

    [Fact]
    public void FromStream_ShouldReturnStream_WhenJsonIsStreamDocument()
    {
        var json = /*lang=json,strict*/ @"{
            ""type"": ""stream"",
            ""stream"": ""stream"",
            ""id"": ""id"",
            ""version"": 1
        }";
        var stream = BinaryData.FromString(json).ToStream();

        var document = _sut.FromStream<StreamDocument>(stream);

        document.Type.Should().Be(DocumentTypes.Stream);
        document.Stream.Value.Should().Be("stream");
        document.Id.Value.Should().Be("id");
        document.Version.Value.Should().Be(1);
    }

    [Fact]
    public void FromStream_ShouldReturnSnapshot_WhenJsonIsSnapshotDocument()
    {
        var json = /*lang=json,strict*/ @"{
            ""type"": ""snapshot"",
            ""stream"": ""stream"",
            ""id"": ""id"",
            ""version"": 1,
            ""state"": {
                ""type"": { ""name"": ""System.Object"", ""assembly"": ""System.Private.CoreLib"" },
                ""value"": { }
            }
        }";
        var stream = BinaryData.FromString(json).ToStream();

        var document = _sut.FromStream<SnapshotDocument>(stream);

        document.Type.Should().Be(DocumentTypes.Snapshot);
        document.Stream.Value.Should().Be("stream");
        document.Id.Value.Should().Be("id");
        document.Version.Value.Should().Be(1);
        document.State.Type.Name.Should().Be("System.Object");
        document.State.Type.Assembly.Should().Be("System.Private.CoreLib");
        document.State.Value.Should().NotBeNull();
    }
}
