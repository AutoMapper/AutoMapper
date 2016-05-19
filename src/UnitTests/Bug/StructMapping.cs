using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class StructMapping : AutoMapperSpecBase
    {
        private Destination _destination;

        struct Source
        {
            public int Number { get; set; }
        }
        class Destination
        {
            public int Number { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            var source = new Source
            {
                Number = 23
            };
            _destination = Mapper.Map<Source, Destination>(source);
        }

        [Fact]
        public void Should_work()
        {
            _destination.Number.ShouldEqual(23);
        }
    }
}