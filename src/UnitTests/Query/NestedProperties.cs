using System.Linq;
using AutoMapper.QueryableExtensions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Query
{

    public class NestedProperties : AutoMapperSpecBase
    {
        private IQueryable<Dest> _dests;

        class Source
        {
            public SourceChild Child1 { get; set; }
            public SourceChild Child2 { get; set; }
        }

        class SourceChild
        {


            public int Value { get; set; }
        }

        class Dest
        {
            public DestChild Child1 { get; set; }
            public DestChild Child2 { get; set; }
        }

        class DestChild
        {
            public DestChild(int value)
            {
                Value = value;
            }
            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<SourceChild, DestChild>();
            Mapper.CreateMap<Source, Dest>()
                .ForMember(m => m.Child1, opt => opt.ExplicitExpansion())
                .ForMember(m => m.Child2, opt => opt.ExplicitExpansion());
        }

        protected override void Because_of()
        {
            var destList = new[]
            {
                new Dest
                {
                    Child1 = new DestChild(10),
                    Child2 = new DestChild(1000),
                }, new Dest
                {
                    Child1 = new DestChild(200),
                    Child2 = new DestChild(500),
                }
            };

            _dests = new Source[0].AsQueryable()
                .Where(s => s.Child1.Value > 100 || s.Child2.Value < 1000)
                .Map<Source, Dest>(destList.AsQueryable());
        }

        [Fact]
        public void Should_filtrate_by_nested_properties()
        {
            _dests.Count().ShouldEqual(1);
            _dests.First().Child1.Value.ShouldEqual(200);
            _dests.First().Child2.Value.ShouldEqual(500);
        }
    }

}
