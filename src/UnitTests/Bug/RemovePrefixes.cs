using Xunit;
using Should;
using System;
using AutoMapper.Mappers;

namespace AutoMapper.UnitTests.Bug
{
    public class RemovePrefixes : SpecBase
    {
        MapperConfiguration config;

        class Source
        {
            public int GetNumber { get; set; }
        }
        class Destination
        {
            public int Number { get; set; }
        }

        protected override void Establish_context()
        {
            config = new MapperConfiguration();
            config.ClearPrefixes();
            config.CreateMap<Source, Destination>();
        }

        [Fact]
        public void Should_not_map_with_default_postfix()
        {
            new Action(config.AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>();
        }
    }
}