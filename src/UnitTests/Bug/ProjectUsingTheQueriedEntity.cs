namespace AutoMapper.UnitTests.Bug;

public class ProjectUsingTheQueriedEntity : AutoMapperSpecBase
{
    private Destination _destination;

    class Source
    {
        public int Number { get; set; }
    }
    class Destination
    {
        public int Number { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Source, Destination>().ConvertUsing(s => new Destination {Number = 23 + s.Number});
    });

    protected override void Because_of()
    {
        _destination = new[] { new Source() }.AsQueryable().ProjectTo<Destination>(Configuration).First();
    }

    [Fact]
    public void Should_handle_projectusing_with_the_queried_entity()
    {
        _destination.Number.ShouldBe(23);
    }
}