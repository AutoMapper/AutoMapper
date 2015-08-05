using System;
using System.Linq;
using Should;
using Xunit;
using AutoMapper;

namespace AutoMapper.UnitTests.Bug
{
    public class DynamicMapArrays : AutoMapperSpecBase
    {
        Source[] source;
        Destination[] destination;

        public class Source
        {
            public Source(int value)
            {
                Value = value;
            }

            public int Value { get; set; }
        }

        public class Destination
        {
            public int Value { get; set; }
        }

        protected override void Because_of()
        {
            source = Enumerable.Range(0, 9).Select(i => new Source(i)).ToArray();
            destination = Mapper.DynamicMap<Destination[]>(source);
            //Mapper.CreateMap<Source, Destination>();
            //destination = Mapper.Map<Destination[]>(source);
        }

        [Fact]
        public void Should_dynamic_map_the_array()
        {
            destination.Length.ShouldEqual(source.Length);
            Array.TrueForAll(source, s => s.Value == destination[s.Value].Value).ShouldBeTrue(); 
        }
    }
}