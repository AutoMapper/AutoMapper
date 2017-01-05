using Should;
using Xunit;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace AutoMapper.UnitTests.Bug
{
    public class CannotProjectStringToNullableEnum
    {
        public enum DummyTypes : int
        {
            Foo = 1,
            Bar = 2
        }

        public class DummySource
        {
            public string Dummy { get; set; }
        }

        public class DummyDestination
        {
            public DummyTypes? Dummy { get; set; }
        }

        [Fact]
        public void Should_project_string_to_nullable_enum()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<string, DummyTypes?>().ProjectUsing(s => (DummyTypes)System.Enum.Parse(typeof(DummyTypes),s));
                cfg.CreateMap<DummySource, DummyDestination>();
            });

            config.AssertConfigurationIsValid();

            var src = new DummySource[] { new DummySource { Dummy = "Foo" } };

            var destination = src.AsQueryable().ProjectTo<DummyDestination>(config).First();

            destination.Dummy.ShouldEqual(DummyTypes.Foo);
        }
    }
}
