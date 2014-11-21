namespace AutoMapper.UnitTests
{
    using Should;
    using Xunit;

    public class OpenGenerics
    {
        public class Source<T>
        {
            public T Value { get; set; }
        }

        public class Dest<T>
        {
            public T Value { get; set; }
        }

        [Fact]
        public void Can_map_simple_generic_types()
        {
            Mapper.Initialize(cfg => cfg.CreateMap(typeof(Source<>), typeof(Dest<>)));

            var source = new Source<int>
            {
                Value = 5
            };

            var dest = Mapper.Map<Source<int>, Dest<int>>(source);

            dest.Value.ShouldEqual(5);
        }
    }
}