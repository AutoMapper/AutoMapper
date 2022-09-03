namespace AutoMapper.UnitTests.Bug;

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
            cfg.CreateProjection<string, DummyTypes?>().ConvertUsing(s => (DummyTypes)System.Enum.Parse(typeof(DummyTypes),s));
            cfg.CreateProjection<DummySource, DummyDestination>();
        });

        config.AssertConfigurationIsValid();

        var src = new DummySource[] { new DummySource { Dummy = "Foo" } };

        var destination = src.AsQueryable().ProjectTo<DummyDestination>(config).First();

        destination.Dummy.ShouldBe(DummyTypes.Foo);
    }
}
