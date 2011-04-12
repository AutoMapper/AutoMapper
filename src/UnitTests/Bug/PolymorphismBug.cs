using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using AutoMapper;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class PolymorphismBug
    {

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class CustomerStubDTO
        {
            public int Id { get; set; }
        }

        public class CustomerDTO : CustomerStubDTO
        {
            public string Name { get; set; }
        }

        public class Order
        {
            public Customer Customer { get; set; }
        }

        public class OrderDTO
        {
            public CustomerStubDTO Customer { get; set; }
        }
        [Test]
        public void Can_Map_Polymorphic_Classes()
        {
            Mapper.CreateMap<Customer, CustomerStubDTO>();
            Mapper.CreateMap<Order, OrderDTO>();

            Customer customer = new Customer{Id=1,Name="John"};
            Order order = new Order{Customer = customer};

            var orderDto = Mapper.Map<Order, OrderDTO>(order);
            Assert.AreEqual(orderDto.Customer.Id,order.Customer.Id);
        }
    }
}
