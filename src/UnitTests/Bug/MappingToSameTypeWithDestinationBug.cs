using Xunit;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    // Bug #656
    // https://github.com/AutoMapper/AutoMapper/issues/656
    public class MappingToSameTypeWithDestinationBug
    {
        class Foo { public int X { get; set; } }

        [Fact]
        public void Example()
        {
            var source = new Foo { X = 42 };
            var target = new Foo();
            var result = Mapper.Map(source, target, o => o.CreateMissingTypeMaps = true);

            result.ShouldBeSameAs(target);
            target.X.ShouldEqual(source.X);
        }
    }
}
