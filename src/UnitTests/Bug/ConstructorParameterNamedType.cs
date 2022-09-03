namespace AutoMapper.UnitTests.Bug;

public class ConstructorParameterNamedType
{
    public class SourceClass { }

    public class DestinationClass
    {
        public DestinationClass() { }

        public DestinationClass(int type)
        {
            Type = type;
        }

        public int Type { get; private set; }
    }

    [Fact]
    public void Should_handle_constructor_parameter_named_type()
    {
        var config = new MapperConfiguration(c => c.CreateMap<SourceClass, DestinationClass>());
        new Action(config.AssertConfigurationIsValid).ShouldThrowException<AutoMapperConfigurationException>(ex=>ex.Errors[0].UnmappedPropertyNames[0].ShouldBe("Type"));
    }
}