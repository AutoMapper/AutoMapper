using System.Linq;
using AutoMapper.QueryableExtensions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Query.Visitors
{

    public class MemberAccess : AutoMapperSpecBase
    {
        readonly Source[] _source = new[]
                    {
                        new Source {SrcValue = 5},
                        new Source {SrcValue = 4},
                        new Source {SrcValue = 7}
                    };

        public class Source
        {
            public string SourceStr { get; set; }
            public int SrcValue { get; set; }
        }

        public class Destination
        {
            public string DestStr { get; set; }
            public int DestValue { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Destination, Source>()
                       .ForMember(s => s.SrcValue, opt => opt.MapFrom(d => d.DestValue))
                       .ForMember(s => s.SourceStr, opt => opt.MapFrom(d => d.DestStr))
                       .ReverseMap()
                       .ForMember(d => d.DestValue, opt => opt.MapFrom(s => s.SrcValue))
                       .ForMember(d => d.DestStr, opt => opt.MapFrom(s => s.SourceStr));
        }

        protected override void Because_of()
        {
        }

        [Fact]
        public void Should_skip_static_properties_and_not_fall()
        {
            var source = new Destination[0].AsQueryable()
                .Where(s => s.DestStr == string.Empty)
                .Map(_source.AsQueryable()).ToList();
        }
    }

}
