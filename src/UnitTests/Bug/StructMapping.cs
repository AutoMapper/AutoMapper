namespace AutoMapper.UnitTests.Bug;

public class StructMapping : AutoMapperSpecBase
{
    private Destination _destination;

    struct Source
    {
        public int Number { get; set; }
    }
    class Destination
    {
        public int Number { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        var source = new Source
        {
            Number = 23
        };
        _destination = Mapper.Map<Source, Destination>(source);
    }

    [Fact]
    public void Should_work()
    {
        _destination.Number.ShouldBe(23);
    }
}
public class DestinationStructMapping : AutoMapperSpecBase
{
    struct Source
    {
        public int Number { get; set; }
    }
    struct Destination
    {
        public int Number { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Destination>());
    [Fact]
    public void Should_work() => Mapper.Map<Source, Destination>(new Source { Number = 23 }).Number.ShouldBe(23);
    [Fact]
    public void Should_work_with_object() => ((Destination)Mapper.Map(new Source { Number = 23 }, typeof(Source), typeof(Destination))).Number.ShouldBe(23);
}