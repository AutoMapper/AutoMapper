namespace AutoMapper.UnitTests.MappingInheritance;

public class IncludeAllDerived : AutoMapperSpecBase
{
    public class A
    {
        public int Value { get; set; }
    }
    public class B : A { }
    public class C : B { }
    public class D : A { }

    public class ADto
    {
        public int Value { get; set; }
    }

    public class BDto : ADto { }
    public class CDto : BDto { }
    public class DDto : ADto { }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<A, ADto>()
            .ForMember(d => d.Value, opt => opt.MapFrom(src => 5))
            .IncludeAllDerived();

        cfg.CreateMap<B, BDto>();
        cfg.CreateMap<C, CDto>();
        cfg.CreateMap<D, DDto>();
    });

    [Fact]
    public void Should_apply_configuration_to_all_derived()
    {
        Mapper.Map<ADto>(new A {Value = 10}).Value.ShouldBe(5);
        Mapper.Map<BDto>(new B {Value = 10}).Value.ShouldBe(5);
        Mapper.Map<CDto>(new C {Value = 10}).Value.ShouldBe(5);
        Mapper.Map<DDto>(new D {Value = 10}).Value.ShouldBe(5);
    }
}