using System.Linq;
using AutoMapper.UnitTests;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.Core
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
                Database.EnsureDeleted();
                Database.EnsureCreated();
                Seed();
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase("Local");
                base.OnConfiguring(optionsBuilder);
            }

            private void Seed()
            {
                var source = new Source
                {
                    Name = "name", InnerSource = new InnerSource {Description = "description"},
                    OtherInnerSource = new OtherInnerSource {Title = "title"}
                };
                Sources.Add(source);
                SaveChanges();
            }

            public DbSet<Source> Sources { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>(MemberList.None);
        });

        [Fact]
        public void Should_flatten()
        {
            using var context = new Context();
            var projectTo = ProjectTo<Destination>(context.Sources);
            var result = projectTo.Single();
            result.Name.ShouldBe("name");
            result.Description.ShouldBe("description");
            result.Title.ShouldBe("title");
        }
    }
}