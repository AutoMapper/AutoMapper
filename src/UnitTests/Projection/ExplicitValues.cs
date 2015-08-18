namespace AutoMapper.UnitTests.Projection
{
    using System.Collections.Generic;
    using System.Linq;
    using QueryableExtensions;
    using Should;
    using Xunit;

    public class ExplicitValues : AutoMapperSpecBase
    {
        private List<Dest> _dests;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<Source, Dest>()
                .ForMember(dest => dest.Value, opt => opt.UseValue(5)));
        }

        protected override void Because_of()
        {
            var source = new[] { new Source { Value = 10 } }.AsQueryable();

            _dests = source.ProjectTo<Dest>().ToList();
        }

        [Fact]
        public void Should_substitute_value()
        {
            _dests[0].Value.ShouldEqual(5);
        }
    }

    public class ExplicitValues_ForReset : AutoMapperSpecBase
    {
        private List<Dest> _dests;

        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Dest>()
                .ForMember(dest => dest.Value, opt => opt.UseValue(5));

            new[] { new Source { Value = 10 } }.AsQueryable().ProjectTo<Dest>().ToList();

            Mapper.Reset();

            Mapper.CreateMap<Source, Dest>()
                    .ForMember(dest => dest.Value, opt => opt.UseValue(10));
        }

        protected override void Because_of()
        {
            var source = new[] { new Source { Value = 10 } }.AsQueryable();

            _dests = source.ProjectTo<Dest>().ToList();
        }

        [Fact]
        public void Should_substitute_value()
        {
            _dests[0].Value.ShouldEqual(10);
        }
    }
}