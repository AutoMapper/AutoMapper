using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests
{
    using UnitTests;
    using QueryableExtensions;
    using System.Collections.Generic;

    public class ProjectEnumerableOfIntToList : AutoMapperSpecBase
    {
        public class Customer
        {
            [Key]
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<Item> Items { get; set; }
        }

        public class Item
        {
            public int Id { get; set; }
        }

        public class CustomerViewModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public List<int> ItemsIds { get; set; }
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
                    Items = new List<Item>(new[] { new Item { Id = 1 }, new Item { Id = 3 }, new Item { Id = 3 } })
                });

                base.Seed(context);
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Customer, CustomerViewModel>().ForMember(d=>d.ItemsIds, o=>o.MapFrom(s=>s.Items.Select(i=>i.Id)));
        });

        [Fact]
        public void Can_map_with_projection()
        {
            using(var context = new Context())
            {
                var customer = context.Customers.ProjectTo<CustomerViewModel>(Configuration).Single();
                customer.ItemsIds.SequenceEqual(new int[] { 1, 2, 3 }).ShouldBeTrue();
            }
        }
    }
}