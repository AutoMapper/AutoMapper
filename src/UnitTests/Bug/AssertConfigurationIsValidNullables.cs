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

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Source, Destination>();
            });
        }
    }
}