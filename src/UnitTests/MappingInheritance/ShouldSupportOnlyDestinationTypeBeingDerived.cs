using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper.Mappers;
using NUnit.Framework;

namespace AutoMapper.UnitTests.MappingInheritance
{
    [TestFixture]
    public class DestinationTypePolymorphismTest
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
        public void Mapper_Should_Allow_Overriding_Of_Destination_Type()
        {
            var order = new Order() { Customer = new Customer() { Id = 1, Name = "A" } };
            Mapper.CreateMap<Order, OrderDTO>();
            Mapper.CreateMap<Customer, CustomerDTO>();
            Mapper.CreateMap<Customer, CustomerStubDTO>().As<CustomerDTO>();
            var orderDto = Mapper.Map<Order, OrderDTO>(order);

            var customerDto = (CustomerDTO)orderDto.Customer;
            Assert.AreEqual("A", customerDto.Name);
            Assert.AreEqual(1, customerDto.Id);

        }

    }
}
