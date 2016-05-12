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
        public class Map
        {
            [Fact]
            public void ReturnsNullIfSourceValueIsNull()
            {
                var config = new MapperConfiguration(_ => { });
                var mapper = new Mapper(config);
                var dest = new NameValueCollection();
                var rc = new ResolutionContext(null, dest, new TypePair(typeof(NameValueCollection), typeof(NameValueCollection)), new MappingOperationOptions(config.ServiceCtor), mapper);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(null, dest, rc);

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

                var result = nvcm.Map(sourceValue, null, rc);

                result.ShouldBeEmpty(); 
            }

            [Fact]
            public void ReturnsMappedObjectWithExpectedValuesWhenSourceCollectionHasOneItem()
            {
                var config = new MapperConfiguration(_ => { });
                var mapper = new Mapper(config);
                var sourceValue = new NameValueCollection() { { "foo", "bar" } };
                var dest = new NameValueCollection();
                var rc = new ResolutionContext(sourceValue, dest, new TypePair(typeof(NameValueCollection), typeof(NameValueCollection)), new MappingOperationOptions(config.ServiceCtor), mapper);

                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(sourceValue, dest, rc);

                1.ShouldEqual(result.Count);
                "foo".ShouldEqual(result.AllKeys[0]);
                "bar".ShouldEqual(result["foo"]);
            }
        }
        
    }
}
#endif