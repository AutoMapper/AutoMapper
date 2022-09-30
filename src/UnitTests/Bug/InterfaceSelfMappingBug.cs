namespace AutoMapper.UnitTests.Bug;

public class InterfaceSelfMappingBug
{
    public interface IFoo
    {
        int Value { get; set; } 
    }

    public class Bar : IFoo
    {
        public int Value { get; set; }
    }

    public class Baz : IFoo
    {
        public int Value { get; set; }
    }

    [Fact]
    public void Example()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.CreateMap<IFoo, IFoo>();
        });
        config.AssertConfigurationIsValid();

        IFoo bar = new Bar
        {
            Value = 5
        };
        IFoo baz = new Baz
        {
            Value = 10
        };

        config.CreateMapper().Map(bar, baz);

        baz.Value.ShouldBe(5);
    }
}