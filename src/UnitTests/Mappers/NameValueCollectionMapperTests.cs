using System;
using System.Collections.Specialized;
using AutoMapper.Mappers;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Mappers
{
    public class NameValueCollectionMapperTests
    {
        public class IsMatch
        {
            [Fact]
            public void ReturnsTrueWhenBothSourceAndDestinationTypesAreNameValueCollection()
            {
                var tp = new TypePair(typeof(NameValueCollection), typeof(NameValueCollection));
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.IsMatch(tp);

                result.ShouldBeTrue();
            }

            [Fact]
            public void ReturnsIsFalseWhenDestinationTypeIsNotNameValueCollection()
            {
                var tp = new TypePair(typeof(NameValueCollection), typeof(Object));
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.IsMatch(tp);

                result.ShouldBeFalse();
            }            

            [Fact]
            public void ReturnsIsFalseWhenSourceTypeIsNotNameValueCollection()
            {
                var tp = new TypePair(typeof(Object), typeof(NameValueCollection));
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.IsMatch(tp);

                result.ShouldBeFalse();
            }            
        }
        public class Map
        {
            [Fact]
            public void ReturnsNullIfSourceValueIsNull()
            {
                var config = new MapperConfiguration(_ => { });
                IMapper mapper = new Mapper(config);

                var result = mapper.Map((NameValueCollection)null, new NameValueCollection());

                result.ShouldBeNull();
            }

            [Fact]
            public void ReturnsEmptyCollectionWhenSourceCollectionIsEmpty()
            {
                var config = new MapperConfiguration(_ => { });
                IMapper mapper = new Mapper(config);

                var result = mapper.Map(new NameValueCollection(), (NameValueCollection)null);

                result.ShouldBeEmpty(); 
            }

            [Fact]
            public void ReturnsMappedObjectWithExpectedValuesWhenSourceCollectionHasOneItem()
            {
                var config = new MapperConfiguration(_ => { });
                IMapper mapper = new Mapper(config);
                var sourceValue = new NameValueCollection() { { "foo", "bar" } };

                var result = mapper.Map(sourceValue, new NameValueCollection());

                1.ShouldEqual(result.Count);
                "foo".ShouldEqual(result.AllKeys[0]);
                "bar".ShouldEqual(result["foo"]);
            }
        }
        
    }
}
