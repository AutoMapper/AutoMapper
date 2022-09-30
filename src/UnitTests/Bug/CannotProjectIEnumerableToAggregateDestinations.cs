namespace AutoMapper.UnitTests.Bug;

public class CannotProjectIEnumerableToAggregateDestinations
{
    class DummySource
    {
        public IEnumerable<int> DummyEnumerable { get; set; }
    }

    class DummyDestination
    {
        public int DummyEnumerableCount { get; set; }
        public int DummyEnumerableSum { get; set; }
        public int DummyEnumerableMin { get; set; }
        public int DummyEnumerableMax { get; set; }
    }

    [Fact]
    public void Should_project_ienumerable_to_aggregate_destinations()
    {
        // arrange
        var config = new MapperConfiguration(cfg => cfg.CreateProjection<DummySource, DummyDestination>());
        var source = new DummySource() { DummyEnumerable = new[] { 1, 4, 5 } };

        // act
        var destination = new[] { source }.AsQueryable()
            .ProjectTo<DummyDestination>(config)
            .Single();

        // assert
        destination.DummyEnumerableCount.ShouldBe(3);
        destination.DummyEnumerableSum.ShouldBe(10);
        destination.DummyEnumerableMin.ShouldBe(1);
        destination.DummyEnumerableMax.ShouldBe(5);
    }
}
