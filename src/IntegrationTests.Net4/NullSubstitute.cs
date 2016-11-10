using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Should;
using Xunit;

namespace AutoMapper.IntegrationTests
{
    using UnitTests;
    using QueryableExtensions;
        
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

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Customer, CustomerViewModel>().ForMember(d => d.Value, o => o.NullSubstitute(5));
        });

        [Fact]
        public void Can_map_with_projection()
        {
            using (var context = new Context())
            {
                context.Customers.ProjectTo<CustomerViewModel>(Configuration).ToList()[0].Value.ShouldEqual(5);
            }
        }
    }
}