using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper.Mappers;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.MappingInheritance
{
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


        [Fact]
        public void Mapper_Should_Allow_Overriding_Of_Destination_Type()
        {
            var order = new Order() { Customer = new Customer() { Id = 1, Name = "A" } };

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Order, OrderDTO>();
                cfg.CreateMap<Customer, CustomerDTO>();
                cfg.CreateMap<Customer, CustomerStubDTO>().As<CustomerDTO>();
            });
            var orderDto = Mapper.Map<Order, OrderDTO>(order);

            var customerDto = (CustomerDTO)orderDto.Customer;
            "A".ShouldEqual(customerDto.Name);
            1.ShouldEqual(customerDto.Id);

        }

    }
    public class DestinationTypePolymorphismTestNonGeneric
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


        [Fact]
        public void Mapper_Should_Allow_Overriding_Of_Destination_Type()
        {
            var order = new Order() { Customer = new Customer() { Id = 1, Name = "A" } };
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap(typeof(Order), typeof(OrderDTO));
                cfg.CreateMap(typeof(Customer), typeof(CustomerDTO));
                cfg.CreateMap(typeof(Customer), typeof(CustomerStubDTO)).As(typeof(CustomerDTO));
            });
            var orderDto = Mapper.Map<Order, OrderDTO>(order);

            var customerDto = (CustomerDTO)orderDto.Customer;
            "A".ShouldEqual(customerDto.Name);
            1.ShouldEqual(customerDto.Id);

        }

    }
}
