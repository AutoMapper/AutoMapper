using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NBehave.Spec.NUnit;
using NUnit.Framework;
using System.Linq;

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

		public class Chris_bennages_nullable_datetime_issue : AutoMapperSpecBase
		{
			private Destination _result;

			class Source
			{
				public DateTime? SomeDate { get; set; }
			}

			class Destination
			{
				public MyCustomDate SomeDate { get; set; }
			}

			class MyCustomDate
			{
				public int Day { get; set; }
				public int Month { get; set; }
				public int Year { get; set; }

				public MyCustomDate(int day, int month, int year)
				{
					Day = day;
					Month = month;
					Year = year;
				}
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Source, Destination>();
				Mapper.CreateMap<DateTime?, MyCustomDate>()
					.ConvertUsing(src => src.HasValue ? new MyCustomDate(src.Value.Day, src.Value.Month, src.Value.Year) : null);
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Source, Destination>(new Source { SomeDate = new DateTime(2005, 12, 1) });
			}

			[Test]
			public void Should_map_a_date_with_a_value()
			{
				_result.SomeDate.Day.ShouldEqual(1);
			}

			[Test]
			public void Should_map_null_to_null()
			{
				var destination = Mapper.Map<Source, Destination>(new Source());
				destination.SomeDate.ShouldBeNull();
			}
		}

		public class When_mappings_are_created_on_the_fly : NonValidatingSpecBase
		{
			private Order _order;

			private class Order
			{
				public string Name { get; set; }
				public Product Product { get; set; }
			}

			private class Product
			{
				public string ProductName { get; set; }
			}

			[Test]
			public void Should_not_use_AssignableMapper_when_mappings_are_specified_on_the_fly()
			{
				Mapper.CreateMap<Order, Order>();

				var sourceOrder = new Order()
				{
					Name = "order",
					Product = new Product() { ProductName = "product" }
				};
				var destinationOrder = new Order();
				destinationOrder = Mapper.Map(sourceOrder, destinationOrder);

				// Defining this mapping on the fly, but since the previous call to Mapper.Map()
				// had to deal with mapping Product to Product, it created an AssignableMapper
				// which will get used in place of this one.  I would expect that if I call
				// Mapper.CreateMap(), that should replace any cached value in
				// MappingEngine._objectMapperCache.
				Mapper.CreateMap<Product, Product>();

				var sourceProduct = new Product() { ProductName = "name" };
				var destinationProduct = new Product();
				destinationProduct = Mapper.Map(sourceProduct, destinationProduct);

				sourceProduct.ProductName.ShouldEqual(destinationProduct.ProductName);
				sourceProduct.ShouldNotEqual(destinationProduct);
			}
		}

		public class IssueFromSuresh : NonValidatingSpecBase
		{
			public class GetStartupDataResponse
			{
				public struct KeyValuePair<K, V>
				{
					public K Key { get; set; }
					public V Value { get; set; }
				}

				public List<GetStartupDataResponse.KeyValuePair<string, string>> Collections { get; set; }
			}

			public partial class GetStartupDataResponse1
			{
				public KeyValuePairOfStringString[] Collections
				{
					get;
					set;
				}
			}

			public partial class KeyValuePairOfStringString
			{
				public string Key { get; set; }
				public string Value { get; set; }
			}

			[Test]
			public void Test()
			{
				//var values = new[] { new KeyValuePairOfStringString { Key = "Name", Value = "Suresh" } };

				//GetStartupDataResponse1 res1 = new GetStartupDataResponse1 { Collections = values };
				//var response = new GetStartupDataResponse
				//{
				//    Collections = new List<GetStartupDataResponse.KeyValuePair<string, string>>
				//        {
				//            new GetStartupDataResponse.KeyValuePair<string, string> {Key = "234", Value = "ASdfasdf"}
				//        }
				//};

				var pair = new GetStartupDataResponse.KeyValuePair<string, string>();

				SetPair(pair, "234", "asdfsdf");

				//Mapper.CreateMap<GetStartupDataResponse1, GetStartupDataResponse>();
				//Mapper.CreateMap<KeyValuePairOfStringString, GetStartupDataResponse.KeyValuePair<string, string>>();
				//Mapper.AssertConfigurationIsValid();

				//var result = Mapper.Map<GetStartupDataResponse1, GetStartupDataResponse>(res1);
			}

			private void SetPair<K, V>(GetStartupDataResponse.KeyValuePair<K, V> pair, K key, V value)
			{
				pair.Key = key;
				pair.Value = value;
			}
		}
	}
}