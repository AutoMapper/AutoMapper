using System;
using System.Collections.Specialized;
using AutoMapper;
using AutoMapper.Mappers;
using NUnit.Framework;

namespace Automapper.UnitTests.Mappers
{
    [TestFixture]
    public class NameValueCollectionMapperTests
    {
        [TestFixture]
        public class IsMatch
        {
            [Test]
            public void ReturnsTrueWhenBothSourceAndDestinationTypesAreNameValueCollection()
            {
                var rc = new ResolutionContext(null, null, null, typeof(NameValueCollection), typeof(NameValueCollection), null);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.IsMatch(rc);

                Assert.IsTrue(result);
            }

            [Test]
            public void ReturnsIsFalseWhenDestinationTypeIsNotNameValueCollection()
            {
                var rc = new ResolutionContext(null, null, null, typeof(NameValueCollection), typeof(Object), null);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.IsMatch(rc);

                Assert.IsFalse(result);
            }            

            [Test]
            public void ReturnsIsFalseWhenSourceTypeIsNotNameValueCollection()
            {
                var rc = new ResolutionContext(null, null, null, typeof(Object), typeof(NameValueCollection), null);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.IsMatch(rc);

                Assert.IsFalse(result);
            }            
        }

        [TestFixture]
        public class Map
        {
            [Test]
            public void ReturnsNullIfSourceTypeIsNotNameValueCollection()
            {
                var rc = new ResolutionContext(null, new Object(), new NameValueCollection(), typeof(Object), typeof(NameValueCollection), null);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc, null);

                Assert.IsNull(result);
            }

            [Test]
            public void ReturnsNullIfSourceValueIsNull()
            {
                var rc = new ResolutionContext(null, null, new NameValueCollection(), typeof(NameValueCollection), typeof(NameValueCollection), null);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc, null);

                Assert.IsNull(result);
            }

            [Test]
            public void ReturnsEmptyCollectionWhenSourceCollectionIsEmpty()
            {
                var sourceValue = new NameValueCollection();
                var rc = new ResolutionContext(null, sourceValue, new NameValueCollection(), typeof(NameValueCollection), typeof(NameValueCollection), null);
                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc, null) as NameValueCollection;

                Assert.IsEmpty(result); 
            }

            [Test]
            public void ReturnsMappedObjectWithExpectedValuesWhenSourceCollectionHasOneItem()
            {
                var sourceValue = new NameValueCollection() { { "foo", "bar" } };
                var rc = new ResolutionContext(null, sourceValue, new NameValueCollection(), typeof(NameValueCollection), typeof(NameValueCollection), null);

                var nvcm = new NameValueCollectionMapper();

                var result = nvcm.Map(rc, null) as NameValueCollection;

                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("foo", result.AllKeys[0]);
                Assert.AreEqual("bar", result["foo"]);
            }
        }
        
    }
}
