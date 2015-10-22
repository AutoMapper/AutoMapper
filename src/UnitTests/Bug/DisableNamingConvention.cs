using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class DisableNamingConvention : NonValidatingSpecBase
    {
        class Source
        {
            public string Name { get; set; }
        }
        class Destination
        {
            public string Name { get; set; }

            public string COMPANY_Name { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.DestinationMemberNamingConvention = null;
                cfg.CreateMap<Source, Destination>();
            });
        }

        [Fact]
        public void Should_not_use_pascal_naming_convention()
        {
            new Action(Mapper.AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>(
                ex=>ex.Errors[0].UnmappedPropertyNames.ShouldContain("COMPANY_Name")
            );
        }
    }
}