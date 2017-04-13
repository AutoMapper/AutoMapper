using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class ForPath : AutoMapperSpecBase
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

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<OrderDto, Order>(MemberList.None).ForPath(o=>o.Customer.Name, o=>o.MapFrom(s=>s.CustomerName));
        });

        [Fact]
        public void Should_unflatten()
        {
            var dto = new OrderDto { CustomerName = "George Costanza", Total = 74.85m };
            var model = Mapper.Map<Order>(dto);
            model.Customer.Name.ShouldEqual("George Costanza");
        }
    }
}