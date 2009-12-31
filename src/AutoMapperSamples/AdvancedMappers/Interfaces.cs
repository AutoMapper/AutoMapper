using AutoMapper;
using NUnit.Framework;
using NBehave.Spec.NUnit;

namespace AutoMapperSamples
{
	namespace Interfaces
	{
		[TestFixture]
		public class MappingToInterfaces
		{
			public class OrderForm
			{
				public Customer Customer { get; set; }
			}

			public class Customer
			{
				public string Name { get; set; }
			}

			public interface ICreateOrderMessage
			{
				string CustomerName { get; set; }
			}

			[SetUp]
			public void SetUp()
			{
				Mapper.Reset();
			}

			[Test]
			public void Example()
			{
				Mapper.CreateMap<OrderForm, ICreateOrderMessage>();

				Mapper.AssertConfigurationIsValid();

				var order = new OrderForm
					{
						Customer = new Customer {Name = "Bob Smith"}
					};

				var message = Mapper.Map<OrderForm, ICreateOrderMessage>(order);

				message.CustomerName.ShouldEqual("Bob Smith");
			}
		}
	}
}