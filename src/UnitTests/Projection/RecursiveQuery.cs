namespace AutoMapper.UnitTests.Projection;

public class RecursiveQuery : AutoMapperSpecBase
{
    class Source
    {
        public int Id { get; set; }
        public Source Parent { get; set; }
    }
    class Destination
    {
        public int Id { get; set; }
        public Destination Parent { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(c=>
    {
        c.CreateProjection<Source, Destination>();
        c.Internal().RecursiveQueriesMaxDepth = 1;
    });
    [Fact]
    public void Should_work()
    {
        var source = new[] { new Source { Id = 1, Parent = new Source { Id = 2, Parent = new Source { } } }, new Source { Id = 3, Parent = new Source { Id = 4, Parent = new Source { } } } };
        var result = ProjectTo<Destination>(source.AsQueryable()).ToArray();
        result[0].Id.ShouldBe(1);
        result[0].Parent.Id.ShouldBe(2);
        result[0].Parent.Parent.ShouldBeNull();
        result[1].Id.ShouldBe(3);
        result[1].Parent.Id.ShouldBe(4);
        result[1].Parent.Parent.ShouldBeNull();
    }
}