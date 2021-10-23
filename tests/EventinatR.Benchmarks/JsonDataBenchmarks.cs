using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using BenchmarkDotNet.Attributes;

namespace EventinatR.Benchmarks
{
    public class JsonDataBenchmarks
    {
        private record BaseData(string String, int Int32, long Int64, bool Boolean, Guid Guid);
        private record InheritedData(string String, int Int32, long Int64, bool Boolean, Guid Guid)
            : BaseData(String, Int32, Int64, Boolean, Guid);

        private JsonData[] _data = Array.Empty<JsonData>();

        [Params(1, 10, 100, 1000)]
        public int Count { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var fixture = new Fixture();
            _data = fixture.CreateMany<InheritedData>(Count).Select(x => JsonData.From(x)).ToArray();
        }

        [Benchmark()]
        public void JsonDataAsBase()
        {
            foreach(var data in _data)
            {
                _ = data.As<BaseData>();
            }
        }

        [Benchmark()]
        public void JsonDataAsInherited()
        {
            foreach (var data in _data)
            {
                _ = data.As<InheritedData>();
            }
        }
    }
}
