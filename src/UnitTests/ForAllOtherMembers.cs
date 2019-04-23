using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class ForAllOtherMembers : AutoMapperSpecBase
    {
        Destination _destination;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Destination
        {
            public int value { get; set; }
            public int value1 { get; set; }
            public int value2 { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().ForMember(d => d.value, o => o.MapFrom(s => s.Value)).ForAllOtherMembers(o => o.Ignore());
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source { Value = 12 });
        }

        [Fact]
        public void Should_map_not_ignored()
        {
            _destination.value.ShouldBe(12);
        }
    }

    public class ForAllOtherMembersWithInherintance : AutoMapperSpecBase
    {
        public interface Interface1
        {
            string Name { get; }
        }
        public interface Interface2 : Interface1
        {
            new string Name { get; set; }
        }
        public interface Interface3 : Interface1, Interface2
        {
            string Code { get; set; }
        }
        class SourceClass
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }
        class TargetClass : Interface3
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=>
        {
            cfg.CreateMap<SourceClass, Interface3>(MemberList.None)
                    .ForMember(trg => trg.Name, opt => opt.MapFrom(src => src.Name))
                    .ForAllOtherMembers(opt => opt.Ignore());
        });

        [Fact]
        public void Should_work()
        {
            var source = new SourceClass()
            {
                Name = "SourceName",
                Code = "SourceCode"
            };
            Interface3 dest = new TargetClass()
            {
                Name = "TargetName",
                Code = "TargetCode"
            };
            Mapper.Map(source, dest);
            dest.Code.ShouldBe("TargetCode");
            dest.Name.ShouldBe("SourceName");
        }
    }
}