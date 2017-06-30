using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Xunit;
using Assert = Should.Core.Assertions.Assert;
using Should;

namespace AutoMapper.IntegrationTests.Net4
{
    using UnitTests;
    using QueryableExtensions;
    using System.Data.Entity.ModelConfiguration;

    public class ProjectAndAllowNullCollections : AutoMapperSpecBase
    {
        public class Foo
        {
            public virtual int ID { get; set; }

            public virtual ISet<Bar> Bars { get; } = new HashSet<Bar>();

            public virtual ISet<Baz> Bazs { get; } = new HashSet<Baz>();
        }

        public class Bar
        {
            public virtual int ID { get; set; }

            public virtual string Value { get; set; }
        }

        public class Baz
        {
            public virtual int ID { get; set; }

            public virtual string Value { get; set; }
        }

        public class FooDto
        {
            public int ID { get; set; }

            public List<BarDto> Bars { get; set; }

            public List<BazDto> Bazs { get; set; }
        }

        public class BarDto
        {
            public int ID { get; set; }

            public string Value { get; set; }
        }

        public class BazDto
        {
            public int ID { get; set; }

            public string Value { get; set; }
        }

        public class MyContextInitializer : DropCreateDatabaseAlways<MyContext>
        {
            public override void InitializeDatabase(MyContext context)
            {
                base.InitializeDatabase(context);

                var foo1 = new Foo();
                var foo2 = new Foo(); // { Bars = new List<Bar>() };
                var foo3 = new Foo(); // { Bars = new List<Bar>() };

                context.Foos.Add(foo1);
                context.Foos.Add(foo2);
                context.Foos.Add(foo3);

                context.SaveChanges();

                var bar1 = new Bar { Value = "bar1" };
                var bar2 = new Bar { Value = "bar2" };

                foo2.Bars.Add(bar1);

                foo3.Bars.Add(bar1);
                foo3.Bars.Add(bar2);

                context.Bars.Add(bar1);
                context.Bars.Add(bar2);

                context.SaveChanges();
            }
        }

        public class MyContext : DbContext
        {
            public MyContext() : base("AutomapperNullCollections")
            {
                Database.SetInitializer(new MyContextInitializer());
            }

            public DbSet<Foo> Foos { get; set; }

            public DbSet<Bar> Bars { get; set; }

            public DbSet<Baz> Bazs { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Configurations.AddFromAssembly(typeof(MyContext).Assembly);
            }
        }

        public class FooConfiguration : EntityTypeConfiguration<Foo>
        {
            public FooConfiguration()
            {
                HasMany(m => m.Bars).WithMany();
                HasMany(m => m.Bazs).WithMany();
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(c =>
        {
            c.AllowNullCollections = true;

            c.CreateMap<Foo, FooDto>()
                .ForMember(d => d.Bars, o => o.ExplicitExpansion())
                .ForMember(d => d.Bazs, o => o.ExplicitExpansion());

            c.CreateMap<Bar, BarDto>();
            c.CreateMap<Baz, BazDto>();
        });

        [Fact]
        public void Should_work()
        {
            using(var context = new MyContext())
            {
                var foos = context.Foos.AsNoTracking().ProjectTo<FooDto>(Configuration, m => m.Bars).ToList();

                foos[0].Bars.ShouldNotBeNull();
                foos[0].Bazs.ShouldBeNull();
            }
        }
    }
}