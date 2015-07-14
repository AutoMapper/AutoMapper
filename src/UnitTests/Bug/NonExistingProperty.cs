using System;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class NonExistingProperty : AutoMapperSpecBase
    {
        public class Source
        {
        }

        public class Destination
        {
        }

        [Fact]
        public void Should_report_missing_property()
        {
            var mapping = Mapper.CreateMap<Source, Destination>();
            new Action(() => mapping.ForMember("X", s => { })).ShouldThrow<ArgumentOutOfRangeException>();
        }
    }
}
