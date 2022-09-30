using static AutoMapper.UnitTests.Bug.MapFromClosureBug;

namespace AutoMapper.UnitTests;

public class When_specifying_a_mapping_order_for_base_members : AutoMapperSpecBase
{
    Destination _destination;

    class Source
    {
        public string One { get; set; }

        public string Two { get; set; }
    }

    class SourceChild : Source
    {
    }

    class Destination
    {
        // must be defined before property "One" to fail
        public string Two { get; set; }

        private string one;

        public string One
        {
            get
            {
                return this.one;
            }
            set
            {
                this.one = value;
                this.Two = value;
            }
        }
    }

    protected override void Because_of()
    {
        _destination = Mapper.Map<Destination>(new SourceChild { One = "first", Two = "second" });
    }

    [Fact]
    public void Should_inherit_the_mapping_order()
    {
        _destination.Two.ShouldBe("first");
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .Include<SourceChild, Destination>()
            .ForMember(dest => dest.One, opt => opt.SetMappingOrder(600))
            .ForMember(dest => dest.Two, opt => opt.SetMappingOrder(-500));

        cfg.CreateMap<SourceChild, Destination>();
    });
}

public class When_specifying_a_mapping_order : AutoMapperSpecBase
{
    private Destination _result;

    public class Source
    {
        private int _startValue;

        public Source(int startValue)
        {
            _startValue = startValue;
        }

        public int GetValue1()
        {
            _startValue += 10;
            return _startValue;
        }

        public int GetValue2()
        {
            _startValue += 5;
            return _startValue;
        }
    }

    public class Destination
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>()
            .ForMember(d => d.Value1, opt => opt.SetMappingOrder(2))
            .ForMember(d => d.Value2, opt => opt.SetMappingOrder(1));
    });

    protected override void Because_of()
    {
        _result = Mapper.Map<Source, Destination>(new Source(10));
    }

    [Fact]
    public void Should_perform_the_mapping_in_the_order_specified()
    {
        _result.Value2.ShouldBe(15);
        _result.Value1.ShouldBe(25);
    }
}
public class NullMappingOrderComesFirst : AutoMapperSpecBase
{
    public class Source
    {
        private int _startValue;
        public int GetValue1()
        {
            _startValue += 5;
            return _startValue;
        }
        public int GetValue2()
        {
            _startValue += 5;
            return _startValue;
        }
    }
    public class Destination
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateMap<Source, Destination>().ForMember(d => d.Value1, opt => opt.SetMappingOrder(0)));
    [Fact]
    public void Should_perform_the_mapping_in_the_order_specified()
    {
        var _result = Mapper.Map<Source, Destination>(new Source());
        _result.Value2.ShouldBe(5);
        _result.Value1.ShouldBe(10);
    }
}