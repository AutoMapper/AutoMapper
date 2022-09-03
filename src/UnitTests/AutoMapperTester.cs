namespace AutoMapper.UnitTests;

public class AutoMapperTester : IDisposable
{
    [Fact]
    public void Should_be_able_to_handle_derived_proxy_types()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<ModelType, DtoType>());
        var source = new[] { new DerivedModelType { TheProperty = "Foo" }, new DerivedModelType { TheProperty = "Bar" } };

        var mapper = config.CreateMapper();
        var destination = (DtoType[])mapper.Map(source, typeof(ModelType[]), typeof(DtoType[]));

        destination[0].TheProperty.ShouldBe("Foo");
        destination[1].TheProperty.ShouldBe("Bar");
    }

    public void Dispose()
    {
        
    }

    public class ModelType
    {
        public string TheProperty { get; set; }
    }

    public class DerivedModelType : ModelType
    {
    }

    public class DtoType
    {
        public string TheProperty { get; set; }
    }
}