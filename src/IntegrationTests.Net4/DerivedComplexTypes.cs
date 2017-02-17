using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Should;
using Xunit;

namespace AutoMapper.IntegrationTests
{
    using UnitTests;
    using QueryableExtensions;
    using System.ComponentModel.DataAnnotations.Schema;

    public class DerivedComplexTypes : AutoMapperSpecBase
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

        public class Context : DbContext
        {
            public Context()
            {
                Database.SetInitializer<Context>(new DatabaseInitializer());
            }

            public DbSet<Customer> Customers { get; set; }
        }

        public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
        {
            protected override void Seed(Context context)
            {
                context.Customers.Add(new Customer
                {
                    Id = 1,
                    FirstName = "Bob",
                    LastName = "Smith",
                    Address = new DerivedLocalizedString { Value = "home" }
                });

                base.Seed(context);
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Customer, CustomerViewModel>();
            cfg.CreateMap<LocalizedString, string>().ProjectUsing(v=>v.Value);
        });

        [Fact]
        public void Can_map_with_projection()
        {
            using (var context = new Context())
            {
                var customerVm = context.Customers.ProjectTo<CustomerViewModel>(Configuration).First();
                customerVm.Address.ShouldEqual("home");
            }
        }
    }
}