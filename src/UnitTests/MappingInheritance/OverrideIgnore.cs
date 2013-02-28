using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class OverrideIgnore
    {
        public class DomainBase
        {
            public string SomeProperty { get; set; }
        }

        public class DtoBase
        {
            public string SomeDifferentProperty { get; set; }
        }

        [Fact]
        public void specifying_map_should_override_ignore()
        {
            Mapper.CreateMap<DomainBase, DtoBase>()
                .ForMember(m=>m.SomeDifferentProperty, m=>m.Ignore())
                .ForMember(m=>m.SomeDifferentProperty, m=>m.MapFrom(s=>s.SomeProperty));

            var dto = Mapper.Map<DomainBase, DtoBase>(new DomainBase {SomeProperty = "Test"});

            "Test".ShouldEqual(dto.SomeDifferentProperty);
        }

        [Fact]
        public void specifying_map_should_override_ignore_with_one_parameter()
        {
            Mapper.CreateMap<DomainBase, DtoBase>()
                .ForMember(m => m.SomeDifferentProperty, m => m.Ignore())
                .ForMember(m => m.SomeDifferentProperty, m => m.MapFrom(s => s.SomeProperty));

            var dto = Mapper.Map<DtoBase>(new DomainBase { SomeProperty = "Test" });

            "Test".ShouldEqual(dto.SomeDifferentProperty);
        }
    }
}
