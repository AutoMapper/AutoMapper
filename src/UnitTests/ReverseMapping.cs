using Xunit;
using Shouldly;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Reflection;

namespace AutoMapper.UnitTests
{
    public class InvalidReverseMap : NonValidatingSpecBase
    {
        public class One
        {
            public string Name { get; set; }
            public Three2 Three2 { get; set; }
        }

        public class Two
        {
            public string Name { get; set; }
            public Three Three { get; set; }
        }

        public class Three
        {
        }

        public class Three2
        {
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=>
        {
            cfg.CreateMissingTypeMaps = false;
            cfg.CreateMap<One, Two>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s))
                .ForMember(d => d.Three, o => o.MapFrom(s => s.Three2))
                .ReverseMap();
            cfg.CreateMap<Three, Three2>();
        });

        [Fact]
        public void Should_report_the_error()
        {
            new Action(() => Configuration.AssertConfigurationIsValid())
                .ShouldThrowException<AutoMapperConfigurationException>(ex =>
                {
                    ex.PropertyMap.DestinationProperty.ShouldBe(typeof(Two).GetProperty("Three"));
                    ex.Types.ShouldBe(new TypePair(typeof(One), typeof(Two)));
                });
        }
    }

    public class MapFromReverseResolveUsing : AutoMapperSpecBase
    {
        public class Source
        {
            public int Total { get; set; }
        }

        public class Destination
        {
            public int Total { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(c =>
        {
            c.CreateMap<Destination, Source>()
             .ForMember(dest => dest.Total, opt => opt.MapFrom(x => x.Total))
             .ReverseMap()
             .ForMember(dest => dest.Total, opt => opt.ResolveUsing<CustomResolver>());
        });

        public class CustomResolver : IValueResolver<Source, Destination, int>
        {
            public int Resolve(Source source, Destination destination, int member, ResolutionContext context)
            {
                return Int32.MaxValue;
            }
        }

        [Fact]
        public void Should_use_the_resolver()
        {
            Mapper.Map<Destination>(new Source()).Total.ShouldBe(int.MaxValue);
        }
    }

    public class MethodsWithReverse : AutoMapperSpecBase
    {
        class Order
        {
            public OrderItem[] OrderItems { get; set; }
        }

        class OrderItem
        {
            public string Product { get; set; }
        }

        class OrderDto
        {
            public int OrderItemsCount { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(c=>
        {
            c.CreateMap<Order, OrderDto>().ReverseMap();
        });

        [Fact]
        public void ShouldMapOk()
        {
            Mapper.Map<Order>(new OrderDto());
        }
    }

    public class ReverseForPath : AutoMapperSpecBase
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
                .ForPath(o => o.CustomerHolder.Customer.Name, o => o.MapFrom(s => s.CustomerName))
                .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Total))
                .ReverseMap();
        });

        [Fact]
        public void Should_flatten()
        {
            var model = new Order {
                CustomerHolder = new CustomerHolder {
                        Customer = new Customer { Name = "George Costanza", Total = 74.85m }
                    }
            };
            var dto = Mapper.Map<OrderDto>(model);
            dto.CustomerName.ShouldBe("George Costanza");
            dto.Total.ShouldBe(74.85m);
        }
    }

    public class ReverseMapFrom : AutoMapperSpecBase
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
            cfg.CreateMap<Order, OrderDto>()
                .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.CustomerHolder.Customer.Name))
                .ForMember(d => d.Total, o => o.MapFrom(s => s.CustomerHolder.Customer.Total))
                .ReverseMap();
        });

        [Fact]
        public void Should_unflatten()
        {
            var dto = new OrderDto { CustomerName = "George Costanza", Total = 74.85m };
            var model = Mapper.Map<Order>(dto);
            model.CustomerHolder.Customer.Name.ShouldBe("George Costanza");
            model.CustomerHolder.Customer.Total.ShouldBe(74.85m);
        }
    }

    public class ReverseDefaultFlatteningWithIgnoreMember : AutoMapperSpecBase
    {
        public class Order
        {
            public CustomerHolder Customerholder { get; set; }
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
            public string CustomerholderCustomerName { get; set; }
            public decimal CustomerholderCustomerTotal { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ReverseMap()
                .ForMember(d=>d.Customerholder, o=>o.Ignore())
                .ForPath(d=>d.Customerholder.Customer.Total, o=>o.MapFrom(s=>s.CustomerholderCustomerTotal));
        });

        [Fact]
        public void Should_unflatten()
        {
            var dto = new OrderDto { CustomerholderCustomerName = "George Costanza", CustomerholderCustomerTotal = 74.85m };
            var model = Mapper.Map<Order>(dto);
            model.Customerholder.Customer.Name.ShouldBeNull();
            model.Customerholder.Customer.Total.ShouldBe(74.85m);
        }
    }

    public class ReverseDefaultFlattening : AutoMapperSpecBase
    {
        public class Order
        {
            public CustomerHolder Customerholder { get; set; }
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
            public string CustomerholderCustomerName { get; set; }
            public decimal CustomerholderCustomerTotal { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ReverseMap();
        });

        [Fact]
        public void Should_unflatten()
        {
            var dto = new OrderDto { CustomerholderCustomerName = "George Costanza", CustomerholderCustomerTotal = 74.85m };
            var model = Mapper.Map<Order>(dto);
            model.Customerholder.Customer.Name.ShouldBe("George Costanza");
            model.Customerholder.Customer.Total.ShouldBe(74.85m);
        }
    }

    public class ReverseMapConventions : AutoMapperSpecBase
    {
        Rotator_Ad_Run _destination;
        DateTime _startDate = DateTime.Now, _endDate = DateTime.Now.AddHours(2);

        public class Rotator_Ad_Run
        {
            public DateTime Start_Date { get; set; }
            public DateTime End_Date { get; set; }
            public bool Enabled { get; set; }
        }

        public class RotatorAdRunViewModel
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool Enabled { get; set; }
        }

        public class UnderscoreNamingConvention : INamingConvention
        {
            public Regex SplittingExpression { get; } = new Regex(@"\p{Lu}[a-z0-9]*(?=_?)");

            public string SeparatorCharacter => "_";
            public string ReplaceValue(Match match)
            {
                return match.Value;
            }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateProfile("MyMapperProfile", prf =>
            {
                prf.SourceMemberNamingConvention = new UnderscoreNamingConvention();
                prf.CreateMap<Rotator_Ad_Run, RotatorAdRunViewModel>();
            });
            cfg.CreateProfile("MyMapperProfile2", prf =>
            {
                prf.DestinationMemberNamingConvention = new UnderscoreNamingConvention();
                prf.CreateMap<RotatorAdRunViewModel, Rotator_Ad_Run>();
            });
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<RotatorAdRunViewModel, Rotator_Ad_Run>(new RotatorAdRunViewModel { Enabled = true, EndDate = _endDate, StartDate = _startDate });
        }

        [Fact]
        public void Should_apply_the_convention_in_reverse()
        {
            _destination.Enabled.ShouldBeTrue();
            _destination.End_Date.ShouldBe(_endDate);
            _destination.Start_Date.ShouldBe(_startDate);
        }
    }

    public class When_reverse_mapping_classes_with_simple_properties : AutoMapperSpecBase
    {
        private Source _source;

        public class Source
        {
            public int Value { get; set; }
        }
        public class Destination
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ReverseMap();
        });

        protected override void Because_of()
        {
            var dest = new Destination
            {
                Value = 10
            };
            _source = Mapper.Map<Destination, Source>(dest);
        }

        [Fact]
        public void Should_create_a_map_with_the_reverse_items()
        {
            _source.Value.ShouldBe(10);
        }
    }

    public class When_validating_only_against_source_members_and_source_matches : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }
        public class Destination
        {
            public int Value { get; set; }
            public int Value2 { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>(MemberList.Source);
        });

        [Fact]
        public void Should_only_map_source_members()
        {
            var typeMap = ConfigProvider.FindTypeMapFor<Source, Destination>();

            typeMap.GetPropertyMaps().Count().ShouldBe(1);
        }

        [Fact]
        public void Should_not_throw_any_configuration_validation_errors()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
        }
    }

    public class When_validating_only_against_source_members_and_source_does_not_match : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
            public int Value2 { get; set; }
        }
        public class Destination
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>(MemberList.Source);
        });

        [Fact]
        public void Should_throw_a_configuration_validation_error()
        {
            typeof(AutoMapperConfigurationException).ShouldBeThrownBy(Configuration.AssertConfigurationIsValid);
        }
    }

    public class When_validating_only_against_source_members_and_unmatching_source_members_are_manually_mapped : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
            public int Value2 { get; set; }
        }
        public class Destination
        {
            public int Value { get; set; }
            public int Value3 { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>(MemberList.Source)
                .ForMember(dest => dest.Value3, opt => opt.MapFrom(src => src.Value2));
        });

        [Fact]
        public void Should_not_throw_a_configuration_validation_error()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
        }
    }

    public class When_validating_only_against_source_members_and_unmatching_source_members_are_manually_mapped_with_resolvers : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
            public int Value2 { get; set; }
        }
        public class Destination
        {
            public int Value { get; set; }
            public int Value3 { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>(MemberList.Source)
                .ForMember(dest => dest.Value3, opt => opt.ResolveUsing(src => src.Value2))
                .ForSourceMember(src => src.Value2, opt => opt.Ignore());
        });

        [Fact]
        public void Should_not_throw_a_configuration_validation_error()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
        }
    }

    public class When_reverse_mapping_and_ignoring_via_method : NonValidatingSpecBase
    {
        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
            public int Ignored { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dest>()
                .ForMember(d => d.Ignored, opt => opt.Ignore())
                .ReverseMap();
        });

        [Fact]
        public void Should_show_valid()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(() => Configuration.AssertConfigurationIsValid());
        }
    }

    public class When_reverse_mapping_and_ignoring : SpecBase
    {
        public class Foo
        {
            public string Bar { get; set; }
            public string Baz { get; set; }
        }

        public class Foo2
        {
            public string Bar { get; set; }
            public string Boo { get; set; }
        }

        [Fact]
        public void GetUnmappedPropertyNames_ShouldReturnBoo()
        {
            //Arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Foo, Foo2>();
            });
            var typeMap = config.GetAllTypeMaps()
                      .First(x => x.SourceType == typeof(Foo) && x.DestinationType == typeof(Foo2));
            //Act
            var unmappedPropertyNames = typeMap.GetUnmappedPropertyNames();
            //Assert
            unmappedPropertyNames[0].ShouldBe("Boo");
        }

        [Fact]
        public void WhenSecondCallTo_GetUnmappedPropertyNames_ShouldReturnBoo()
        {
            //Arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Foo, Foo2>().ReverseMap();
            });
            var typeMap = config.GetAllTypeMaps()
                      .First(x => x.SourceType == typeof(Foo2) && x.DestinationType == typeof(Foo));
            //Act
            var unmappedPropertyNames = typeMap.GetUnmappedPropertyNames();
            //Assert
            unmappedPropertyNames[0].ShouldBe("Boo");
        }
    }

    public class When_reverse_mapping_open_generics : AutoMapperSpecBase
    {
        private Source<int> _source;

        public class Source<T>
        {
            public T Value { get; set; }
        }
        public class Destination<T>
        {
            public T Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(Source<>), typeof(Destination<>))
                .ReverseMap();
        });

        protected override void Because_of()
        {
            var dest = new Destination<int>
            {
                Value = 10
            };
            _source = Mapper.Map<Destination<int>, Source<int>>(dest);
        }

        [Fact]
        public void Should_create_a_map_with_the_reverse_items()
        {
            _source.Value.ShouldBe(10);
        }
    }
}