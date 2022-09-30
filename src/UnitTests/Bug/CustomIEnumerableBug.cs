namespace AutoMapper.UnitTests.Bug;

public class One
{
    public IEnumerable<string> Stuff { get; set; }
}

public class Two
{
    public IEnumerable<Item> Stuff { get; set; }
}

public class Item
{
    public string Value { get; set; }
}

public class StringToItemConverter : ITypeConverter<IEnumerable<string>, IEnumerable<Item>>
{
    public IEnumerable<Item> Convert(IEnumerable<string> source, IEnumerable<Item> destination, ResolutionContext context)
    {
        var result = new List<Item>();
        foreach (string s in source)
            if (!String.IsNullOrEmpty(s))
                result.Add(new Item { Value = s });
        return result;
    }
}
public class AutoMapperBugTest
{
    [Fact]
    public void ShouldMapOneToTwo()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<One, Two>();

            cfg.CreateMap<IEnumerable<string>, IEnumerable<Item>>().ConvertUsing<StringToItemConverter>();
        });

        config.AssertConfigurationIsValid();

        var engine = config.CreateMapper();
        var one = new One
        {
            Stuff = new List<string> { "hi", "", "mom" }
        };

        var two = engine.Map<One, Two>(one);

        two.ShouldNotBeNull();
        two.Stuff.Count().ShouldBe(2);
    }
}
