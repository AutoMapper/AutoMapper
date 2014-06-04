using System.Collections.Generic;
using System.Collections.ObjectModel;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    // Bug #511
    // https://github.com/AutoMapper/AutoMapper/issues/511
    public class ReadOnlyCollectionMappingBug
    {
        class Source { public int X { get; set; } }
        class Target { public int X { get; set; } }

        [Fact]
        public void Example()
        {
            Mapper.CreateMap<Source, Target>();

            var source = new List<Source> { new Source { X = 42 } };
            var target = Mapper.Map<ReadOnlyCollection<Target>>(source);

            target.Count.ShouldEqual(source.Count);
            target[0].X.ShouldEqual(source[0].X);
        }
    }
}
