namespace AutoMapper.UnitTests.Bug;

public class MappingToAReadOnlyCollection : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int[] Values { get; set; }
        public int[] Values2 { get; set; }
    }

    public class Destination
    {
        public ReadOnlyCollection<int> Values { get; set; }
        public ReadOnlyCollection<int> Values2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        var source = new Source
                         {
                             Values = new[] {1, 2, 3, 4},
                             Values2 = new[] {5, 6},
                         };
        _destination = Mapper.Map<Source, Destination>(source);
    }

    [Fact]
    public void Should_map_the_list_of_source_items()
    {
        _destination.Values.ShouldNotBeNull();
        _destination.Values.ShouldBeOfLength(4);
        _destination.Values.ShouldContain(1);
        _destination.Values.ShouldContain(2);
        _destination.Values.ShouldContain(3);
        _destination.Values.ShouldContain(4);

        _destination.Values2.ShouldNotBeNull();
        _destination.Values2.ShouldBeOfLength(2);
        _destination.Values2.ShouldContain(5);
        _destination.Values2.ShouldContain(6);
    }
}