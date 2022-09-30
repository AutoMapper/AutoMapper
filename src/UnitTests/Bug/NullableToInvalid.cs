namespace AutoMapper.UnitTests.Bug;

public class NullableToInvalid : NonValidatingSpecBase
{
    public class Source
    {
        public int? Value { get; set; }
    }

    public class Destination
    {
        public SomeObject Value { get; set; }
    }

    public class SomeObject
    {
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_not_validate()
    {
        new Action(AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>();
    }
}
