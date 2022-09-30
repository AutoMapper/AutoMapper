namespace AutoMapper.UnitTests;
public class ExplicitMapperCreation : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() =>new(cfg => cfg.CreateMap<Source, Dest>());
    public class Source
    {
        public int Value { get; set; }
    }
    public class Dest
    {
        public int Value { get; set; }
    }
    [Fact]
    public void Should_map()
    {
        var source = new Source {Value = 10};
        var dest = Mapper.Map<Source, Dest>(source);
        dest.Value.ShouldBe(source.Value);
    }
}