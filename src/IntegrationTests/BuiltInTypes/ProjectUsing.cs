﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.UnitTests;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.BuiltInTypes;

public class ProjectUsingWithNullables : AutoMapperSpecBase, IAsyncLifetime
{
    public class MyProfile : Profile
    {
        public MyProfile()
        {
            CreateProjection<MyTable, MyTableModel>();
            CreateProjection<int, MyEnum>().ConvertUsing(x => (MyEnum)x);
            CreateProjection<int?, MyEnum>().ConvertUsing(x => x.HasValue ? (MyEnum)x.Value : MyEnum.Value1);
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
                new MyTable { EnumValue = (int)MyEnum.Value2 },
                new MyTable { EnumValueNullable = (int?)MyEnum.Value1 },
            });
            base.Seed(context);
        }
    }

    public class TestContext : LocalDbContext
    {
        public DbSet<MyTable> MyTable { get; set; }
    }
    protected override MapperConfiguration CreateConfiguration() => new(cfg => cfg.AddProfile<MyProfile>());

    [Fact]
    public void Should_project_ok()
    {
        using(var context = new TestContext())
        {
            var results = ProjectTo<MyTableModel>(context.MyTable).ToList();
            results[0].Id.ShouldBe(1);
            results[0].EnumValue.ShouldBe(MyEnum.Value2);
            results[0].EnumValueNullable.ShouldBe(MyEnum.Value1);
            results[1].Id.ShouldBe(2);
            results[1].EnumValue.ShouldBe(MyEnum.Value1);
            results[1].EnumValueNullable.ShouldBe(MyEnum.Value1);
        }
    }

    public async Task InitializeAsync()
    {
        var initializer = new DatabaseInitializer();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class ProjectUsingBug : AutoMapperSpecBase, IAsyncLifetime
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

    public partial class ApplicationDBContext : LocalDbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>()
                .HasMany(x => x.Children);
        }

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Children> Children { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg =>
    {
        cfg.CreateProjection<Parent, ParentVM>();
        cfg.CreateProjection<Children, int>()
            .ConvertUsing(c => c.ID);
    });

    [Fact]
    public void can_map_with_projection()
    {
        using (var db = new ApplicationDBContext())
        {
            var result = ProjectTo<ParentVM>(db.Parents);
        }
    }

    public async Task InitializeAsync()
    {
        var initializer = new CreateDatabaseIfNotExists<ApplicationDBContext>();

        await initializer.Migrate();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}