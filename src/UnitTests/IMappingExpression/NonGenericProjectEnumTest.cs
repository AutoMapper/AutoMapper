﻿using System;

namespace AutoMapper.UnitTests.Projection
{
    using QueryableExtensions;
    using Shouldly;
    using System.Linq;
    using Xunit;

    public class NonGenericProjectEnumTest
    {
        private MapperConfiguration _config;

        public NonGenericProjectEnumTest()
        {
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap(typeof(Customer), typeof(CustomerDto));
                cfg.CreateMap(typeof(CustomerType), typeof(string)).ConvertUsing(ct => ct.ToString().ToUpper());
            });
        }

        [Fact]
        public void ProjectingEnumToString()
        {
            var customers = new[] { new Customer() { FirstName = "Bill", LastName = "White", CustomerType = CustomerType.Vip } }.AsQueryable();

            var projected = customers.ProjectTo<CustomerDto>(_config);
            projected.ShouldNotBeNull();
            customers.Single().CustomerType.ToString().ToUpper().ShouldBe(projected.Single().CustomerType);
        }

        public class Customer
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public CustomerType CustomerType { get; set; }
        }

        public class CustomerDto
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string CustomerType { get; set; }
        }

        public enum CustomerType
        {
            Regular,
            Vip,
        }
    }

    public class NonGenericProjectAndMapEnumTest
    {
        private IMapper _mapper;

        public NonGenericProjectAndMapEnumTest()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap(typeof (Customer), typeof (CustomerDto));
                cfg.CreateMap(typeof (CustomerType), typeof (string)).ConvertUsing(ct => ct.ToString().ToUpper());
            });
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void ProjectingEnumToString()
        {
            var customers = new[] { new Customer() { FirstName = "Bill", LastName = "White", CustomerType = CustomerType.Vip } }.AsQueryable();

            var projected = customers.ProjectTo<CustomerDto>(_mapper.ConfigurationProvider);
            projected.ShouldNotBeNull();
            Assert.Equal(customers.Single().CustomerType.ToString(), projected.Single().CustomerType, StringComparer.OrdinalIgnoreCase);
        }

        public class Customer
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public CustomerType CustomerType { get; set; }
        }

        public class CustomerDto
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string CustomerType { get; set; }
        }

        public enum CustomerType
        {
            Regular,
            Vip,
        }
    }

    public class NonGenericProjectionOverrides : NonValidatingSpecBase
    {
        public class Source
        {
            
        }

        public class Dest
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof (Source), typeof (Dest)).ConvertUsing(src => new Dest {Value = 10});
        });

        [Fact]
        public void Should_validate_because_of_overridden_projection()
        {
            typeof(AutoMapperConfigurationException).ShouldNotBeThrownBy(Configuration.AssertConfigurationIsValid);
        }
    }
}
