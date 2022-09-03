namespace AutoMapper.UnitTests.Bug;

public class ExistingArrays : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>();
        cfg.CreateMap<Source, DestWithIEnumerableInitializer>();
    });

    [Fact]
    public void should_map_array_inside_object()
    {
        var source = new Source { Values = new[] { "1", "2" } };
        var dest = Mapper.Map<Dest>(source);
    }


    [Fact]
    public void should_map_over_enumerable_empty()
    {
        var source = new Source { Values = new[] { "1", "2" } };
        var dest = Mapper.Map<DestWithIEnumerableInitializer>(source);
    }

    public class Source
    {
        public Source()
        {
            Values = new string[0];
        }

        public string[] Values { get; set; }
    }

    public class Dest
    {
        public Dest()
        {
            // remove this line will get it fixed. 
            Values = new string[0];
        }

        public string[] Values { get; set; }
    }

    public class DestWithIEnumerableInitializer
    {
        public DestWithIEnumerableInitializer()
        {
            // remove this line will get it fixed. 
            Values = Enumerable.Empty<string>();
        }

        public IEnumerable<string> Values { get; set; }
    }
}