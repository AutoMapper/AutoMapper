namespace AutoMapper.UnitTests;

public class EnumToNullableEnum : AutoMapperSpecBase
{
    Destination _destination;
    public enum SomeEnum { Foo, Bar }

    public class Source
    {
        public SomeEnum EnumValue { get; set; }
    }

    public class Destination
    {
        public SomeEnum? EnumValue { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    protected override void Because_of()
    {
        _destination = Mapper.Map<Source, Destination>(new Source{ EnumValue = SomeEnum.Bar });
    }

    [Fact]
    public void Should_map_enum_to_nullable_enum()
    {
        _destination.EnumValue.ShouldBe(SomeEnum.Bar);
    }
}
