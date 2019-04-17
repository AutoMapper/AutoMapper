using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class CannotConvertByteEnumToNullable
    {
        public enum DummyTypes : byte
        {
            Foo = 1,
            Bar = 2
        }

        public class DummySource
        {
            public DummyTypes Dummy { get; set; }
        }

        public class DummyDestination
        {
            public int? Dummy { get; set; }
        }

        [Fact]
        public void Should_map_byte_enum_to_nullable_int()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<DummySource, DummyDestination>());
            config.AssertConfigurationIsValid();
            DummySource src = new DummySource() { Dummy = DummyTypes.Bar };

            var destination = config.CreateMapper().Map<DummySource, DummyDestination>(src);

            destination.Dummy.ShouldBe((int)DummyTypes.Bar);
        }
    }
}
