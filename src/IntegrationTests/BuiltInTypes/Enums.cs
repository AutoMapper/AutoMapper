﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.UnitTests;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.BuiltInTypes;

public class EnumToUnderlyingType : IntegrationTest<EnumToUnderlyingType.DatabaseInitializer>
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ConsoleColor ConsoleColor { get; set; }
    }
    public class CustomerViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ConsoleColor { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer { FirstName = "Bob", LastName = "Smith", ConsoleColor = ConsoleColor.Yellow });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<Customer, CustomerViewModel>());
    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = new Context())
        {
            ProjectTo<CustomerViewModel>(context.Customers).First().ConsoleColor.ShouldBe((int)ConsoleColor.Yellow);
        }
    }
}
public class UnderlyingTypeToEnum : IntegrationTest<UnderlyingTypeToEnum.DatabaseInitializer>
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ConsoleColor { get; set; }
    }
    public class CustomerViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ConsoleColor ConsoleColor { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer { FirstName = "Bob", LastName = "Smith", ConsoleColor = (int)ConsoleColor.Yellow });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<Customer, CustomerViewModel>());
    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = new Context())
        {
            ProjectTo<CustomerViewModel>(context.Customers).First().ConsoleColor.ShouldBe(ConsoleColor.Yellow);
        }
    }
}
public class EnumToEnum : IntegrationTest<EnumToEnum.DatabaseInitializer>
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DayOfWeek ConsoleColor { get; set; }
    }
    public class CustomerViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ConsoleColor ConsoleColor { get; set; }
    }
    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }
    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer { FirstName = "Bob", LastName = "Smith", ConsoleColor = DayOfWeek.Saturday });
            base.Seed(context);
        }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.CreateProjection<Customer, CustomerViewModel>());
    [Fact]
    public void Can_map_with_projection()
    {
        using (var context = new Context())
        {
            ProjectTo<CustomerViewModel>(context.Customers).First().ConsoleColor.ShouldBe(ConsoleColor.DarkYellow);
        }
    }
}