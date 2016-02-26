using Xunit;
using Should;
using System;

namespace AutoMapper.UnitTests.Bug
{
    public class NullableResolveUsing : AutoMapperSpecBase
    {
        private Destination _destination;

        class Source
        {
            public decimal? Number { get; set; }
        }
        class Destination
        {
            public decimal? OddNumber { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().ForMember(d=>d.OddNumber, o=>o.ResolveUsing(s=>s.Number));
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_map_nullable_decimal_with_ResolveUsing()
        {
            _destination.OddNumber.ShouldBeNull();
        }
    }
}