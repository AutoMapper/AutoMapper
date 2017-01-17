using System;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class IgnoreShouldBeInherited : AutoMapperSpecBase
    {
        public class BaseDomain
        {            
        }

        public class SpecificDomain : BaseDomain
        {
            public string SpecificProperty { get; set; }            
        }

        public class Dto
        {
            public string SpecificProperty { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BaseDomain, Dto>()
                .ForMember(d => d.SpecificProperty, m => m.Ignore())
                .Include<SpecificDomain, Dto>();
            cfg.CreateMap<SpecificDomain, Dto>();
        });

        [Fact]
        public void Should_map_ok()
        {
            var dto = Mapper.Map<Dto>(new SpecificDomain { SpecificProperty = "Test" });
            dto.SpecificProperty.ShouldBeNull();
        }
    }
}
