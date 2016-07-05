using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class ForAllMaps : AutoMapperSpecBase
    {
        private Destination _destination;
        private Destination1 _destination1;
        private Destination2 _destination2;

        class Source
        {
            public int Number { get; set; }
        }
        class Destination
        {
            public int Number { get; set; }
        }

        class Source1
        {
            public int Number { get; set; }
        }
        class Destination1
        {
            public int Number { get; set; }
        }

        class Source2
        {
            public int Number { get; set; }
        }
        class Destination2
        {
            public int Number { get; set; }
        }

        public class MinusOneResolver : IValueResolver<object, object, object>
        {
            public object Resolve(object source, object dest, object destMember, ResolutionContext context)
            {
                return -1;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<Source1, Destination1>();
            cfg.CreateMap<Source2, Destination2>();
            cfg.ForAllMaps((tm, map) => map.ForMember("Number", o => o.ResolveUsing<MinusOneResolver>()));
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Source, Destination>(new Source());
            _destination1 = Mapper.Map<Source1, Destination1>(new Source1());
            _destination2 = Mapper.Map<Source2, Destination2>(new Source2());
        }

        [Fact]
        public void Should_configure_all_maps()
        {
            _destination.Number.ShouldEqual(-1);
            _destination1.Number.ShouldEqual(-1);
            _destination2.Number.ShouldEqual(-1);
        }
    }
}