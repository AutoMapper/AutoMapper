﻿using Xunit;
﻿using Should;

namespace AutoMapper.UnitTests.Projection
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using AutoMapper;
	using QueryableExtensions;

    public class ProjectTest
	{
        private IExpressionBuilder _builder;

        public ProjectTest()
        {
            var config = new MapperConfiguration();
			config.CreateMap<Address, AddressDto>();
			config.CreateMap<Customer, CustomerDto>();
            _builder = config.CreateExpressionBuilder();
        }

		[Fact(Skip = "EF doesn't support null values in expressions")]
		public void SelectUsingProjectToWithNullComplexSourceProperty()
		{
			var customers = new[] { new Customer { FirstName = "Bill", LastName = "White" } }.AsQueryable();

			var projected = customers.ProjectTo<CustomerDto>(_builder).SingleOrDefault();
			projected.ShouldNotBeNull();
			projected.Address.ShouldBeNull();
		}

		[Fact]
		public void ProjectToWithUnmappedTypeShouldThrowException()
		{
			var customers =
				new[] { new Customer { FirstName = "Bill", LastName = "White", Address = new Address("Street1") } }
					.AsQueryable();

			IList<Unmapped> projected = null;

            typeof(InvalidOperationException).ShouldBeThrownBy(() => projected = customers.ProjectTo<Unmapped>(_builder).ToList());

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
