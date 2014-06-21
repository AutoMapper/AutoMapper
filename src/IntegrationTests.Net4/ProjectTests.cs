using System.Linq;
using AutoMapper.QueryableExtensions;
using Xunit;
using Should;

namespace AutoMapper.IntegrationTests.Net4
{
    namespace ProjectTests
    {
        public class Source
        {
            public int? Value { get; set; }
        }

        public class Dest
        {
            public int? Value { get; set; }
        }

        public class UnitTest
        {
            public UnitTest()
            {
                Mapper.Reset();
            }

            [Fact]
            public void NullSubstitution()
            {
                Mapper.CreateMap<Source, Dest>()
                    .ForMember(dest => dest.Value, opt => opt.NullSubstitute(3));

                var sources = new[] {new Source()}.AsQueryable();
                var dests = sources.Project().To<Dest>().ToList();
                dests[0].Value.ShouldEqual(3);
            }
        }

    }
}