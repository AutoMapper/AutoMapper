using AutoMapper;
using NUnit.Framework;
using StructureMap;

namespace AutoMapperSamples.Configuration
{
	namespace Initialization
	{
		[TestFixture]
		public class InitializationExample
		{
			public class Order
			{
				public decimal Amount { get; set; }
			}

			public class OrderListViewModel
			{
				public string Amount { get; set; }
			}

			public class OrderEditViewModel
			{
				public string Amount { get; set; }
			}

			[Test]
			public void Example()
			{
				Mapper.Initialize(cfg =>
				{
					cfg.ConstructServicesUsing(ObjectFactory.GetInstance);
				});
			}
		}
	}
}