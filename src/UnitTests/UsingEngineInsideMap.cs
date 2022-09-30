namespace AutoMapper.UnitTests;
public class UsingEngineInsideMap : AutoMapperSpecBase
{
    private Dest _dest;

    public class Source
    {
        public int Foo { get; set; }
    }

    public class Dest
    {
        public int Foo { get; set; }
        public ChildDest Child { get; set; }
    }

    public class ChildDest
    {
        public int Foo { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Dest>()
            .ForMember(dest => dest.Child,
                opt =>
                    opt.MapFrom(
                        (src, dest, destMember, context) =>
                            context.Mapper.Map(src, destMember, typeof (Source), typeof (ChildDest))));
        cfg.CreateMap<Source, ChildDest>();
    });

    protected override void Because_of()
    {
        _dest = Mapper.Map<Source, Dest>(new Source {Foo = 5});
    }

    [Fact]
    public void Should_map_child_property()
    {
        _dest.Child.ShouldNotBeNull();
        _dest.Child.Foo.ShouldBe(5);
    }
}

public class When_mapping_null_with_context_mapper : AutoMapperSpecBase
{
    class Source
    {
    }

    class Destination
    {
        public string Value { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
        cfg.CreateMap<Source, Destination>().ForMember(d=>d.Value, o=>o.MapFrom((s,d,dm, context)=>context.Mapper.Map<string>(null))));

    [Fact]
    public void Should_return_null() => Mapper.Map<Destination>(new Source()).Value.ShouldBeNull();
}