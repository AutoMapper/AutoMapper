namespace AutoMapper.UnitTests.Projection
{
    using System.Linq;
    using QueryableExtensions;
    using Should;
    using Xunit;

    public class ToStringTests : AutoMapperSpecBase
    {
        private Dest[] _dests;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public string Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>();
        }

        protected override void Because_of()
        {
            var sources = new[]
            {
                new Source
                {
                    Value = 5
                }
            }.AsQueryable();

            _dests = sources.ProjectTo<Dest>().ToArray();
        }

        [Fact]
        public void Should_convert_to_string()
        {
            _dests[0].Value.ShouldEqual("5");
        }
    }
}