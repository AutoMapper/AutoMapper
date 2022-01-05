using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests
{
    using UnitTests;
        
    public class NullSubstitute : AutoMapperSpecBase
    {
        public class Customer
        {
            [Key]
            public int Id { get; set; }

            public double? Value { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class CustomerViewModel
        {
            public double Value { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class Context : DbContext
        {
            public Context()
            {
                Database.SetInitializer<Context>(new DatabaseInitializer());
            }

            public DbSet<Customer> Customers { get; set; }
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
                });

                base.Seed(context);
            }
        }

        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateProjection<Customer, CustomerViewModel>().ForMember(d => d.Value, o => o.NullSubstitute(5));
        });

        [Fact]
        public void Can_map_with_projection()
        {
            using (var context = new Context())
            {
                ProjectTo<CustomerViewModel>(context.Customers).First().Value.ShouldBe(5);
            }
        }
    }
    public class NullSubstituteWithStrings : AutoMapperSpecBase
    {
        public class Customer
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        public class CustomerViewModel
        {
            public string Value { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
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
                context.Customers.Add(new Customer { Id = 1, FirstName = "Bob", LastName = "Smith" });
                base.Seed(context);
            }
        }
        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            cfg.CreateProjection<Customer, CustomerViewModel>().ForMember(d => d.Value, o => o.NullSubstitute("5")));
        [Fact]
        public void Can_map_with_projection()
        {
            using (var context = new Context())
            {
                ProjectTo<CustomerViewModel>(context.Customers).First().Value.ShouldBe("5");
            }
        }
    }
    public class NullSubstituteWithEntity : AutoMapperSpecBase
    {
        class Customer
        {
            public int Id { get; set; }
            public Value Value { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        class Value
        {
            public int Id { get; set; }
        }
        class CustomerViewModel
        {
            public ValueViewModel Value { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
        class ValueViewModel
        {
            public int Id { get; set; }
        }
        class Context : DbContext
        {
            public Context() => Database.SetInitializer(new DatabaseInitializer());
            public DbSet<Customer> Customers { get; set; }
        }
        class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
        {
            protected override void Seed(Context context)
            {
                context.Customers.Add(new Customer { Id = 1, FirstName = "Bob", LastName = "Smith" });
                base.Seed(context);
            }
        }
        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateProjection<Customer, CustomerViewModel>().ForMember(d => d.Value, o => o.NullSubstitute(new Value()));
            cfg.CreateProjection<Value, ValueViewModel>();
        });
        [Fact]
        public void Can_map_with_projection()
        {
            using (var context = new Context())
            {
                ProjectTo<CustomerViewModel>(context.Customers).First().Value.ShouldBeNull();
            }
        }
    }
}