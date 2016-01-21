using System;
using Should;
using AutoMapper;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class NullableDateTime : AutoMapperSpecBase
    {
        Destination _destination;
        DateTime _date = new DateTime(1900, 1, 1);

        public class Source
        {
            public DateTime Value { get; set; }
        }

        public class Destination
        {
            public DateTime Value { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<DateTime, DateTime?>()
                .ConvertUsing(source => source == new DateTime(1900, 1, 1) ? (DateTime?) null : source);
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source { Value = _date });
        }

        [Fact]
        public void Should_map_as_usual()
        {
            _destination.Value.ShouldEqual(_date);
        }
    }
}