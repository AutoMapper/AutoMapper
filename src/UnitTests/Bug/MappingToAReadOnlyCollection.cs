using System;
using System.Collections.ObjectModel;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    public class MappingToAReadOnlyCollection : AutoMapperSpecBase
    {
        private Destination _destination;

        public class Source
        {
            public int[] Values { get; set; }
        }

        public class Destination
        {
            public ReadOnlyCollection<int> Values { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.CreateMap<Source, Destination>();
        }

        protected override void Because_of()
        {
            var source = new Source
                             {
                                 Values = new[] {1, 2, 3, 4},
                             };
            _destination = Mapper.Map<Source, Destination>(source);
        }

        [Test]
        public void Should_map_the_list_of_source_items()
        {
            _destination.Values.ShouldNotBeNull();
            _destination.Values.ShouldContain(1);
            _destination.Values.ShouldContain(2);
            _destination.Values.ShouldContain(3);
            _destination.Values.ShouldContain(4);
        }
    }
}