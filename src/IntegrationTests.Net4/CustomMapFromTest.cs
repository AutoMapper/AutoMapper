using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Xunit;
using Assert = Should.Core.Assertions.Assert;
using Should;

namespace AutoMapper.IntegrationTests.Net4
{
    namespace CustomMapFromTest
    {
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
        
        public class AutoMapperQueryableExtensionsThrowsNullReferenceExceptionSpec
        {
            public AutoMapperQueryableExtensionsThrowsNullReferenceExceptionSpec()
            {
                Mapper.CreateMap<Customer, CustomerViewModel>()
                      .ForMember(x => x.FullAddress,
                                 o => o.MapFrom(s => String.Format("{0}, {1} {2}", 
                                                            s.Address.Street, 
                                                            s.Address.City, 
                                                            s.Address.State))); 

                Mapper.AssertConfigurationIsValid();
            }

            [Fact]
            public void can_map_with_projection()
            {
                using (var context = new Context())
                {
                    var customerVms = context.Customers.Select(c => new CustomerViewModel
                    {
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        FullAddress = String.Format("{0}, {1} {2}",
                                                    c.Address.Street,
                                                    c.Address.City,
                                                    c.Address.State)
                    }).ToList();

                    customerVms.ForEach(x =>
                    {
                        x.FullAddress.ShouldNotBeNull();
                        x.FullAddress.ShouldNotBeEmpty();
                    });

                    customerVms = context.Customers.Project().To<CustomerViewModel>().ToList();
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