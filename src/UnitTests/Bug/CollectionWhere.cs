namespace AutoMapper.UnitTests.Bug;

public class CollectionWhere : AutoMapperSpecBase
{
    private Destination _destination;
    private List<int> _sourceList = new List<int>() { 1, 2, 3 };

    class Source
    {
        public int Id { get; set; }

        public IEnumerable<int> ListProperty { get; set; }
    }

    class Destination
    {
        public int Id { get; set; }

        public IEnumerable<int> ListProperty { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        var source = new Source()
        {
            Id = 1,
            ListProperty = _sourceList,
        };
        _destination = new Destination()
        {
            Id = 2,
            ListProperty = new List<int>() { 4, 5, 6 }.Where(a=>true).ToArray()
        };
        _destination = Mapper.Map<Source, Destination>(source, _destination);
    }

    [Fact]
    public void Should_map_collections_with_where()
    {
        Enumerable.SequenceEqual(_destination.ListProperty, _sourceList).ShouldBeTrue();
    }
}