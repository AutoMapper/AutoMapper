namespace AutoMapper.UnitTests.Bug;

public class CannotConvertEnumToNullable
{
    public enum DummyTypes : int
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
    public void Should_map_enum_to_nullable()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<DummySource, DummyDestination>());
        config.AssertConfigurationIsValid();
        DummySource src = new DummySource() { Dummy = DummyTypes.Bar };

        var destination = config.CreateMapper().Map<DummySource, DummyDestination>(src);

        destination.Dummy.ShouldBe((int)DummyTypes.Bar);
    }
}
