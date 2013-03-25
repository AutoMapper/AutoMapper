namespace UnitTest.WP8
{
    using AutoMapper;

    using Xunit;

    public class AutoMapperWpSupportTest
    {
        [Fact]
        public void Should_be_able_to_create_a_map()
        {
            var foo = new Foo() { Bar = new Bar() { Value = "someValue" } };

            Mapper.CreateMap<Foo, FooBar>();

            var fooBar = Mapper.Map<FooBar>(foo);

            Assert.Equal(foo.Bar.Value, fooBar.BarValue);
        }
    }

    public class Foo
    {
        public Bar Bar { get; set; }
    }

    public class Bar
    {
        public string Value { get; set; }
    }

    public class FooBar
    {
        public string BarValue { get; set; }
    }
}
