namespace AutoMapper.UnitTests.Projection
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using AutoMapper;
	using AutoMapper.QueryableExtensions;

	using NUnit.Framework;

	using SharpTestsEx;

	[TestFixture]
	public class ProjectTest
	{
		[SetUp]
		public void SetUp()
		{
			Mapper.CreateMap<Address, AddressDto>();
			Mapper.CreateMap<Customer, CustomerDto>();
		}

		[Test]
		public void SelectUsingProjectToWithNullComplexSourceProperty()
		{
			var customers = new[] { new Customer { FirstName = "Bill", LastName = "White" } }.AsQueryable();

			var projected = customers.Project().To<CustomerDto>().SingleOrDefault();
			projected.Should().Not.Be.Null();
			projected.Address.Should().Be.Null();
		}

		[Test]
		public void ProjectToWithUnmappedTypeShouldThrowException()
		{
			var customers =
				new[] { new Customer { FirstName = "Bill", LastName = "White", Address = new Address("Street1") } }
					.AsQueryable();

			IList<Unmapped> projected = null;

			const string Message = "Missing map from Customer to Unmapped. Create using Mapper.CreateMap<Customer, Unmapped>.";

			Executing
				.This(() => projected = customers.Project().To<Unmapped>().ToList())
				.Should().Throw<InvalidOperationException>()
				.And.Exception.Message.Should().Be.EqualTo(Message);

			projected.Should().Be.Null();
		}

		private class Customer
		{
			public string FirstName { get; set; }

			public string LastName { get; set; }

			public Address Address { get; set; }
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
			public string FirstName { get; set; }

			public AddressDto Address { get; set; }
		}

		private class AddressDto
		{
			public string Street { get; set; }
		}

		private class Unmapped
		{
			public string FirstName { get; set; }
		}
	}
}