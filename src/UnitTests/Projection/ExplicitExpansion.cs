namespace AutoMapper.UnitTests.Projection;
public class ExplicitExpansion : AutoMapperSpecBase
{
    private Dest[] _dests;

    public class Source
    {
        public ChildSource Child1 { get; set; }
        public ChildSource Child2 { get; set; }
        public ChildSource Child3 { get; set; }
    }

    public class ChildSource
    {
        
    }

    public class Dest
    {
        public ChildDest Child1 { get; set; }
        public ChildDest Child2 { get; set; }
        public ChildDest Child3 { get; set; }
    }

    public class ChildDest
    {
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Dest>()
            .ForMember(m => m.Child1, opt => opt.ExplicitExpansion())
            .ForMember(m => m.Child2, opt => opt.ExplicitExpansion())
            ;
        cfg.CreateProjection<ChildSource, ChildDest>();
    });
        

    protected override void Because_of()
    {
        var sourceList = new[]
        {
            new Source
            {
                Child1 = new ChildSource(),
                Child2 = new ChildSource(),
                Child3 = new ChildSource()
            }
        };

        _dests = sourceList.AsQueryable().ProjectTo<Dest>(Configuration, d => d.Child2).ToArray();
    }

    [Fact]
    public void Should_leave_non_expanded_item_null()
    {
        _dests[0].Child1.ShouldBeNull();
    }

    [Fact]
    public void Should_expand_explicitly_expanded_item()
    {
        _dests[0].Child2.ShouldNotBeNull();
    }

    [Fact]
    public void Should_default_to_expand()
    {
        _dests[0].Child3.ShouldNotBeNull();
    }
}