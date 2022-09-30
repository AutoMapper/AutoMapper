namespace AutoMapper.UnitTests.Bug;

public class PropertyNamedType
{
    class Source
    {
        public int Number { get; set; }
    }
    class Destination
    {
        public int Type { get; set; }
    }

    [Fact]
    public void Should_detect_unmapped_destination_property_named_type()
    {
        var config = new MapperConfiguration(c=>c.CreateMap<Source, Destination>());
        new Action(config.AssertConfigurationIsValid).ShouldThrowException<AutoMapperConfigurationException>(
            ex=>ex.Errors[0].UnmappedPropertyNames[0].ShouldBe("Type"));
    }
}