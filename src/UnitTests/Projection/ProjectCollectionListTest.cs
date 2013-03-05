namespace AutoMapper.UnitTests.Projection
{
	using System.Collections.Generic;
	using System.Linq;

	using AutoMapper;
	using AutoMapper.QueryableExtensions;

	using NUnit.Framework;

	using SharpTestsEx;

	[TestFixture]
	public class ProjectCollectionListTest
	{
		private const string Street1 = "Street1";
		private const string Street2 = "Street2";

		[SetUp]
		public void SetUp()
		{
			Mapper.CreateMap<Address, AddressDto>();
			Mapper.CreateMap<Customer, CustomerDto>();
		}

		[Test]
		public void ProjectWithNullCollectionSourceProperty()
		{
			var customers = new[] { new Customer() }.AsQueryable();

			var mapped = customers.Project().To<CustomerDto>().SingleOrDefault();
			mapped.Should().Not.Be.Null();
			mapped.Addresses.Should().Be.Null();
		}

		[Test]
		public void ProjectWithAssignedCollectionSourceProperty()
		{
			var customer = new Customer { Addresses = new List<Address> { new Address(Street1), new Address(Street2) } };

			var customers = new[] { customer }.AsQueryable();

			var mapped = customers.Project().To<CustomerDto>().SingleOrDefault();

			mapped.Should().Not.Be.Null();

			var addresses = new[] { new AddressDto { Street = Street1 }, new AddressDto { Street = Street2 } };
			mapped.Addresses.Should().Have.SameSequenceAs(addresses);
		}

		private class Customer
		{
			public IList<Address> Addresses { get; set; }
		}

		private class Address
		{
			public Address(string street)
			{
				Street = street;
			}

			public string Street { get; set; }
		}

		private class CustomerDto
		{
			public IList<AddressDto> Addresses { get; set; }
		}

		private class AddressDto
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
		}
	}
}