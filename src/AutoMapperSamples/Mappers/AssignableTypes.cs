using AutoMapper;
using Should;
using NUnit.Framework;

namespace AutoMapperSamples.Mappers
{
    namespace AssignableTypes
    {
        [TestFixture]
        public class Examples
        {
            public class Foo
            {
                public int Value { get; set; }
            }

            public class Bar : Foo
            {

            }

            [Test]
            public void SimpleTypeExample()
            {
                // No configuration needed
                var config = new MapperConfiguration(cfg => { });
                var mapper = config.CreateMapper();
                mapper.Map<int, int>(5).ShouldEqual(5);
                mapper.Map<string, string>("foo").ShouldEqual("foo");
            }

            [Test]
            public void ComplexTypeExample()
            {
                var source = new Bar { Value = 5 };

                var config = new MapperConfiguration(cfg => { });
                var mapper = config.CreateMapper();
                var dest = mapper.Map<Bar, Foo>(source);

                dest.Value.ShouldEqual(5);
            }
        }
    }
}