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

    public class ForPathWithoutSettersForSubObjects : AutoMapperSpecBase
    {
        public class Order
        {
            public CustomerHolder CustomerHolder { get; set; }
        }

        public class CustomerHolder
        {
            public Customer Customer { get; }
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
                .ForPath(o => o.CustomerHolder.Customer.Name, o => o.MapFrom(s => s.CustomerName))
                .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Total));
        });

        [Fact]
        public void Should_unflatten()
        {
            new Action(() => Mapper.Map<Order>(new OrderDto())).ShouldThrow<NullReferenceException>(ex =>
                  ex.Message.ShouldEqual("typeMapDestination.CustomerHolder.Customer cannot be null."));
        }
    }

    public class ForPathWithoutSetters : AutoMapperSpecBase
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
            public string Name { get; }
            public decimal Total { get; }
        }

        public class OrderDto
        {
            public string CustomerName { get; set; }
            public decimal Total { get; set; }
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
            model.CustomerHolder.Customer.Name.ShouldBeNull();
            model.CustomerHolder.Customer.Total.ShouldEqual(0m);
        }
    }


    public class ForPathWithPrivateSetters : AutoMapperSpecBase
    {
        public class Order
        {
            public CustomerHolder CustomerHolder { get; private set; }
        }

        public class CustomerHolder
        {
            public Customer Customer { get; private set; }
        }

        public class Customer
        {
            public string Name { get; private set; }
            public decimal Total { get; private set; }
        }

        public class OrderDto
        {
            public string CustomerName { get; set; }
            public decimal Total { get; set; }
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