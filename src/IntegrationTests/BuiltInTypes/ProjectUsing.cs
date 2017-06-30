using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Xunit;
using Should;

namespace AutoMapper.IntegrationTests.Net4
{
    using UnitTests;
    using QueryableExtensions;

    public class ProjectUsingWithNullables : AutoMapperSpecBase
    {
        public class MyProfile : Profile
        {
            public MyProfile()
            {
                CreateMap<MyTable, MyTableModel>();
                CreateMap<int, MyEnum>().ProjectUsing(x => (MyEnum)x);
                CreateMap<int?, MyEnum>().ProjectUsing(x => x.HasValue ? (MyEnum)x.Value : MyEnum.Value1);
            }
        }

        public enum MyEnum
        {
            Value1 = 0,
            Value2 = 1
        }

        public class MyTable
        {
            public int Id { get; set; }
            public int EnumValue { get; set; }
            public int? EnumValueNullable { get; set; }
        }

        public class MyTableModel
        {
            public int Id { get; set; }
            public MyEnum EnumValue { get; set; }
            public MyEnum EnumValueNullable { get; set; }
        }

        public class DatabaseInitializer : CreateDatabaseIfNotExists<TestContext>
        {
            protected override void Seed(TestContext context)
            {
                context.MyTable.AddRange(new[]{
                    new MyTable { Id = 1, EnumValue = (int)MyEnum.Value2 },
                    new MyTable { Id = 2, EnumValueNullable = (int?)MyEnum.Value1 },
                });
                base.Seed(context);
            }
        }

        public class TestContext : DbContext
        {
            public TestContext()
            {
                Database.SetInitializer(new DatabaseInitializer());
            }
            public DbSet<MyTable> MyTable { get; set; }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.AddProfile<MyProfile>());

        [Fact]
        public void Should_project_ok()
        {
            using(var context = new TestContext())
            {
                var results = context.MyTable.ProjectTo<MyTableModel>(Configuration).ToList();
                results[0].Id.ShouldEqual(1);
                results[0].EnumValue.ShouldEqual(MyEnum.Value2);
                results[0].EnumValueNullable.ShouldEqual(MyEnum.Value1);
                results[1].Id.ShouldEqual(2);
                results[1].EnumValue.ShouldEqual(MyEnum.Value1);
                results[1].EnumValueNullable.ShouldEqual(MyEnum.Value1);
            }
        }
    }

    public class ProjectUsingBug : AutoMapperSpecBase
    {
        public class Parent
        {
            [Key]
            public int ID { get; set; }
            public string ParentTitle { get; set; }

            public ICollection<Children> Children { get; set; }
        }

        public class Children
        {
            public int ID { get; set; }
            public string ChildTitle { get; set; }
        }

        public class ParentVM
        {
            [Key]
            public int ID { get; set; }
            public string ParentTitle { get; set; }
            public List<int> Children { get; set; }
        }

        public partial class ApplicationDBContext : DbContext
        {
            public ApplicationDBContext()
            {
                Database.SetInitializer(new CreateDatabaseIfNotExists<ApplicationDBContext>());
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Parent>()
                    .HasMany(x => x.Children);
            }

            public DbSet<Parent> Parents { get; set; }
            public DbSet<Children> Children { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Parent, ParentVM>();
            cfg.CreateMap<Children, int>()
                .ProjectUsing(c => c.ID);
        });

        [Fact]
        public void can_map_with_projection()
        {
            using (var db = new ApplicationDBContext())
            {
                var result = db.Parents.ProjectTo<ParentVM>(Configuration);
            }
        }
    }
}