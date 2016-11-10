using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using NUnit.Framework;
using Should;

namespace AutoMapperSamples
{
    namespace Flattening
    {
        public class Order
        {
            private readonly IList<OrderLineItem> _orderLineItems = new List<OrderLineItem>();

            public Customer Customer { get; set; }

            public OrderLineItem[] GetOrderLineItems()
            {
                return _orderLineItems.ToArray();
            }

            public void AddOrderLineItem(Product product, int quantity)
            {
                _orderLineItems.Add(new OrderLineItem(product, quantity));
            }

            public decimal GetTotal()
            {
                return _orderLineItems.Sum(li => li.GetTotal());
            }
        }

        public class Product
        {
            public decimal Price { get; set; }
            public string Name { get; set; }
        }

        public class OrderLineItem
        {
            public OrderLineItem(Product product, int quantity)
            {
                Product = product;
                Quantity = quantity;
            }

            public Product Product { get; private set; }
            public int Quantity { get; private set; }

            public decimal GetTotal()
            {
                return Quantity * Product.Price;
            }
        }

        public class Customer
        {
            public string Name { get; set; }
        }

        public class OrderDto
        {
            public string CustomerName { get; set; }
            public decimal Total { get; set; }
        }

        [TestFixture]
        public class Flattening
        {
            [Test]
            public void Example()
            {
                // Complex model
                var customer = new Customer
                    {
                        Name = "George Costanza"
                    };
                var order = new Order
                    {
                        Customer = customer
                    };
                var bosco = new Product
                    {
                        Name = "Bosco",
                        Price = 4.99m
                    };
                order.AddOrderLineItem(bosco, 15);

                // Configure AutoMapper
                var config = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDto>());

                // Perform mapping
                var mapper = config.CreateMapper();
                OrderDto dto = mapper.Map<Order, OrderDto>(order);

                dto.CustomerName.ShouldEqual("George Costanza");
                dto.Total.ShouldEqual(74.85m);
            }
        }
    }

}