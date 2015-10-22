using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class DynamicMapAndInheritance : AutoMapperSpecBase
    {
        Destination _destination;

        class SourceBase { }
        class DestinationBase { }
        class Source : SourceBase { }
        class Destination : DestinationBase { }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                Mapper.CreateMap<SourceBase, DestinationBase>();
            });
        }

        protected override void Because_of()
        {
            _destination = Mapper.DynamicMap<Destination>(new Source());
        }

        [Fact]
        public void Should_choose_the_most_derived_map()
        {
            _destination.ShouldBeType<Destination>();
        }
    }
}