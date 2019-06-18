using Xunit;
using Shouldly;
using System;
using AutoMapper.Mappers;

namespace AutoMapper.UnitTests.Bug
{
    public class RemovePrefixes : NonValidatingSpecBase
    {
        class Source
        {
            public int GetNumber { get; set; }
        }
        class Destination
        {
            public int Number { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.ClearPrefixes();
            cfg.CreateMap<Source, Destination>();
        });

        [Fact]
        public void Should_not_map_with_default_postfix()
        {
            new Action(Configuration.AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>();
        }
    }
}