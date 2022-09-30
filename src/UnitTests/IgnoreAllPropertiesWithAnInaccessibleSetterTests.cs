namespace AutoMapper.UnitTests;

public class SomeSource
{
    public int IgnoreMe { get; set; }
}

public class Destination : DestinationBase
{
}

public class DestinationBase
{
    public int IgnoreMe { get; private set; }
}

public class IgnoreAllPropertiesWithAnInaccessibleSetterTests
{
    [Fact]
    public void AutoMapper_SimpleObject_IgnoresPrivateSettersInBaseClasses()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SomeSource, Destination>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter();
        });
        var mapper = config.CreateMapper();

        var source = new SomeSource { IgnoreMe = 666 };
        var destination = new Destination();

        // Act
        mapper.Map(source, destination);

        // Assert
        config.AssertConfigurationIsValid();
        Assert.Equal(0, destination.IgnoreMe);
    }
}
