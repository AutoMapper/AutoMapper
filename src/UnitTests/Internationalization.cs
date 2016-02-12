using Xunit;
using Should;

namespace AutoMapper.UnitTests
{
	namespace Internationalization
	{
		public class When_mapping_a_source_with_non_english_property_names : AutoMapperSpecBase
		{
			private OrderDto _result;

			public class Order
			{
				public Customer Customer { get; set; }
			}

			public class Customer
			{
				public string ��� { get; set; }
			}

			public class OrderDto
			{
				public string Customer��� { get; set; }
			}

		    protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
		    {
		        cfg.CreateMap<Order, OrderDto>();
		    });

			protected override void Because_of()
			{
				_result = Mapper.Map<Order, OrderDto>(new Order {Customer = new Customer {��� = "Bob"}});
			}

			[Fact]
			public void Should_match_to_identical_property_name_on_destination()
			{
				_result.Customer���.ShouldEqual("Bob");
			}
		}

	}
}