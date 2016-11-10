using Xunit;
using Should;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AutoMapper.UnitTests.Bug
{
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