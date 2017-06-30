using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Xunit;
using Assert = Should.Core.Assertions.Assert;
using Should;

namespace AutoMapper.IntegrationTests.Net4
{
    namespace CustomMapFromTest
    {
        using AutoMapper.UnitTests;
        using QueryableExtensions;

        public class Customer
        {
            [Key]
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public Address Address { get; set; }
        }

        public class Address
        {
            [Key]
            public int Id { get; set; }
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
        }

        public class CustomerViewModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string FullAddress { get; set; }
        }

        public class Context : DbContext
        {
            public Context()
                : base()
            {
                Database.SetInitializer<Context>(new DatabaseInitializer());
            }

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Address> Addresses { get; set; }

        }

        public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
        {
            protected override void Seed(Context context)
            {
                context.Customers.Add(new Customer
                {
                    Id = 1,
                    FirstName = "Bob",
                    LastName = "Smith",
                    Address = new Address
                    {
                        Id = 1,
                        Street = "123 Anywhere",
                        City = "Austin",
                        State = "TX"
                    }
                });

                base.Seed(context);
            }
        }
        
        public class AutoMapperQueryableExtensionsThrowsNullReferenceExceptionSpec : AutoMapperSpecBase
        {
            protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Customer, CustomerViewModel>()
                    .ForMember(x => x.FullAddress,
                        o => o.MapFrom(c => c.Address.Street + ", " + c.Address.City + " " + c.Address.State));
            });

            [Fact]
            public void can_map_with_projection()
            {
                using (var context = new Context())
                {
                    var customerVms = context.Customers.Select(c => new CustomerViewModel
                    {
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        FullAddress = c.Address.Street + ", " + c.Address.City + " " + c.Address.State
                    }).ToList();

                    customerVms.ForEach(x =>
                    {
                        x.FullAddress.ShouldNotBeNull();
                        x.FullAddress.ShouldNotBeEmpty();
                    });

                    customerVms = context.Customers.ProjectTo<CustomerViewModel>(Configuration).ToList();
                    customerVms.ForEach(x =>
                    {
                        x.FullAddress.ShouldNotBeNull();
                        x.FullAddress.ShouldNotBeEmpty();
                    });
                }
            }
        }
    }
}