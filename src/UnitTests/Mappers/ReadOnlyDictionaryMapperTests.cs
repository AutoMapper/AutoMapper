namespace AutoMapper.UnitTests.Mappers.ReadOnlyDictionaryMapper;

public class When_mapping_to_interface_readonly_dictionary : AutoMapperSpecBase
{
    public class Source
    {
        public IReadOnlyDictionary<int, int> Values { get; set; }
    }

    public class Destination
    {
        public IReadOnlyDictionary<int, int> Values { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(config =>
    {
        config.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_map_readonly_values()
    {
        var values = new Dictionary<int, int>
        {
            {1, 1},
            {2, 2},
            {3, 3},
            {4, 4},
        };
        var source = new Source
        {
            Values = values
        };

        var dest = Mapper.Map<Destination>(source);

        dest.Values.ShouldBe(values);
    }
}
public class When_mapping_to_concrete_readonly_dictionary : AutoMapperSpecBase
{
    public class Source
    {
        public ReadOnlyDictionary<int, int> Values { get; set; }
    }

    public class Destination
    {
        public ReadOnlyDictionary<int, int> Values { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(config =>
    {
        config.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_map_readonly_values()
    {
        var values = new Dictionary<int, int>
        {
            {1, 1},
            {2, 2},
            {3, 3},
            {4, 4},
        };
        var source = new Source
        {
            Values = new ReadOnlyDictionary<int, int>(values)
        };

        var dest = Mapper.Map<Destination>(source);

        dest.Values.ShouldBe(values);
    }
}