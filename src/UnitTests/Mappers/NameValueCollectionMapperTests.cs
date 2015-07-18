#if NET4 || MONODROID || MONOTOUCH || __IOS__ || DNXCORE50
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
                var rc = new ResolutionContext(null, null, null, typeof(NameValueCollection), typeof(NameValueCollection), null, Mapper.Engine);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.IsMatch(rc);

                result.ShouldBeTrue();
            }

            [Fact]
            public void ReturnsIsFalseWhenDestinationTypeIsNotNameValueCollection()
            {
                var rc = new ResolutionContext(null, null, null, typeof(NameValueCollection), typeof(Object), null, Mapper.Engine);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.IsMatch(rc);

                result.ShouldBeFalse();
            }            

            [Fact]
            public void ReturnsIsFalseWhenSourceTypeIsNotNameValueCollection()
            {
                var rc = new ResolutionContext(null, null, null, typeof(Object), typeof(NameValueCollection), null, Mapper.Engine);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.IsMatch(rc);

                result.ShouldBeFalse();
            }            
        }
        public class Map
        {
            [Fact]
            public void ReturnsNullIfSourceTypeIsNotNameValueCollection()
            {
                var rc = new ResolutionContext(null, new Object(), new NameValueCollection(), typeof(Object), typeof(NameValueCollection), null, Mapper.Engine);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc, null);

                result.ShouldBeNull();
            }

            [Fact]
            public void ReturnsNullIfSourceValueIsNull()
            {
                var rc = new ResolutionContext(null, null, new NameValueCollection(), typeof(NameValueCollection), typeof(NameValueCollection), null, Mapper.Engine);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc, null);

                result.ShouldBeNull();
            }

            [Fact]
            public void ReturnsEmptyCollectionWhenSourceCollectionIsEmpty()
            {
                var sourceValue = new NameValueCollection();
                var rc = new ResolutionContext(null, sourceValue, new NameValueCollection(), typeof(NameValueCollection), typeof(NameValueCollection), null, Mapper.Engine);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc, null) as NameValueCollection;

                result.ShouldBeEmpty(); 
            }

            [Fact]
            public void ReturnsMappedObjectWithExpectedValuesWhenSourceCollectionHasOneItem()
            {
                var sourceValue = new NameValueCollection() { { "foo", "bar" } };
                var rc = new ResolutionContext(null, sourceValue, new NameValueCollection(), typeof(NameValueCollection), typeof(NameValueCollection), null, Mapper.Engine);

                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc, null) as NameValueCollection;

                1.ShouldEqual(result.Count);
                "foo".ShouldEqual(result.AllKeys[0]);
                "bar".ShouldEqual(result["foo"]);
            }
        }
        
    }
}
#endif