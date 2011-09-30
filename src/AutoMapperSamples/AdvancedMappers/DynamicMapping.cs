using AutoMapper;
using Should;
using NUnit.Framework;

namespace AutoMapperSamples
{
	namespace DynamicMapping
	{
		[TestFixture]
		public class MappingToInterfaces
		{
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
				var order = new {CustomerName = "Bob Smith"};

				var message = Mapper.DynamicMap<ICreateOrderMessage>(order);

				message.CustomerName.ShouldEqual("Bob Smith");
			}
		}
	}
}