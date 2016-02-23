using Xunit;
using Should;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace AutoMapper.UnitTests.Bug
{
    using StringKeyValuePair = KeyValuePair<string, string>;

    public class JsonNetDictionary : AutoMapperSpecBase
    {
        private Destination _destination;

        class JObject : Dictionary<string, string>, IEnumerable, IEnumerable<KeyValuePair<string, string>>
        {
            public JObject(string json) : base(
                (from pair in json.Split('&')
                let items = pair.Split(',')
                select new StringKeyValuePair(items[0], items[1]))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
            {
            }
        }

        class Source
        {
            public string JsonString { get; set; }
        }
        class Destination
        {
            public dynamic Json { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ForMember(d => d.Json, o => o.ResolveUsing(s => new JObject(s.JsonString)));
        });

        protected override void Because_of()
        {
            var source = new Source
            {
                JsonString = "1,one&2,two&3,three"
            };
            _destination = Mapper.Map<Source, Destination>(source);
        }

        [Fact]
        public void Should_map_dictionary_with_non_KeyValuePair_enumerable()
        {
            var json = (JObject)_destination.Json;
            json.Count.ShouldEqual(3);
            json["1"].ShouldEqual("one");
            json["2"].ShouldEqual("two");
            json["3"].ShouldEqual("three");
        }
    }
}