namespace AutoMapper.UnitTests.Bug;
public class ForAllMapsTypeConverter : AutoMapperSpecBase
{
    protected override MapperConfiguration CreateConfiguration() => new(c =>
    {
        c.CreateMap<int, int>().ConvertUsing(s => s+1);
        c.ForAllMaps((_, m) => m.ForAllMembers(_ => { }));
    });
    [Fact]
    public void Should_work() => Map<int>(42).ShouldBe(43);
}
public class ForAllMaps : AutoMapperSpecBase
{
    private Destination _destination;
    private Destination1 _destination1;
    private Destination2 _destination2;

    class Source
    {
        public int Number { get; set; }
    }
    class Destination
    {
        public int Number { get; set; }
    }

    class Source1
    {
        public int Number { get; set; }
    }
    class Destination1
    {
        public int Number { get; set; }
    }

    class Source2
    {
        public int Number { get; set; }
    }
    class Destination2
    {
        public int Number { get; set; }
    }

    public class MinusOneResolver : IValueResolver<object, object, object>
    {
        public object Resolve(object source, object dest, object destMember, ResolutionContext context)
        {
            return -1;
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
        cfg.CreateMap<Source1, Destination1>();
        cfg.CreateMap<Source2, Destination2>();
        cfg.ForAllMaps((tm, map) => map.ForMember("Number", o => o.MapFrom<MinusOneResolver>()));
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source());
        _destination1 = Mapper.Map<Source1, Destination1>(new Source1());
        _destination2 = Mapper.Map<Source2, Destination2>(new Source2());
    }

    [Fact]
    public void Should_configure_all_maps()
    {
        _destination.Number.ShouldBe(-1);
        _destination1.Number.ShouldBe(-1);
        _destination2.Number.ShouldBe(-1);
    }
}
public class ForAllMapsWithConstructors : AutoMapperSpecBase
{
    class Source
    {
    }
    class Destination
    {
        public Destination(int first, int second)
        {
            First = first;
            Second = second;
        }
        public int First { get; }
        public int Second { get; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.ForAllMaps((_, c) => c.ForCtorParam("second", o => o.MapFrom(s => 2)));
        cfg.CreateMap<Source, Destination>().ForCtorParam("first", o => o.MapFrom(s => 1));
    });
    [Fact]
    public void Should_map_ok()
    {
        var result = Map<Destination>(new Source());
        result.First.ShouldBe(1);
        result.Second.ShouldBe(2);
    }
}