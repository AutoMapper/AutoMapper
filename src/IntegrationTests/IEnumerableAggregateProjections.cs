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

    public class IEnumerableAggregateProjections : AutoMapperSpecBase
    {
        public class Customer
        {
            [Key]
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public ICollection<Item> Items { get; set; }
        }

        public class Item
        {
            public int Id { get; set; }
            public int Code { get; set; }
        }

        public class CustomerViewModel
        {
            public int ItemCodesCount { get; set; }
            public int ItemCodesMin { get; set; }
            public int ItemCodesMax { get; set; }
            public int ItemCodesSum { get; set; }
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
                    Items = new[] { new Item { Code = 1 }, new Item { Code = 3 }, new Item { Code = 5 } }
                });

                base.Seed(context);
            }
        }

        public class CustomerItemCodes
        {
            public IEnumerable<int> ItemCodes { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.CreateMap<CustomerItemCodes, CustomerViewModel>());

        [Fact]
        public void Can_map_with_projection()
        {
            using (var context = new Context())
            {
                var result = ProjectTo<CustomerViewModel>(context.Customers.Select(customer => new CustomerItemCodes
                {
                    ItemCodes = customer.Items.Select(item => item.Code)
                })).Single();

                result.ItemCodesCount.ShouldBe(3);
                result.ItemCodesMin.ShouldBe(1);
                result.ItemCodesMax.ShouldBe(5);
                result.ItemCodesSum.ShouldBe(9);
            }
        }
    }
}
