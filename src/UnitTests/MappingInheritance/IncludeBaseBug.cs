namespace AutoMapper.UnitTests.MappingInheritance;
public class Include : AutoMapperSpecBase
{
    public class From
    {
        public int Value { get; set; }
        public int ChildValue { get; set; }
    }


    public class Concrete
    {
        public int ConcreteValue { get; set; }
    }

    public abstract class AbstractChild : Concrete
    {
        public int AbstractValue { get; set; }
    }

    public class Derivation : AbstractChild
    {
        public int DerivedValue { get; set; }
    }


    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<From, Concrete>()
            .ForMember(d => d.ConcreteValue, o => o.MapFrom(s => s == null ? default(int) : s.ChildValue))
            .Include<From, AbstractChild>();
        cfg.CreateMap<From, AbstractChild>()
            .ForMember(d => d.AbstractValue, o => o.Ignore())
            .Include<From, Derivation>();
        cfg.CreateMap<From, Derivation>()
            .ForMember(d => d.DerivedValue, o => o.Ignore());
        cfg.AllowNullDestinationValues = false;
    });
    [Fact]
    public void Should_map_ok()
    {
        var dest = Mapper.Map(null, typeof(From), typeof(Concrete));
        dest.ShouldBeOfType<Concrete>();
        ReferenceEquals(dest.GetType(), typeof(Concrete)).ShouldBeTrue();
    }
}
public class BaseNotMatching : AutoMapperSpecBase
{
    public class From
    {
        public int Value { get; set; }
        public int ChildValue { get; set; }
    }
    public class FromDerived : From
    {
        public int AbstractValue { get; set; }
    }
    public class Concrete
    {
        public int ConcreteValue { get; set; }
    }
    public abstract class AbstractChild : Concrete
    {
        public int AbstractValue { get; set; }
    }
    public class Derivation : AbstractChild
    {
        public int DerivedValue { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<From, Concrete>()
            .ForMember(d => d.ConcreteValue, o => o.MapFrom(s => s == null ? default(int) : s.ChildValue))
            .Include<From, AbstractChild>();
        cfg.CreateMap<From, AbstractChild>(MemberList.None)
            .Include<FromDerived, Derivation>();
        cfg.CreateMap<FromDerived, Derivation>()
            .ForMember(d => d.DerivedValue, o => o.Ignore());
    });
    [Fact]
    public void Derived_matches()
    {
        var dest = Mapper.Map<Derivation>(new FromDerived { AbstractValue = 42 });
        dest.AbstractValue.ShouldBe(42);
    }
}
public class BaseMatchingDifferentType : AutoMapperSpecBase
{
    public class From
    {
        public int Value { get; set; }
        public int ChildValue { get; set; }
        public DateTime AbstractValue { get; set; }
    }
    public class FromDerived : From
    {
        public new int AbstractValue { get; set; }
    }
    public class Concrete
    {
        public int ConcreteValue { get; set; }
    }
    public abstract class AbstractChild : Concrete
    {
        public int AbstractValue { get; set; }
    }
    public class Derivation : AbstractChild
    {
        public int DerivedValue { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<From, Concrete>()
            .ForMember(d => d.ConcreteValue, o => o.MapFrom(s => s == null ? default(int) : s.ChildValue))
            .Include<From, AbstractChild>();
        cfg.CreateMap<From, AbstractChild>(MemberList.None)
            .Include<FromDerived, Derivation>();
        cfg.CreateMap<FromDerived, Derivation>()
            .ForMember(d => d.DerivedValue, o => o.Ignore());
    });
    [Fact]
    public void Derived_matches()
    {
        var dest = Mapper.Map<Derivation>(new FromDerived { AbstractValue = 42 });
        dest.AbstractValue.ShouldBe(42);
    }
}
public class IgnoreBaseMatching : AutoMapperSpecBase
{
    public class From
    {
        public int Value { get; set; }
        public int ChildValue { get; set; }
        public int AbstractValue { get; set; }
    }
    public class FromDerived : From
    {
    }
    public class Concrete
    {
        public int ConcreteValue { get; set; }
    }
    public abstract class AbstractChild : Concrete
    {
        public int AbstractValue { get; set; }
    }
    public class Derivation : AbstractChild
    {
        public int DerivedValue { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<From, Concrete>()
            .ForMember(d => d.ConcreteValue, o => o.MapFrom(s => s == null ? default(int) : s.ChildValue))
            .Include<From, AbstractChild>();
        cfg.CreateMap<From, AbstractChild>(MemberList.None)
            .Include<FromDerived, Derivation>();
        cfg.CreateMap<FromDerived, Derivation>()
            .ForMember(d => d.AbstractValue, o => o.Ignore())
            .ForMember(d => d.DerivedValue, o => o.Ignore());
    });
    [Fact]
    public void Derived_ignores()
    {
        var dest = Mapper.Map<Derivation>(new FromDerived { AbstractValue = 42 });
        dest.AbstractValue.ShouldBe(0);
    }
}