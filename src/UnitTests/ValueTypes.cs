namespace AutoMapper.UnitTests.ValueTypes;

public class When_value_types_are_the_source_of_map_cycles : AutoMapperSpecBase
{
    public struct Source
    {
        public InnerSource Value { get; set; }
    }

    public class InnerSource
    {
        public Source Parent { get; set; }
    }

    public struct Destination
    {
        public InnerDestination Value { get; set; }
    }

    public class InnerDestination
    {
        public Destination Parent { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg=>
    {
        cfg.CreateMap<Source, Destination>().MaxDepth(2);
        cfg.CreateMap<InnerSource, InnerDestination>();
    });

    [Fact]
    public void Should_work()
    {
        var innerSource = new InnerSource();
        var source = new Source { Value = innerSource };
        innerSource.Parent = source;
        Mapper.Map<Destination>(source);
    }
}

public class When_value_types_are_the_source_of_map_cycles_with_PreserveReferences : AutoMapperSpecBase
{
    public struct Source
    {
        public InnerSource Value { get; set; }
    }

    public class InnerSource
    {
        public Source Parent { get; set; }
        public InnerSource Inner { get; set; }
    }

    public struct Destination
    {
        public InnerDestination Value { get; set; }
    }

    public class InnerDestination
    {
        public Destination Parent { get; set; }
        public InnerDestination Inner { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>().MaxDepth(2);
        cfg.CreateMap<InnerSource, InnerDestination>();
    });

    [Fact]
    public void Should_work()
    {
        var innerSource = new InnerSource();
        var source = new Source { Value = innerSource };
        innerSource.Parent = source;
        innerSource.Inner = innerSource;
        var destinationValue = Mapper.Map<Destination>(source).Value;
        destinationValue.Inner.ShouldBe(destinationValue);
        FindTypeMapFor<InnerSource, InnerDestination>().MemberMaps.Single(m => m.DestinationName == nameof(InnerDestination.Inner)).Inline.ShouldBeFalse();
    }
}

public class When_destination_type_is_a_value_type : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public int Value1 { get; set; }
        public string Value2 { get; set; }
    }

    public struct Destination
    {
        public int Value1 { get; set; }
        public string Value2;
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();

    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source {Value1 = 4, Value2 = "hello"});
    }

    [Fact]
    public void Should_map_property_value()
    {
        _destination.Value1.ShouldBe(4);
    }

    [Fact]
    public void Should_map_field_value()
    {
        _destination.Value2.ShouldBe("hello");
    }
}

public class When_source_struct_config_has_custom_mappings : AutoMapperSpecBase
{
    public struct matrixDigiInStruct1
    {
        public ushort CNCinfo;
        public ushort Reg1;
        public ushort Reg2;
    }
    public class DigiIn1
    {
        public ushort CncInfo { get; set; }
        public ushort Reg1 { get; set; }
        public ushort Reg2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(
        cfg => cfg.CreateMap<matrixDigiInStruct1, DigiIn1>()
            .ForMember(d => d.CncInfo, x => x.MapFrom(s => s.CNCinfo)));

    [Fact]
    public void Should_map_correctly()
    {
        var source = new matrixDigiInStruct1
        {
            CNCinfo = 5,
            Reg1 = 6,
            Reg2 = 7
        };
        var dest = Mapper.Map<matrixDigiInStruct1, DigiIn1>(source);

        dest.CncInfo.ShouldBe(source.CNCinfo);
        dest.Reg1.ShouldBe(source.Reg1);
        dest.Reg2.ShouldBe(source.Reg2);
    }
}


public class When_destination_type_is_a_nullable_value_type : AutoMapperSpecBase
{
    private Destination _destination;

    public class Source
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
    }

    public struct Destination
    {
        public int Value1 { get; set; }
        public int? Value2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<string, int>().ConvertUsing((string s) => System.Convert.ToInt32(s));
        cfg.CreateMap<string, int?>().ConvertUsing((string s) => (int?) System.Convert.ToInt32(s));
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source {Value1 = "10", Value2 = "20"});
    }

    [Fact]
    public void Should_use_map_registered_for_underlying_type()
    {
        _destination.Value2.ShouldBe(20);
    }

    [Fact]
    public void Should_still_map_value_type()
    {
        _destination.Value1.ShouldBe(10);
    }
}
public class ValueTypeDestinationPreserveReferences : AutoMapperSpecBase
{
    record Source(List<Source> List);
    record struct Destination(List<Destination> List);
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Destination>());
    [Fact]
    public void ShouldWork() => Map<Destination>(new Source(new() { new Source(null) })).List.Single().List.ShouldBeEmpty();
}