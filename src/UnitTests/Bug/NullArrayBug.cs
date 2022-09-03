namespace AutoMapper.UnitTests.Bug;
public class NullArrayBug : AutoMapperSpecBase
{
    private static Source _source;
    private Destination _destination;

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.AllowNullCollections = false;
        cfg.CreateMap<Source, Destination>();

        _source = new Source {Name = null, Data = null};
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(_source);
    }

    [Fact]
    public void Should_map_name_to_null()
    {
        _destination.Name.ShouldBeNull();
    }

    [Fact]
    public void Should_map_null_array_to_empty()
    {
        _destination.Data.ShouldNotBeNull();
        _destination.Data.ShouldBeEmpty();
    }

    public class Source
    {
        public string Name { get; set; }
        public string[] Data { get; set; }
    }

    public class Destination
    {
        public string Name { get; set; }
        public string[] Data { get; set; }
    }
}