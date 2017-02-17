using Xunit;
using Should;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AutoMapper.UnitTests.Bug
{
    public class Self_referencing_existing_destination : AutoMapperSpecBase
    {
        public class BaseType
        {
            public BaseType()
            {
                SelfReference = this;
            }
            public BaseType SelfReference { get; set; }
        }

        public class BaseTypeDto
        {
            public BaseTypeDto()
            {
                SelfReference = this;
            }
            public BaseTypeDto SelfReference { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=> cfg.CreateMap<BaseType, BaseTypeDto>().PreserveReferences());

        protected override void Because_of()
        {
            var baseType = new BaseType();
            var baseTypeDto = new BaseTypeDto();

            Mapper.Map(baseType, baseTypeDto);
        }
    }

    public class PreserveReferencesSameDestination : AutoMapperSpecBase
    {
        public class DtoOne
        {
            public DtoTwo Two { get; set; }
        }

        public class DtoTwo
        {
            public virtual ICollection<DtoOne> Ones { get; set; }
        }

        public class DtoThree
        {
            public int Id { get; set; }
        }

        public class EntityOne
        {
            public int Id { get; set; }
            public EntityTwo Two { get; set; }
        }

        public class EntityTwo
        {
            public virtual ICollection<EntityOne> Ones { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<EntityTwo, DtoTwo>().PreserveReferences();
            cfg.CreateMap<EntityOne, DtoOne>().PreserveReferences();
            cfg.CreateMap<EntityOne, DtoThree>().PreserveReferences();
        });

        [Fact]
        public void Should_use_the_right_map()
        {
            var source =
                    new EntityOne {
                        Two = new EntityTwo {
                            Ones = new List<EntityOne> {
                                new EntityOne {
                                    Two = new EntityTwo { Ones = new List<EntityOne>() }
                                }
                            }
                        }
                    };
            Mapper.Map<EntityOne, DtoOne>(source).ShouldBeType<DtoOne>();
            Mapper.Map<EntityOne, DtoThree>(source).ShouldBeType<DtoThree>();
        }
    }
}