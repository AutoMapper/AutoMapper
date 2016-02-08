using Should;
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
            _destination.value.ShouldEqual(12);
        }
    }
}
