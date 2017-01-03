﻿using Xunit;
﻿using Should;

namespace AutoMapper.UnitTests.Projection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using AutoMapper;
    using QueryableExtensions;

    public class ProjectWithFields : AutoMapperSpecBase
    {
        public class Foo
        {
            public int A;
        }

        public class FooDto
        {
            public int A;
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Foo, FooDto>();
        });

        [Fact]
        public void Should_work()
        {
            new[] { new Foo() }.AsQueryable().ProjectTo<FooDto>(Configuration).Single().A.ShouldEqual(0);
        } 
    }

    public class ProjectTest
    {
        private MapperConfiguration _config;

        public ProjectTest()
        {
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<Customer, CustomerDto>();
            });
        }

        [Fact]
        public void ProjectToWithUnmappedTypeShouldThrowException()
        {
            var customers =
                new[] { new Customer { FirstName = "Bill", LastName = "White", Address = new Address("Street1") } }
                    .AsQueryable();

            IList<Unmapped> projected = null;

            typeof(InvalidOperationException).ShouldBeThrownBy(() => projected = customers.ProjectTo<Unmapped>(_config).ToList());

            projected.ShouldBeNull();
        }

        public class Customer
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public Address Address { get; set; }
        }

        public class Address
        {
            public Address(string street)
            {
                Street = street;
            }

            public string Street { get; set; }
        }

        public class CustomerDto
        {
            public string FirstName { get; set; }

            public AddressDto Address { get; set; }
        }

        public class AddressDto
        {
            public string Street { get; set; }
        }

        public class Unmapped
        {
            public string FirstName { get; set; }
        }
    }
}
