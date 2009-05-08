using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
    namespace Regression
    {
        public class TestDomainItem : ITestDomainItem
        {
            public Guid ItemId { get; set; }
        }

        public interface ITestDomainItem
        {
            Guid ItemId { get; }
        }

        public class TestDtoItem
        {
            public Guid Id { get; set; }
        }

        [TestFixture]
        public class automapper_fails_to_map_custom_mappings_when_mapping_collections_for_an_interface
        {
            [SetUp]
            public void Setup()
            {
                Mapper.Reset();
            }

            [Test]
            public void should_map_the_id_property()
            {
                var domainItems = new List<ITestDomainItem>
                {
                    new TestDomainItem {ItemId = Guid.NewGuid()},
                    new TestDomainItem {ItemId = Guid.NewGuid()}
                };
                Mapper.CreateMap<ITestDomainItem, TestDtoItem>()
                    .ForMember(d => d.Id, s => s.MapFrom(x => x.ItemId));

                Mapper.AssertConfigurationIsValid();

                var dtos = Mapper.Map<IEnumerable<ITestDomainItem>, TestDtoItem[]>(domainItems);

                Assert.AreEqual(domainItems[0].ItemId, dtos[0].Id);
            }
        }
    }
}