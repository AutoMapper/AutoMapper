﻿using Should;
﻿using Xunit;

namespace AutoMapper.UnitTests.Projection
{
	using System.Collections.Generic;
	using System.Linq;

	using AutoMapper;
	using QueryableExtensions;

    public class ProjectCollectionEnumerableTest
	{
		private const string Street1 = "Street1";
		private const string Street2 = "Street2";

        public ProjectCollectionEnumerableTest()
		{
			Mapper.CreateMap<Address, AddressDto>();
			Mapper.CreateMap<Customer, CustomerDto>();
		}

		[Fact(Skip = "EF doesn't support null assignments in expressions")]
		public void ProjectWithNullCollectionSourceProperty()
		{
			var customers = new[] { new Customer() }.AsQueryable();

			var mapped = customers.ProjectTo<CustomerDto>().SingleOrDefault();

            mapped.ShouldNotBeNull();
            mapped.Addresses.ShouldBeNull();
        }

		[Fact]
		public void ProjectWithAssignedCollectionSourceProperty()
		{
			var customer = new Customer { Addresses = new List<Address> { new Address(Street1), new Address(Street2) } };
			var customers = new[] { customer }.AsQueryable();

			var mapped = customers.ProjectTo<CustomerDto>().SingleOrDefault();

			mapped.ShouldNotBeNull();

            mapped.Addresses.ShouldBeOfLength(2);
            mapped.Addresses.ElementAt(0).Street.ShouldEqual(Street1);
            mapped.Addresses.ElementAt(1).Street.ShouldEqual(Street2);
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
			public IEnumerable<AddressDto> Addresses { get; set; }
		}

		public class AddressDto
		{
			public string Street { get; set; }

			public override string ToString()
			{
				return Street;
			}

			public override bool Equals(object obj)
			{
				return string.Equals(ToString(), obj.ToString());
			}

		    public override int GetHashCode()
		    {
                return Street.GetHashCode();
		    }
		}
	}
}
