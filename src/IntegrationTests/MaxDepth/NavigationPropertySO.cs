﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.UnitTests;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.MaxDepth;

public class NavigationPropertySO : IntegrationTest<NavigationPropertySO.DatabaseInitializer>
{
    CustomerDTO _destination;

    public class Cust
    {

        [Key]
        public int CustomerID { get; set; }

        public string CustomerNumber { get; set; }
        public bool Status { get; set; }
        public virtual ICollection<Customer> Customers { get; set; }
    }

    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Cust")]
        public int CustomerId { get; set; }
        public virtual Cust Cust { get; set; }
        public bool Status { get; set; }
        public string Name1 { get; set; }
    }

    public class CustDTO
    {
        public int CustomerID { get; set; }
        public string CustomerNumber { get; set; }
        public bool Status { get; set; }

        public virtual ICollection<CustomerDTO> Customers { get; set; }
    }

    public class CustomerDTO
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public virtual CustDTO Cust { get; set; }
        public bool Status { get; set; }
        public string Name1 { get; set; }
    }

    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Cust> Custs { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            var cust = new Cust { };
            context.Custs.Add(cust);
            var customer = new Customer
            {
                Name1 = "Bob",
                CustomerId = 1,
                Cust = cust,
            };
            context.Customers.Add(customer);
            cust.Customers.Add(customer);
            base.Seed(context);
        }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerDTO>().MaxDepth(1);
        cfg.CreateProjection<Cust, CustDTO>();
    });

    [Fact]
    public void Can_map_with_projection()
    {
        using(var context = new Context())
        {
            _destination = ProjectTo<CustomerDTO>(context.Customers).Single();
            _destination.Id.ShouldBe(1);
            _destination.Name1.SequenceEqual("Bob");
            _destination.Cust.CustomerID.ShouldBe(1);
        }
    }
}