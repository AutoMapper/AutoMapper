namespace AutoMapper.UnitTests.Bug;

public class ForAllMembersAndResolveUsing : AutoMapperSpecBase
{
    private Destination _destination;

    class Source
    {
        public int Number { get; set; }
    }
    class Destination
    {
        public int Number { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().ForAllMembers(opt => opt.MapFrom(s=>12));
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
    public void Should_work_together()
    {
        _destination.Number.ShouldBe(12);
    }
}