namespace AutoMapper.UnitTests.Bug;

public class TargetISet : AutoMapperSpecBase
{
    Destination _destination;
    string[] _items = new[] { "one", "two", "three" };

    public class Source
    {
        public IEnumerable<string> Items { get; set; }
    }

    class Destination
    {
        public ISet<string> Items { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new Source { Items = _items });
    }

    [Fact]
    public void Should_map_IEnumerable_to_ISet()
    {
        _destination.Items.SetEquals(_items).ShouldBeTrue();
    }

    [Fact]
    public void Should_map_null_to_empty()
    {
        Mapper.Map<Destination>(new Source()).ShouldNotBeNull();
    }
}