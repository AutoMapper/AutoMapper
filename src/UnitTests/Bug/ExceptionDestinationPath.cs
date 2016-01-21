using System;
using System.Linq;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class ExceptionDestinationPath : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Destination
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().ForMember(d => d.Value, o => o.ResolveUsing((IValueResolver) null));
        });

        [Fact]
        public void Should_report_destination_path()
        {
            new Action(() => Mapper.Map<Destination>(new Source())).ShouldThrow<AutoMapperMappingException>(e => e.Message.Split().ShouldContain("Destination.Value"));
        }
    }
}