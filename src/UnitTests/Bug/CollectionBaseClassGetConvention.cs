using System;
using System.Collections.Generic;
using System.Linq;
using Should;
using AutoMapper;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class CollectionBaseClassGetConvention : AutoMapperSpecBase
    {
        Destination _destination;
        static int[] SomeCollection = new[] { 1, 2, 3 };

        public abstract class SourceBase
        {
            public IEnumerable<int> GetItems()
            {
                return SomeCollection;
            }
        }

        public class Source : SourceBase
        {
        }

        public class Destination
        {
            public IEnumerable<int> Items { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Destination>();
        }

        protected override void Because_of()
        {
            _destination = Mapper.Map<Destination>(new Source());
        }

        [Fact]
        public void Should_map_collection_with_get_convention()
        {
            _destination.Items.SequenceEqual(SomeCollection).ShouldBeTrue();
        }
    }
}