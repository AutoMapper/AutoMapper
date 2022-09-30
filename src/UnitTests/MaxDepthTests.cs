namespace AutoMapper.UnitTests;

public class MaxDepthTests
{
    public class Source
    {
        public int Level { get; set; }
        public IList<Source> Children { get; set; }
        public Source Parent { get; set; }

        public Source(int level)
        {
            Children = new List<Source>();
            Level = level;
        }

        public void AddChild(Source child)
        {
            Children.Add(child);
            child.Parent = this;
        }
    }

    public class Destination
    {
        public int Level { get; set; }
        public IList<Destination> Children { get; set; }
        public Destination Parent { get; set; }
    }

    private readonly Source _source;

    public MaxDepthTests()
    {
        var nest = new Source(1);

        nest.AddChild(new Source(2));
        nest.Children[0].AddChild(new Source(3));
        nest.Children[0].AddChild(new Source(3));
        nest.Children[0].Children[1].AddChild(new Source(4));
        nest.Children[0].Children[1].AddChild(new Source(4));
        nest.Children[0].Children[1].AddChild(new Source(4));

        nest.AddChild(new Source(2));
        nest.Children[1].AddChild(new Source(3));

        nest.AddChild(new Source(2));
        nest.Children[2].AddChild(new Source(3));

        _source = nest;
    }

    [Fact]
    public void Second_level_children_is_empty_with_max_depth_1()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>().MaxDepth(1));
        var destination = config.CreateMapper().Map<Source, Destination>(_source);
        destination.Children.ShouldBeEmpty();
    }

    [Fact]
    public void Second_level_children_are_not_null_with_max_depth_2()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>().MaxDepth(2));
        var destination = config.CreateMapper().Map<Source, Destination>(_source);
        foreach (var child in destination.Children)
        {
            2.ShouldBe(child.Level);
            child.ShouldNotBeNull();
            destination.ShouldBe(child.Parent);
        }
    }

    [Fact]
    public void Third_level_children_is_empty_with_max_depth_2()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>().MaxDepth(2));
        var destination = config.CreateMapper().Map<Source, Destination>(_source);
        foreach (var child in destination.Children)
        {
            child.Children.ShouldBeEmpty();
        }
    }

    [Fact]
    public void Third_level_children_are_not_null_max_depth_3()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>().MaxDepth(3));
        var destination = config.CreateMapper().Map<Source, Destination>(_source);
        foreach (var child in destination.Children)
        {
            child.Children.ShouldNotBeNull();
            foreach (var subChild in child.Children)
            {
                3.ShouldBe(subChild.Level);
                subChild.Children.ShouldNotBeNull();
                child.ShouldBe(subChild.Parent);
            }
        }
    }
}
