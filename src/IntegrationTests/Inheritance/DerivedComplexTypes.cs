﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.UnitTests;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.Inheritance;

public class DerivedComplexTypes : IntegrationTest<DerivedComplexTypes.DatabaseInitializer>
{
    [ComplexType]
    public class LocalizedString
    {
        public string Value { get; set; }
    }

    [ComplexType]
    public class DerivedLocalizedString : LocalizedString
    {
    }

    public class Customer
    {
        public Customer()
        {
        }

        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DerivedLocalizedString Address { get; set; }
    }

    public class CustomerViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
    }

    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().OwnsOne(c => c.Address);
        }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer
            {
                FirstName = "Bob",
                LastName = "Smith",
                Address = new DerivedLocalizedString { Value = "home" }
            });

            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>();
        cfg.CreateProjection<LocalizedString, string>().ConvertUsing(v => v.Value);
    });

    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = new Context())
        {
            var customerVm = ProjectTo<CustomerViewModel>(context.Customers).First();
            customerVm.Address.ShouldBe("home");
        }
    }
}