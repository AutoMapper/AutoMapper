using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests
{
    using System;
    using UnitTests;

    public class EnumToUnderlyingType : AutoMapperSpecBase
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
        public class Context : DbContext
        {
            public Context() =>  Database.SetInitializer(new DatabaseInitializer());
            public DbSet<Customer> Customers { get; set; }
        }
        public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
        {
            protected override void Seed(Context context)
            {
                context.Customers.Add(new Customer { Id = 1, FirstName = "Bob", LastName = "Smith", ConsoleColor = ConsoleColor.Yellow });
                base.Seed(context);
            }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<Customer, CustomerViewModel>());
        [Fact]
        public void Can_map_with_projection()
        {
            using (var context = new Context())
            {
                ProjectTo<CustomerViewModel>(context.Customers).First().ConsoleColor.ShouldBe((int)ConsoleColor.Yellow);
            }
        }
    }
    public class UnderlyingTypeToEnum : AutoMapperSpecBase
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
        public class Context : DbContext
        {
            public Context() => Database.SetInitializer(new DatabaseInitializer());
            public DbSet<Customer> Customers { get; set; }
        }
        public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
        {
            protected override void Seed(Context context)
            {
                context.Customers.Add(new Customer { Id = 1, FirstName = "Bob", LastName = "Smith", ConsoleColor = (int)ConsoleColor.Yellow });
                base.Seed(context);
            }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<Customer, CustomerViewModel>());
        [Fact]
        public void Can_map_with_projection()
        {
            using (var context = new Context())
            {
                ProjectTo<CustomerViewModel>(context.Customers).First().ConsoleColor.ShouldBe(ConsoleColor.Yellow);
            }
        }
    }
    public class EnumToEnum : AutoMapperSpecBase
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
        public class Context : DbContext
        {
            public Context() => Database.SetInitializer(new DatabaseInitializer());
            public DbSet<Customer> Customers { get; set; }
        }
        public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
        {
            protected override void Seed(Context context)
            {
                context.Customers.Add(new Customer { Id = 1, FirstName = "Bob", LastName = "Smith", ConsoleColor = DayOfWeek.Saturday });
                base.Seed(context);
            }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<Customer, CustomerViewModel>());
        [Fact]
        public void Can_map_with_projection()
        {
            using (var context = new Context())
            {
                ProjectTo<CustomerViewModel>(context.Customers).First().ConsoleColor.ShouldBe(ConsoleColor.DarkYellow);
            }
        }
    }
}