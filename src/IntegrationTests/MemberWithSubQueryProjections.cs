using Shouldly;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Xunit;

namespace AutoMapper.IntegrationTests
{
    using UnitTests;

    public class MemberWithSubQueryProjections : AutoMapperSpecBase
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
            public CustomerNameModel Name { get; set; }
            public ItemModel FirstItem { get; set; }
        }

        public class CustomerNameModel
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class Context : DbContext
        {
            public Context() => Database.SetInitializer<Context>(new DatabaseInitializer());

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

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Customer, CustomerViewModel>()
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.LastName != null ? src : null))
                .ForMember(dst => dst.FirstItem, opt =>
                {
                    opt.MapFrom(src => src.Items.FirstOrDefault());
                });

            cfg.CreateMap<Customer, CustomerNameModel>()
                .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.LastName));

            cfg.CreateProjection<Item, ItemModel>();
        });

        [Fact]
        public void Should_Create_Compilable_Expressions()
        {
            using (var context = new Context())
            {
                var resultQuery = ProjectTo<CustomerViewModel>(context.Customers);
                var result = resultQuery.Single();

                result.ShouldNotBeNull();

                result.Name.FirstName.ShouldBe("Bob");
                result.Name.LastName.ShouldBe("Smith");
                result.FirstItem.Id.ShouldBe(1);
                result.FirstItem.Code.ShouldBe(1);
            }
        }
    }
}
