using Xunit;
using Should;

namespace AutoMapper.UnitTests.Tests
{
    public class MapperTests : SpecBase
    {
        public class Source
        {
            
        }
        
        public class Destination
        {
            
        }
            
        [Fact]
        public void Should_find_configured_type_map_when_two_types_are_configured()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>());

            config.FindTypeMapFor<Source, Destination>().ShouldNotBeNull();
        }
    }
}