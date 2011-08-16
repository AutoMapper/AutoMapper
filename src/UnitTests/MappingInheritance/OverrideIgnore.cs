using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
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

        [Test]
        public void specifying_map_should_override_ignore()
        {
            Mapper.CreateMap<DomainBase, DtoBase>()
                .ForMember(m=>m.SomeDifferentProperty, m=>m.Ignore())
                .ForMember(m=>m.SomeDifferentProperty, m=>m.MapFrom(s=>s.SomeProperty));

            var dto = Mapper.Map<DomainBase, DtoBase>(new DomainBase {SomeProperty = "Test"});

            Assert.AreEqual("Test", dto.SomeDifferentProperty);
        }

        [Test]
        public void specifying_map_should_override_ignore_with_one_parameter()
        {
            Mapper.CreateMap<DomainBase, DtoBase>()
                .ForMember(m => m.SomeDifferentProperty, m => m.Ignore())
                .ForMember(m => m.SomeDifferentProperty, m => m.MapFrom(s => s.SomeProperty));

            var dto = Mapper.Map<DtoBase>(new DomainBase { SomeProperty = "Test" });

            Assert.AreEqual("Test", dto.SomeDifferentProperty);
        }
    }
}
