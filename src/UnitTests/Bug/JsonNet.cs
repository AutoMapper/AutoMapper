namespace AutoMapper.UnitTests.Bug;

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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(d => d.Json, o => o.MapFrom(s => new JObject(s.JsonString)));
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
        json.Count.ShouldBe(3);
        json["1"].ShouldBe("one");
        json["2"].ShouldBe("two");
        json["3"].ShouldBe("three");
    }
}

public class JObjectField : AutoMapperSpecBase
{
    class JContainer : IEnumerable<DBNull>
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<DBNull>)this).GetEnumerator();
        }

        IEnumerator<DBNull> IEnumerable<DBNull>.GetEnumerator()
        {
            return (IEnumerator<DBNull>)new[] { DBNull.Value }.GetEnumerator();
        }
    }

    class JObject : JContainer, IDictionary<string, string>
    {
        public JObject()
        {
        }

        public string this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<string> Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Add(StringKeyValuePair item)
        {
            throw new NotImplementedException();
        }

        public void Add(string key, string value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(StringKeyValuePair item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(StringKeyValuePair[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<StringKeyValuePair> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(StringKeyValuePair item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out string value)
        {
            throw new NotImplementedException();
        }
    }

    class Source
    {
        public JObject Json { get; set; }
    }

    class Destination
    {
        public JObject Json { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });
    [Fact]
    public void Validate() => AssertConfigurationIsValid();
}