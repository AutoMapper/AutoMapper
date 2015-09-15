using Should;
using System.Dynamic;
using Xunit;

namespace AutoMapper.UnitTests.Mappers
{
    class Destination
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
    }

    public class When_mapping_from_dynamic_with_missing_property : NonValidatingSpecBase
    {
        Destination _destination;

        protected override void Because_of()
        {
            dynamic source = new ExpandoObject();
            source.Foo = "Foo";
            _destination = Mapper.Map<Destination>(source);
        }

        [Fact]
        public void Should_map_the_other_properties()
        {
            _destination.Foo.ShouldEqual("Foo");
        }
    }
}