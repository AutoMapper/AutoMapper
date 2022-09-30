namespace AutoMapper.UnitTests.Bug;

public class NullableEnumToNullableValueType
{
    public class CannotConvertEnumToNullableWhenPassedNull : AutoMapperSpecBase
    {
        public enum DummyTypes : int
        {
            Foo = 1,
            Bar = 2
        }

        public class DummySource
        {
            public DummyTypes? Dummy { get; set; }
        }

        public class DummyDestination
        {
            public int? Dummy { get; set; }
        }

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap<DummySource, DummyDestination>();
        });

        [Fact]
        public void Should_map_null_enum_to_nullable_base_type()
        {
            DummySource src = new DummySource() { Dummy = null };

            var destination = Mapper.Map<DummySource, DummyDestination>(src);

            destination.Dummy.ShouldBeNull();
        }
    } 
}