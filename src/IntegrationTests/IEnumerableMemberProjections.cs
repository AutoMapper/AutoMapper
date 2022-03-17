using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests;

using UnitTests;
using QueryableExtensions;
using System.Collections.Generic;

public class IEnumerableMemberProjections : AutoMapperSpecBase, IAsyncLifetime
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

    public class ItemModel
    {
        public int Id { get; set; }
        public int Code { get; set; }
    }

    public class CustomerViewModel
    {
        public IEnumerable<ItemModel> Items { get; set; }
    }

    public class Context : LocalDbContext
    {
        public DbSet<Customer> Customers { get; set; }
    }

    public class DatabaseInitializer : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            context.Customers.Add(new Customer
            {
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

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Customer, CustomerViewModel>();
        cfg.CreateProjection<Item, ItemModel>();
    });

    [Fact]
    public void Can_map_to_ienumerable()
    {
        using (var context = new Context())
        {
            var result = ProjectTo<CustomerViewModel>(context.Customers).Single();

            result.Items.Count().ShouldBe(3);
        }
    }

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;

}