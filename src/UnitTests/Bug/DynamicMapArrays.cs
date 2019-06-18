using System;
using System.Linq;
using Shouldly;
using Xunit;
using AutoMapper;

namespace AutoMapper.UnitTests.Bug
{
    public class DynamicMapArrays
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

        [Fact]
        public void Should_dynamic_map_the_array()
        {
            source = Enumerable.Range(0, 9).Select(i => new Source(i)).ToArray();
            var config = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);
            destination = config.CreateMapper().Map<Destination[]>(source);
            destination.Length.ShouldBe(source.Length);
            Array.TrueForAll(source, s => s.Value == destination[s.Value].Value).ShouldBeTrue(); 
        }
    }
}