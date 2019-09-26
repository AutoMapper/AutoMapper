﻿using Shouldly;
﻿using Xunit;

namespace AutoMapper.UnitTests.Projection
{
    using System.Collections.Generic;
    using System.Linq;

    using AutoMapper;
    using QueryableExtensions;

    public class ProjectIReadOnlyCollection
    {
        private MapperConfiguration _config;
        private const string Street1 = "Street1";
        private const string Street2 = "Street2";

        public ProjectIReadOnlyCollection()
        {
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Address, AddressDto>();
                cfg.CreateMap<Customer, CustomerDto>();
            });
        }

        [Fact]
        public void ProjectWithAssignedCollectionSourceProperty()
        {
            var customer = new Customer { Addresses = new List<Address> { new Address(Street1), new Address(Street2) } };
            var customers = new[] { customer }.AsQueryable();

            var mapped = customers.ProjectTo<CustomerDto>(_config).SingleOrDefault();

            mapped.ShouldNotBeNull();

            mapped.Addresses.ShouldBeOfLength(2);
            mapped.Addresses.ElementAt(0).Street.ShouldBe(Street1);
            mapped.Addresses.ElementAt(1).Street.ShouldBe(Street2);
        }

        public class Customer
        {
            public IList<Address> Addresses { get; set; }
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
            public IReadOnlyCollection<AddressDto> Addresses { get; set; }
        }

        public class AddressDto
        {
            public string Street { get; set; }
        }
    }
}