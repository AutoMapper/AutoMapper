using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class ConstructorMapping : AutoMapperSpecBase
    {
        private Destination _destination;

        class Source
        {
            public int Number { get; set; }
        }
        class Destination
        {
            public Destination()
            {
            }

            public Destination(int x)
            {
                Number = x;
            }

            public int Number { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });
        }

        protected override void Because_of()
        {
            var source = new Source
            {
                Number = 23
            };
            _destination = Mapper.Map<Source, Destination>(source);
        }

        [Fact]
        public void Should_handle_not_used_constructors()
        {
            _destination.Number.ShouldEqual(23);
        }
    }
}