using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.UnitTests;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests
{
    public class IncludeMembers : AutoMapperSpecBase
    {
        class Source
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public InnerSource InnerSource { get; set; }
            public OtherInnerSource OtherInnerSource { get; set; }
        }
        class InnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
        class OtherInnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
        }
        class Destination
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
        }
        class Context : DbContext
        {
            public Context()
            {
                Database.SetInitializer(new DatabaseInitializer());
            }

            public DbSet<Source> Sources { get; set; }
        }

        class DatabaseInitializer : DropCreateDatabaseAlways<Context>
        {
            protected override void Seed(Context context)
            {
                var source = new Source { Name = "name", InnerSource = new InnerSource { Description = "description" }, OtherInnerSource = new OtherInnerSource { Title = "title" } };
                context.Sources.Add(source);
                base.Seed(context);
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg=>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s=>s.InnerSource, s=>s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });
        [Fact]
        public void Should_flatten()
        {
            using(var context = new Context())
            {
                var result = ProjectTo<Destination>(context.Sources).Single();
                result.Name.ShouldBe("name");
                result.Description.ShouldBe("description");
                result.Title.ShouldBe("title");
            }
        }
    }

    public class IncludeMembersWithMapFromExpression : AutoMapperSpecBase
    {
        class Source
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public InnerSource InnerSource { get; set; }
            public OtherInnerSource OtherInnerSource { get; set; }
        }
        class InnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description1 { get; set; }
        }
        class OtherInnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title1 { get; set; }
        }
        class Destination
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Title { get; set; }
        }
        class Context : DbContext
        {
            public Context()
            {
                Database.SetInitializer(new DatabaseInitializer());
            }

            public DbSet<Source> Sources { get; set; }
        }

        class DatabaseInitializer : DropCreateDatabaseAlways<Context>
        {
            protected override void Seed(Context context)
            {
                var source = new Source { Name = "name", InnerSource = new InnerSource { Description1 = "description" }, OtherInnerSource = new OtherInnerSource { Title1 = "title" } };
                context.Sources.Add(source);
                base.Seed(context);
            }
        }
        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d=>d.Description, o=>o.MapFrom(s=>s.Description1));
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d=>d.Title, o=>o.MapFrom(s=>s.Title1));
        });
        [Fact]
        public void Should_flatten_with_MapFrom()
        {
            using(var context = new Context())
            {
                var result = ProjectTo<Destination>(context.Sources).Single();
                result.Name.ShouldBe("name");
                result.Description.ShouldBe("description");
                result.Title.ShouldBe("title");
            }
        }
    }

    public class IncludeMembersWithNullSubstitute : AutoMapperSpecBase
    {
        class Source
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public InnerSource InnerSource { get; set; }
            public OtherInnerSource OtherInnerSource { get; set; }
        }
        class InnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? Code { get; set; }
        }
        class OtherInnerSource
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? Code { get; set; }
            public int? OtherCode { get; set; }
        }
        class Destination
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Code { get; set; }
            public int OtherCode { get; set; }
        }
        class Context : DbContext
        {
            public Context()
            {
                Database.SetInitializer(new DatabaseInitializer());
            }

            public DbSet<Source> Sources { get; set; }
        }

        class DatabaseInitializer : DropCreateDatabaseAlways<Context>
        {
            protected override void Seed(Context context)
            {
                var source = new Source { Name = "name" };
                context.Sources.Add(source);
                base.Seed(context);
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None).ForMember(d => d.Code, o => o.NullSubstitute(5));
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None).ForMember(d => d.OtherCode, o => o.NullSubstitute(7));
        });
        [Fact]
        public void Should_flatten()
        {
            using(var context = new Context())
            {
                var result = ProjectTo<Destination>(context.Sources).Single();
                result.Name.ShouldBe("name");
                result.Code.ShouldBe(5);
                result.OtherCode.ShouldBe(7);
            }
        }
    }
}