using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace AutoMapper.UnitTests
{
	namespace Internationalization
	{
		public class When_mapping_a_source_with_non_english_property_names : AutoMapperSpecBase
		{
			private OrderDto _result;

			private class Order
			{
				public Customer Customer { get; set; }
			}

			private class Customer
			{
				public string Æøå { get; set; }
			}

			private class OrderDto
			{
				public string CustomerÆøå { get; set; }
			}

			protected override void Establish_context()
			{
				Mapper.CreateMap<Order, OrderDto>();
			}

			protected override void Because_of()
			{
				_result = Mapper.Map<Order, OrderDto>(new Order {Customer = new Customer {Æøå = "Bob"}});
			}

			[Test]
			public void Should_match_to_identical_property_name_on_destination()
			{
				_result.CustomerÆøå.ShouldEqual("Bob");
			}
		}

	}
}