namespace AutoMapper.UnitTests.Bug;
public class ListSourceMapperBug
{
    public class CustomCollection<T> : Collection<T>, IListSource
    {
        public IList GetList()
        {
            return new ReadOnlyCollection<T>(this.ToList());
        }

        public bool ContainsListCollection
        {
            get { return true; }
        }
    }

    public class Source
    {
    }

    public class Dest
    {
    }

    [Fact]
    public void CustomListSourceShouldNotBlowUp()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>());

        var source = new CustomCollection<Source> {new Source()};

        var dests = config.CreateMapper().Map<CustomCollection<Source>, CustomCollection<Dest>>(source);

        dests.Count.ShouldBe(1);
    }
}