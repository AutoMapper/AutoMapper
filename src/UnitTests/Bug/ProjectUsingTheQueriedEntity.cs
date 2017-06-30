using Xunit;
using Shouldly;
using System;
using System.Linq;
using AutoMapper.QueryableExtensions;

namespace AutoMapper.UnitTests.Bug
{
    public class ProjectUsingTheQueriedEntity : AutoMapperSpecBase
    {
        private Destination _destination;

        class Source
        {
            public int Number { get; set; }
        }
        class Destination
        {
            public int Number { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().ProjectUsing(s => new Destination {Number = 23 + s.Number});
        });

        protected override void Because_of()
        {
            _destination = new[] { new Source() }.AsQueryable().ProjectTo<Destination>(Configuration).First();
        }

        [Fact]
        public void Should_handle_projectusing_with_the_queried_entity()
        {
            _destination.Number.ShouldBe(23);
        }
    }
}