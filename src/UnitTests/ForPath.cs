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
            public CustomerHolder CustomerHolder { get; set; }
        }

        public class CustomerHolder
        {
            public Customer Customer { get; set; }
        }

        public class Customer
        {
            public string Name { get; set; }
            public decimal Total { get; set; }
        }

        public class OrderDto
        {
            public string CustomerName { get; set; }
            public decimal Total { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<OrderDto, Order>()
                .ForPath(o=>o.CustomerHolder.Customer.Name, o=>o.MapFrom(s=>s.CustomerName))
                .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Total));
        });

        [Fact]
        public void Should_unflatten()
        {
            var dto = new OrderDto { CustomerName = "George Costanza", Total = 74.85m };
            var model = Mapper.Map<Order>(dto);
            model.CustomerHolder.Customer.Name.ShouldEqual("George Costanza");
            model.CustomerHolder.Customer.Total.ShouldEqual(74.85m);
        }
    }

    public class ForPathWithValueTypes : AutoMapperSpecBase
    {
        public struct Order
        {
            public CustomerHolder CustomerHolder;
        }

        public struct CustomerHolder
        {
            public Customer Customer;
        }

        public struct Customer
        {
            public string Name;
            public decimal Total;
        }

        public struct OrderDto
        {
            public string CustomerName;
            public decimal Total;
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<OrderDto, Order>()
                .ForPath(o => o.CustomerHolder.Customer.Name, o => o.MapFrom(s => s.CustomerName))
                .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Total));
        });

        [Fact]
        public void Should_unflatten()
        {
            var dto = new OrderDto { CustomerName = "George Costanza", Total = 74.85m };
            var model = Mapper.Map<Order>(dto);
            model.CustomerHolder.Customer.Name.ShouldEqual("George Costanza");
            model.CustomerHolder.Customer.Total.ShouldEqual(74.85m);
        }
    }
}