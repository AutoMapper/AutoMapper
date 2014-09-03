namespace AutoMapper.UnitTests.Projection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Bug.AssignableLists;
    using QueryableExtensions;
    using Should;
    using Xunit;

    public class ParameterizedQueriesTests_with_anonymous_object : AutoMapperSpecBase
    {
        private Dest[] _dests;
        private IQueryable<Source> _sources;

        public class Source
        {
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            int value = 0;

            Expression<Func<Source, int>> sourceMember = src => value + 5;
            Mapper.CreateMap<Source, Dest>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(sourceMember));
        }

        protected override void Because_of()
        {
            _sources = new[]
            {
                new Source()
            }.AsQueryable();

            _dests = _sources.Project().To<Dest>(new { value = 10 }).ToArray();
        }

        [Fact]
        public void Should_substitute_parameter_value()
        {
            _dests[0].Value.ShouldEqual(15);
        }

        [Fact]
        public void Should_not_cache_parameter_value()
        {
            var newDests = _sources.Project().To<Dest>(new {value = 15}).ToArray();

            newDests[0].Value.ShouldEqual(20);
        }
    }

    public class ParameterizedQueriesTests_with_dictionary_object : AutoMapperSpecBase
    {
        private Dest[] _dests;
        private IQueryable<Source> _sources;

        public class Source
        {
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override void Establish_context()
        {
            int value = 0;

            Expression<Func<Source, int>> sourceMember = src => value + 5;
            Mapper.CreateMap<Source, Dest>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(sourceMember));
        }

        protected override void Because_of()
        {
            _sources = new[]
            {
                new Source()
            }.AsQueryable();

            _dests = _sources.Project().To<Dest>(new Dictionary<string, object>{{"value", 10}}).ToArray();
        }

        [Fact]
        public void Should_substitute_parameter_value()
        {
            _dests[0].Value.ShouldEqual(15);
        }

        [Fact]
        public void Should_not_cache_parameter_value()
        {
            var newDests = _sources.Project().To<Dest>(new Dictionary<string, object> { { "value", 15 } }).ToArray();

            newDests[0].Value.ShouldEqual(20);
        }
    }
}