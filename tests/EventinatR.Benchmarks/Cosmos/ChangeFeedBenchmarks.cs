using System.Text.Json.Serialization;
using AutoFixture;
using BenchmarkDotNet.Attributes;
using EventinatR.Cosmos;
using EventinatR.Cosmos.Documents;

namespace EventinatR.Benchmarks.Cosmos
{
    public class ChangeFeedBenchmarks
    {
        private record Data(string String, int Int32, long Int64, bool Boolean, Guid Guid);

        private string _documents = string.Empty;

        [Params(1, 10, 100)]
        public int Count { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var fixture = new Fixture();

            fixture.Register(() => JsonData.From(fixture.Create<Data>()));

            var documents = fixture.CreateMany<EventDocument>(Count);
            var serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            _documents = JsonSerializer.Serialize(documents, serializerOptions);
        }

        [Benchmark()]
        public void ParseEvents()
        {
            var events = CosmosEventStoreChangeFeed.ParseEvents(_documents);

            foreach (var e in events)
            {
                _ = e.Data.As<Data>();
            }
        }
    }
}
