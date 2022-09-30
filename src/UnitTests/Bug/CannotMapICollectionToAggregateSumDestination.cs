namespace AutoMapper.UnitTests.Bug;

public class CannotMapICollectionToAggregateSumDestination
{
    class DummySource
    {
        public ICollection<int> DummyCollection { get; set; }
    }

    class DummyDestination
    {
        public int DummyCollectionSum { get; set; }
    }

    [Fact]
    public void Should_map_icollection_to_aggregate_sum_destination()
    {
        // arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateProjection<DummySource, DummyDestination>();
        });

        // act
        // do nothing

        // assert
        config.AssertConfigurationIsValid();
    }

    [Fact]
    public void Should_project_icollection_to_aggregate_sum_destination()
    {
        // arrange
        var config = new MapperConfiguration(cfg => cfg.CreateProjection<DummySource, DummyDestination>());
        var source = new DummySource() { DummyCollection = new[] { 1, 4, 5 } };

        // act
        var destination = new[] { source }.AsQueryable()
            .ProjectTo<DummyDestination>(config)
            .Single();

        // assert
        destination.DummyCollectionSum.ShouldBe(10);
    }
}
