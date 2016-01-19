using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class InheritanceMissingMap : AutoMapperSpecBase
    {
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

        [Fact]
        public void Should_report_missing_map()
        {
            new Action(() => Mapper.Map<Destination>(new Source())).ShouldThrow<AutoMapperMappingException>();
        }
    }
}