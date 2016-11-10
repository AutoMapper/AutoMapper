namespace AutoMapper.UnitTests.MappingInheritance
{
    using Should;
    using Xunit;

    public class InheritanceWithoutIncludeShouldWork : AutoMapperSpecBase
    {
        public class FooBase { }
        public class Foo : FooBase { }
        public class FooDto { public int Value { get; set; } }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<FooBase, FooDto>().ForMember(d => d.Value, opt => opt.UseValue(10));
            cfg.CreateMap<Foo, FooDto>().ForMember(d => d.Value, opt => opt.UseValue(5));
        });

        [Fact]
        public void Should_map_derived()
        {
            Map(new Foo()).Value.ShouldEqual(5);
        }

        [Fact]
        public void Should_map_base()
        {
            Map(new FooBase()).Value.ShouldEqual(10);
        }

        private FooDto Map(FooBase foo)
        {
            return Mapper.Map<FooBase, FooDto>(foo);
        }
    }
}