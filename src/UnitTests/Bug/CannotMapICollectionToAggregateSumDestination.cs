using Xunit;
using System.Collections.Generic;

namespace AutoMapper.UnitTests.Bug
{
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
                cfg.CreateMap<DummySource, DummyDestination>();
            });

            // act
            // do nothing

            // assert
            config.AssertConfigurationIsValid();
        }
    }
}
