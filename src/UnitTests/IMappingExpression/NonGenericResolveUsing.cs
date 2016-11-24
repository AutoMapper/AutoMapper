using Xunit;
using Should;
using System.Linq;

namespace AutoMapper.UnitTests
{
    public class When_using_non_generic_ResolveUsing : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
        }
        public class Destination
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(Source), typeof(Destination)).ForMember("Value", o => o.ResolveUsing((src, dest, member, ctx) => 10));
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_map_ok()
        {
            _destination.Value.ShouldEqual(10);
        }
    }
}