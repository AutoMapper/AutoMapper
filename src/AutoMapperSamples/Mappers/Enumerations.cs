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
                var config = new MapperConfiguration(cfg => { });
                var mapper = config.CreateMapper();
                mapper.Map<OrderStatus, OrderStatusDto>(OrderStatus.InProgress).ShouldEqual(OrderStatusDto.InProgress);
                mapper.Map<OrderStatus, short>(OrderStatus.Complete).ShouldEqual((short)1);
                mapper.Map<OrderStatus, string>(OrderStatus.Complete).ShouldEqual("Complete");
                mapper.Map<short, OrderStatus>(1).ShouldEqual(OrderStatus.Complete);
                mapper.Map<string, OrderStatus>("Complete").ShouldEqual(OrderStatus.Complete);
            }

            [Test]
            public void FlagsEnumerationExample()
            {
                var config = new MapperConfiguration(cfg => { });
                var mapper = config.CreateMapper();
                var targets = mapper.Map<AttributeTargets, AttributeTargets>(AttributeTargets.Class | AttributeTargets.Interface);

                (targets & AttributeTargets.Class).ShouldEqual(AttributeTargets.Class);
                (targets & AttributeTargets.Interface).ShouldEqual(AttributeTargets.Interface);
            }
        }
    }
}