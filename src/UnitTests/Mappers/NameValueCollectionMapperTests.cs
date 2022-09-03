using System.Collections.Specialized;

namespace AutoMapper.UnitTests.Mappers;

public class NameValueCollectionMapperTests
{
    public class Map
    {
        [Fact]
        public void ReturnsTheDestinationWhenPassedOne()
        {
            var config = new MapperConfiguration(cfg => { });
            IMapper mapper = new Mapper(config);

            var destination = new NameValueCollection();

            var result = mapper.Map((NameValueCollection)null, destination);

            result.ShouldBeSameAs(destination);
        }

        [Fact]
        public void ReturnsEmptyCollectionWhenSourceCollectionIsEmpty()
        {
            var config = new MapperConfiguration(cfg => { });
            IMapper mapper = new Mapper(config);

            var result = mapper.Map(new NameValueCollection(), (NameValueCollection)null);

            result.ShouldBeEmpty(); 
        }

        [Fact]
        public void ReturnsMappedObjectWithExpectedValuesWhenSourceCollectionHasOneItem()
        {
            var config = new MapperConfiguration(cfg => { });
            IMapper mapper = new Mapper(config);
            var sourceValue = new NameValueCollection() { { "foo", "bar" } };

            var result = mapper.Map(sourceValue, new NameValueCollection());

            1.ShouldBe(result.Count);
            "foo".ShouldBe(result.AllKeys[0]);
            "bar".ShouldBe(result["foo"]);
        }
    }
    
}
