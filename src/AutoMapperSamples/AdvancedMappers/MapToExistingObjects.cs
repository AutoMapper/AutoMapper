using AutoMapper;
using NUnit.Framework;
using Should;

namespace AutoMapperSamples
{
	namespace MapToExistingObjects
	{
		[TestFixture]
		public class ExistingDestinationObjects
		{
			public class OrderDto
			{
				public decimal Amount { get; set; }
			}

			public class Order
			{
				public int Id { get; set; }
				public decimal Amount { get; set; }
			}

			[SetUp]
			public void SetUp()
			{
				Mapper.Reset();
			}

			[Test]
			public void Example()
			{
				Mapper.Initialize(cfg =>
				{
					cfg.CreateMap<OrderDto, Order>()
						.ForMember(dest => dest.Id, opt => opt.Ignore());
				});

				Mapper.AssertConfigurationIsValid();

				var orderDto = new OrderDto {Amount = 50m};

				var order = new Order {Id = 4};

				Mapper.Map(orderDto, order);

				order.Amount.ShouldEqual(50m);
			}
		}
	}
}