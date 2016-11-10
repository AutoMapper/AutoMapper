using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class AssertConfigurationIsValidNullables : AutoMapperSpecBase
    {
        class Source
        {
            public int? Number { get; set; }
        }
        class Destination
        {
            public decimal? Number { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });
    }
}