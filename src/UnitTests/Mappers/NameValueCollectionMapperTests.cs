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
                var rc = new ResolutionContext(null, null, new NameValueCollection(), typeof(NameValueCollection), typeof(NameValueCollection), null, Mapper.Engine);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc);

                result.ShouldBeNull();
            }

            [Fact]
            public void ReturnsEmptyCollectionWhenSourceCollectionIsEmpty()
            {
                var sourceValue = new NameValueCollection();
                var rc = new ResolutionContext(null, sourceValue, new NameValueCollection(), typeof(NameValueCollection), typeof(NameValueCollection), null, Mapper.Engine);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc) as NameValueCollection;

                result.ShouldBeEmpty(); 
            }

            [Fact]
            public void ReturnsMappedObjectWithExpectedValuesWhenSourceCollectionHasOneItem()
            {
                var sourceValue = new NameValueCollection() { { "foo", "bar" } };
                var rc = new ResolutionContext(null, sourceValue, new NameValueCollection(), typeof(NameValueCollection), typeof(NameValueCollection), null, Mapper.Engine);

                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc) as NameValueCollection;

                1.ShouldEqual(result.Count);
                "foo".ShouldEqual(result.AllKeys[0]);
                "bar".ShouldEqual(result["foo"]);
            }
        }
        
    }
}
