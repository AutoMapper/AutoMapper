using System;
using AutoMapper;
using Shouldly;
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
                var config = new MapperConfiguration(cfg => { });
                var mapper = config.CreateMapper();
                mapper.Map<OrderStatus, OrderStatusDto>(OrderStatus.InProgress).ShouldBe(OrderStatusDto.InProgress);
                mapper.Map<OrderStatus, short>(OrderStatus.Complete).ShouldBe((short)1);
                mapper.Map<OrderStatus, string>(OrderStatus.Complete).ShouldBe("Complete");
                mapper.Map<short, OrderStatus>(1).ShouldBe(OrderStatus.Complete);
                mapper.Map<string, OrderStatus>("Complete").ShouldBe(OrderStatus.Complete);
            }

            [Test]
            public void FlagsEnumerationExample()
            {
                var config = new MapperConfiguration(cfg => { });
                var mapper = config.CreateMapper();
                var targets = mapper.Map<AttributeTargets, AttributeTargets>(AttributeTargets.Class | AttributeTargets.Interface);

                (targets & AttributeTargets.Class).ShouldBe(AttributeTargets.Class);
                (targets & AttributeTargets.Interface).ShouldBe(AttributeTargets.Interface);
            }
        }
    }
}