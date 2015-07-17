using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class PropertyNamedType
    {
        class Source
        {
            public int Number { get; set; }
        }
        class Destination
        {
            public int Type { get; set; }
        }

        [Fact]
        public void Should_detect_unmapped_destination_property_named_type()
        {
            Mapper.Initialize(c=>c.CreateMap<Source, Destination>());
            new Action(Mapper.AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>(
                ex=>ex.Errors[0].UnmappedPropertyNames[0].ShouldEqual("Type"));
        }
    }
}