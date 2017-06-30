using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using Should;
using Xunit;

namespace AutoMapper.IntegrationTests.Net4
{
    using QueryableExtensions;
    using UnitTests;

        
    public class NavigationPropertySO : AutoMapperSpecBase
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

        public class Context : DbContext
        {
            public Context()
            {
                Database.SetInitializer(new DatabaseInitializer());
            }

            public DbSet<Customer> Customers { get; set; }
            public DbSet<Cust> Custs { get; set; }
        }

        public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
        {
            protected override void Seed(Context context)
            {
                var cust = new Cust { CustomerID = 1 };
                context.Custs.Add(cust);
                var customer = new Customer
                {
                    Id = 1,
                    Name1 = "Bob",
                    CustomerId = 1,
                    Cust = cust,
                };
                context.Customers.Add(customer);
                cust.Customers.Add(customer);
                base.Seed(context);
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Customer, CustomerDTO>().MaxDepth(1);
            cfg.CreateMap<Cust, CustDTO>();
        });

        [Fact]
        public void Can_map_with_projection()
        {
            using(var context = new Context())
            {
                _destination = context.Customers.ProjectTo<CustomerDTO>(Configuration).Single();
                _destination.Id.ShouldEqual(1);
                _destination.Name1.SequenceEqual("Bob");
                _destination.Cust.CustomerID.ShouldEqual(1);
            }
        }
    }
}