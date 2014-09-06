namespace AutoMapper.UnitTests
{
    using System.Reflection;
    using Should;
    using Xunit;

    public class BindingFlagsConfiguration : AutoMapperSpecBase
    {
        private Destination _dest;

        public class Source
        {
            internal int Value { get; set; }
        }

        public class Destination
        {
            internal int Value { get; set; }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                cfg.CreateMap<Source, Destination>();
            });
        }

        protected override void Because_of()
        {
            _dest = Mapper.Map<Source, Destination>(new Source {Value = 10});
        }

        [Fact]
        public void Should_map_internal_value()
        {
            _dest.Value.ShouldEqual(10);
        }
    }
}