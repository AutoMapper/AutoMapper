using AutoMapper;
using NUnit.Framework;
using Should;

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

            [Test]
            public void Example()
            {
                var mapperConfig = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<OrderForm, ICreateOrderMessage>();
                });

                mapperConfig.AssertConfigurationIsValid();

                var mapper = mapperConfig.CreateMapper();

                var order = new OrderForm
                    {
                        Customer = new Customer {Name = "Bob Smith"}
                    };

                var message = mapper.Map<OrderForm, ICreateOrderMessage>(order);

                message.CustomerName.ShouldEqual("Bob Smith");
            }
        }
    }
}