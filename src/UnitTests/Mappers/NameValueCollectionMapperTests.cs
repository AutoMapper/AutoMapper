#if !PORTABLE
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
                var mapper = new Mapper(config);
                var rc = new ResolutionContext(null, new NameValueCollection(), new TypePair(typeof(NameValueCollection), typeof(NameValueCollection)), new MappingOperationOptions(config.ServiceCtor), mapper);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc);

                result.ShouldBeNull();
            }

            [Fact]
            public void ReturnsEmptyCollectionWhenSourceCollectionIsEmpty()
            {
                var config = new MapperConfiguration(_ => { });
                var mapper = new Mapper(config);
                var sourceValue = new NameValueCollection();
                var rc = new ResolutionContext(sourceValue, null, new TypePair(typeof(NameValueCollection), typeof(NameValueCollection)), new MappingOperationOptions(config.ServiceCtor), mapper);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc) as NameValueCollection;

                result.ShouldBeEmpty(); 
            }

            [Fact]
            public void ReturnsMappedObjectWithExpectedValuesWhenSourceCollectionHasOneItem()
            {
                var config = new MapperConfiguration(_ => { });
                var mapper = new Mapper(config);
                var sourceValue = new NameValueCollection() { { "foo", "bar" } };
                var rc = new ResolutionContext(sourceValue, new NameValueCollection(), new TypePair(typeof(NameValueCollection), typeof(NameValueCollection)), new MappingOperationOptions(config.ServiceCtor), mapper);

                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc) as NameValueCollection;

                1.ShouldEqual(result.Count);
                "foo".ShouldEqual(result.AllKeys[0]);
                "bar".ShouldEqual(result["foo"]);
            }
        }
        
    }
}
#endif