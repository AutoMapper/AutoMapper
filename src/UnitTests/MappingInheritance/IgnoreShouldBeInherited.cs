using System;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class IgnoreShouldBeInheritedRegardlessOfMapOrder : AutoMapperSpecBase
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
            cfg.CreateMap<SpecificDomain, Dto>();
            cfg.CreateMap<BaseDomain, Dto>()
                .ForMember(d => d.SpecificProperty, m => m.Ignore())
                .Include<SpecificDomain, Dto>();
        });

        [Fact]
        public void Should_map_ok()
        {
            var dto = Mapper.Map<Dto>(new SpecificDomain { SpecificProperty = "Test" });
            dto.SpecificProperty.ShouldBeNull();
        }
    }

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

    public class IgnoreShouldBeInheritedWithOpenGenerics : AutoMapperSpecBase
    {
        public abstract class BaseUserDto<TIdType>
        {
            public TIdType Id { get; set; }
            public string Name { get; set; }
        }

        public class ConcreteUserDto : BaseUserDto<string>
        {
        }

        public abstract class BaseUserEntity<TIdType>
        {
            public TIdType Id { get; set; }
            public string Name { get; set; }
        }

        public class ConcreteUserEntity : BaseUserEntity<string>
        {
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(BaseUserDto<>), typeof(BaseUserEntity<>)).ForMember("Id", opt => opt.Ignore());
            cfg.CreateMap(typeof(ConcreteUserDto), typeof(ConcreteUserEntity)).IncludeBase(typeof(BaseUserDto<string>), typeof(BaseUserEntity<string>));
        });

        [Fact]
        public void Should_map_ok()
        {
            var user = new ConcreteUserDto
            {
                Id = "my-id",
                Name = "my-User"
            };
            var userEntity = Mapper.Map<ConcreteUserEntity>(user);
            userEntity.Id.ShouldBeNull();
            userEntity.Name.ShouldBe("my-User");
        }
    }
}
