using AutoMapper;
using System;
using Xunit;
using Shouldly;

namespace AutoMapper.UnitTests
{
    public class MaxExecutionPlanDepth
    {
        class Source
        {
            public Source1 Inner { get; set; }
        }

        class Source1
        {
            public Source2 Inner { get; set; }
        }

        class Source2
        {
            public Source3 Inner { get; set; }
        }

        class Source3
        {
            public Source4 Inner { get; set; }
        }

        class Source4
        {
            public Source5 Inner { get; set; }
        }

        class Source5
        {
            public Source6 Inner { get; set; }
        }

        class Source6
        {
            public int Value { get; set; }
        }

        class Destination
        {
            public Destination1 Inner { get; set; }
        }

        class Destination1
        {
            public Destination2 Inner { get; set; }
        }

        class Destination2
        {
            public Destination3 Inner { get; set; }
        }

        class Destination3
        {
            public Destination4 Inner { get; set; }
        }

        class Destination4
        {
            public Destination5 Inner { get; set; }
        }

        class Destination5
        {
            public Destination6 Inner { get; set; }
        }

        class Destination6
        {
            public int Value { get; set; }
        }

        [Fact]
        public void Should_set_inline_accordingly()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.Advanced.MaxExecutionPlanDepth = 2;
                cfg.CreateMap<Source, Destination>();
                cfg.CreateMap<Source1, Destination1>();
                cfg.CreateMap<Source2, Destination2>();
                cfg.CreateMap<Source3, Destination3>();
                cfg.CreateMap<Source4, Destination4>();
                cfg.CreateMap<Source5, Destination5>();
                cfg.CreateMap<Source6, Destination6>();
            });
            TypeMap map;
            map = configuration.FindTypeMapFor<Source, Destination>();
            map.GetPropertyMaps()[0].Inline.ShouldBeTrue();
            map = configuration.FindTypeMapFor<Source1, Destination1>();
            map.GetPropertyMaps()[0].Inline.ShouldBeTrue();
            map = configuration.FindTypeMapFor<Source2, Destination2>();
            map.GetPropertyMaps()[0].Inline.ShouldBeFalse();
            map = configuration.FindTypeMapFor<Source3, Destination3>();
            map.GetPropertyMaps()[0].Inline.ShouldBeTrue();
            map = configuration.FindTypeMapFor<Source4, Destination4>();
            map.GetPropertyMaps()[0].Inline.ShouldBeTrue();
            map = configuration.FindTypeMapFor<Source5, Destination5>();
            map.GetPropertyMaps()[0].Inline.ShouldBeFalse();
        }
    }
}