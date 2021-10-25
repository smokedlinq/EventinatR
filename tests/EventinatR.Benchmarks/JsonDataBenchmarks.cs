using AutoFixture;
using BenchmarkDotNet.Attributes;

namespace EventinatR.Benchmarks
{
    [MemoryDiagnoser]
    public class JsonDataBenchmarks
    {
        private record BaseData(string String, int Int32, long Int64, bool Boolean, Guid Guid);
        private record ActualData(string String, int Int32, long Int64, bool Boolean, Guid Guid)
            : BaseData(String, Int32, Int64, Boolean, Guid);

        private JsonData[] _data = Array.Empty<JsonData>();

        [Params(1, 10, 100)]
        public int Count { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var fixture = new Fixture();
            _data = fixture.CreateMany<ActualData>(Count).Select(x => JsonData.From(x)).ToArray();
        }

        [Benchmark()]
        public void JsonDataAsBinaryData()
        {
            foreach (var data in _data)
            {
                _ = data.As<BinaryData>();
            }
        }

        [Benchmark()]
        public void JsonDataAsString()
        {
            foreach (var data in _data)
            {
                _ = data.As<string>();
            }
        }

        [Benchmark()]
        public void JsonDataAsArray()
        {
            foreach (var data in _data)
            {
                _ = data.As<byte[]>();
            }
        }

        [Benchmark()]
        public void JsonDataAsReadOnlyMemory()
        {
            foreach (var data in _data)
            {
                _ = data.As<ReadOnlyMemory<byte>>();
            }
        }

        [Benchmark()]
        public void JsonDataAsStream()
        {
            foreach (var data in _data)
            {
                _ = data.As<Stream>();
            }
        }

        [Benchmark()]
        public void JsonDataAsBase()
        {
            foreach (var data in _data)
            {
                _ = data.As<BaseData>();
            }
        }

        [Benchmark()]
        public void JsonDataAsJsonDocument()
        {
            foreach (var data in _data)
            {
                _ = data.As<JsonDocument>();
            }
        }

        [Benchmark()]
        public void JsonDataAsElement()
        {
            foreach (var data in _data)
            {
                _ = data.As<JsonElement>();
            }
        }

        [Benchmark(Baseline = true)]
        public void JsonDataAsActual()
        {
            foreach (var data in _data)
            {
                _ = data.As<ActualData>();
            }
        }
    }
}
