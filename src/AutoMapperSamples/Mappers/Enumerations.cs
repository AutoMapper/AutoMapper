using System;
using AutoMapper;
using Should;
using NUnit.Framework;

namespace AutoMapperSamples.Mappers
{
	namespace Enumerations
	{
		[TestFixture]
		public class SimpleExample
		{
			public enum OrderStatus : short
			{
				InProgress = 0,
				Complete = 1
			}

			public enum OrderStatusDto
			{
				InProgress = 0,
				Complete = 1
			}

			[Test]
			public void Example()
			{
				Mapper.Map<OrderStatus, OrderStatusDto>(OrderStatus.InProgress).ShouldEqual(OrderStatusDto.InProgress);
				Mapper.Map<OrderStatus, short>(OrderStatus.Complete).ShouldEqual((short)1);
				Mapper.Map<OrderStatus, string>(OrderStatus.Complete).ShouldEqual("Complete");
				Mapper.Map<short, OrderStatus>(1).ShouldEqual(OrderStatus.Complete);
				Mapper.Map<string, OrderStatus>("Complete").ShouldEqual(OrderStatus.Complete);
			}

			[Test]
			public void FlagsEnumerationExample()
			{
				var targets = Mapper.Map<AttributeTargets, AttributeTargets>(AttributeTargets.Class | AttributeTargets.Interface);

				(targets & AttributeTargets.Class).ShouldEqual(AttributeTargets.Class);
				(targets & AttributeTargets.Interface).ShouldEqual(AttributeTargets.Interface);
			}
		}
	}
}