using AutoMapper.XpressionMapper.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    using Configuration.Internal;

    public class XpressionMapperForPathTests
    {
        public XpressionMapperForPathTests()
        {
            SetupAutoMapper();
            SetupQueryableCollection();
        }

        #region Tests
        [Fact]
        public void Works_for_inherited_properties()
        {
            //Arrange
            Expression<Func<DerivedModel, bool>> selection = s => s.Nested.NestedTitle2 == "nested test";

            //Act
            Expression<Func<DerivedDataModel, bool>> selectionMapped = mapper.Map<Expression<Func<DerivedDataModel, bool>>>(selection);
            List<DerivedDataModel> items = DataObjects.Where(selectionMapped).ToList();

            //Assert
            Assert.True(items.Count == 1);
        }

        [Fact]
        public void Works_for_top_level_string_member()
        {
            //Arrange
            Expression<Func<Order, bool>> selection = s => s.CustomerHolder.Customer.Name == "Jerry Springer";

            //Act
            Expression<Func<OrderDto, bool>> selectionMapped = mapper.Map<Expression<Func<OrderDto, bool>>>(selection);
            List<OrderDto> items = Orders.Where(selectionMapped).ToList();

            //Assert
            Assert.True(items.Count == 1);
        }

        [Fact]
        public void Works_for_top_level_value_type()
        {
            //Arrange
            Expression<Func<Order, bool>> selection = s => s.CustomerHolder.Customer.Age == 32;

            //Act
            Expression<Func<OrderDto, bool>> selectionMapped = mapper.Map<Expression<Func<OrderDto, bool>>>(selection);
            List<OrderDto> items = Orders.Where(selectionMapped).ToList();

            //Assert
            Assert.True(items.Count == 1);
        }

        [Fact]
        public void Maps_top_level_string_member_as_include()
        {
            //Arrange
            Expression<Func<Order, object>> selection = s => s.CustomerHolder.Customer.Name;

            //Act
            Expression<Func<OrderDto, object>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<OrderDto, object>>>(selection);
            List<object> orders = Orders.Select(selectionMapped).ToList();

            //Assert
            Assert.True(orders.Count == 2);
        }

        [Fact]
        public void Maps_top_level_value_type_as_include()
        {
            //Arrange
            Expression<Func<Order, object>> selection = s => s.CustomerHolder.Customer.Total;

            //Act
            Expression<Func<OrderDto, object>> selectionMapped = mapper.MapExpressionAsInclude<Expression<Func<OrderDto, object>>>(selection);
            List<object> orders = Orders.Select(selectionMapped).ToList();

            //Assert
            Assert.True(orders.Count == 2);
        }

        [Fact]
        public void Throws_exception_when_mapped_value_type_is_a_child_of_the_parameter()
        {
            //Arrange
            Expression<Func<Order, object>> selection = s => s.CustomerHolder.Customer.Age;

            //Assert
            Assert.Throws<InvalidOperationException>(() => mapper.MapExpressionAsInclude<Expression<Func<OrderDto, object>>>(selection));
        }

        [Fact]
        public void Throws_exception_when_mapped_string_is_a_child_of_the_parameter()
        {
            //Arrange
            Expression<Func<Order, object>> selection = s => s.CustomerHolder.Customer.Address;

            //Assert
            Assert.Throws<InvalidOperationException>(() => mapper.MapExpressionAsInclude<Expression<Func<OrderDto, object>>>(selection));
        }
        #endregion Tests

        private void SetupQueryableCollection()
        {
            DataObjects = new DerivedDataModel[]
            {
                new DerivedDataModel() { OtherID = 2, Title2 = "nested test", ID = 1, Title = "test", DescendantField = "descendant field" },
                new DerivedDataModel() { OtherID = 3, Title2 = "nested", ID = 4, Title = "title", DescendantField = "some text" }
            }.AsQueryable<DerivedDataModel>();

            Orders = new OrderDto[]
            {
                new OrderDto
                {
                     Customer = new CustomerDto{ Name = "George Costanza", Total = 7 },
                     CustomerAddress = "333 First Ave",
                     CustomerAge = 32
                },
                new OrderDto
                {
                     Customer = new CustomerDto{ Name = "Jerry Springer", Total = 8 },
                     CustomerAddress = "444 First Ave",
                     CustomerAge = 31
                }
            }.AsQueryable<OrderDto>();
        }

        private static IQueryable<OrderDto> Orders { get; set; }
        private static IQueryable<DerivedDataModel> DataObjects { get; set; }

        private void SetupAutoMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfiles(typeof(ForPathCustomerProfile));
            });

            mapper = config.CreateMapper();
        }

        static IMapper mapper;
    }

    public class RootModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public NestedModel Nested { get; set; }
    }

    public class NestedModel
    {
        public int NestedID { get; set; }
        public string NestedTitle { get; set; }
        public string NestedTitle2 { get; set; }
    }

    public class DerivedModel : RootModel
    {
        public string DescendantField { get; set; }
    }

    public class DataModel
    {
        public int ID { get; set; }
        public string Title { get; set; }

        public int OtherID { get; set; }
        public string Title2 { get; set; }
    }

    public class DerivedDataModel : DataModel
    {
        public string DescendantField { get; set; }
    }

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
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal? Total { get; set; }
        public decimal? Age { get; set; }
    }

    public class CustomerDto
    {
        public string Name { get; set; }
        public decimal? Total { get; set; }
    }

    public class OrderDto
    {
        public string CustomerAddress { get; set; }
        public decimal? CustomerAge { get; set; }
        public CustomerDto Customer { get; set; }
    }

    public class ForPathCustomerProfile : Profile
    {
        public ForPathCustomerProfile()
        {
            CreateMap<DerivedDataModel, DerivedModel>()
                .ForPath(d => d.Nested.NestedTitle, opt => opt.MapFrom(src => src.Title))
                .ForPath(d => d.Nested.NestedTitle2, opt => opt.MapFrom(src => src.Title2));

            CreateMap<OrderDto, Order>()
                .ForPath(o => o.CustomerHolder.Customer.Name, o => o.MapFrom(s => s.Customer.Name))
                .ForPath(o => o.CustomerHolder.Customer.Total, o => o.MapFrom(s => s.Customer.Total))
                .ForPath(o => o.CustomerHolder.Customer.Address, o => o.MapFrom(s => s.CustomerAddress))
                .ForPath(o => o.CustomerHolder.Customer.Age, o => o.MapFrom(s => s.CustomerAge));
        }
    }
}
