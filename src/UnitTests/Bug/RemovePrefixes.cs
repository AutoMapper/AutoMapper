namespace AutoMapper.UnitTests.Bug;

public class RemovePrefixes : NonValidatingSpecBase
{
    class Source
    {
        public int GetNumber { get; set; }
    }
    class Destination
    {
        public int Number { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.ClearPrefixes();
        cfg.CreateMap<Source, Destination>();
    });

    [Fact]
    public void Should_not_map_with_default_postfix()
    {
        new Action(AssertConfigurationIsValid).ShouldThrow<AutoMapperConfigurationException>();
    }
}