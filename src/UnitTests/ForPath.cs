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
                  ex.Message.ShouldEqual("typeMapDestination.CustomerHolder.Customer cannot be null because it's used by ForPath."));
        }
    }

    public class ForPathWithoutSettersShouldBehaveAsForMember : AutoMapperSpecBase
    {
        public class Order
        {
            public CustomerHolder CustomerHolder { get; set; }
            public int Value { get; }
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
                .ForMember(o=>o.Value, o=>o.UseValue(9))
                .ForPath(o => o.CustomerHolder.Customer.Name, o => o.MapFrom(s => s.CustomerName))
                .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Total));
        });

        [Fact]
        public void Should_unflatten()
        {
            var dto = new OrderDto { CustomerName = "George Costanza", Total = 74.85m };
            var model = Mapper.Map<Order>(dto);
            model.Value.ShouldEqual(0);
            model.CustomerHolder.Customer.Name.ShouldBeNull();
            model.CustomerHolder.Customer.Total.ShouldEqual(0m);
        }
    }

    public class ForPathWithIgnoreShouldNotSetValue : AutoMapperSpecBase
    {
        public partial class TimesheetModel
        {
            public int ID { get; set; }
            public DateTime? StartDate { get; set; }
            public int? Contact { get; set; }
            public ContactModel ContactNavigation { get; set; }
        }

        public class TimesheetViewModel
        {
            public int? Contact { get; set; }
            public DateTime? StartDate { get; set; }
        }

        public class ContactModel
        {
            public int Id { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TimesheetModel, TimesheetViewModel>()
                .ForMember(d => d.Contact, o => o.MapFrom(s => s.ContactNavigation.Id))
                .ReverseMap()
                .ForPath(s => s.ContactNavigation.Id, opt => opt.Ignore());
        });

        [Fact]
        public void Should_not_set_value()
        {
            var source = new TimesheetModel
            {
                Contact = 6,
                ContactNavigation = new ContactModel
                {
                    Id = 5
                }
            };
            var dest = new TimesheetViewModel
            {
                Contact = 10
            };
            Mapper.Map(dest, source);

            source.ContactNavigation.Id.ShouldEqual(5);
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

    public class ForPathWithValueTypesAndFields : AutoMapperSpecBase
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