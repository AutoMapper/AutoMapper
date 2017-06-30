using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using Xunit;
using Shouldly;
using AutoMapper.QueryableExtensions;
using AutoMapper.UnitTests;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AutoMapper.IntegrationTests.Net4
{
    public class NestedDtos : AutoMapperSpecBase
    {
        ArtDto _destination;

        public class Sem
        {
            [Key]
            public int Key { get; set; }
            public string Name { get; set; }

            public virtual ICollection<Art> Arts { get; set; }
        }

        public class Art
        {
            [Key]
            public int Key { get; set; }
            public string AName { get; set; }

            [ForeignKey("Sem")]
            public int? SemKey { get; set; }
            public virtual Sem Sem { get; set; }
        }

        public class SemDto
        {
            public int Key { get; set; }
            public string Name { get; set; }
        }

        public class ArtDto
        {
            public int Key { get; set; }
            public string AName { get; set; }

            public SemDto Sem { get; set; }
        }

        public class TestContext : DbContext
        {
            public TestContext()
            {
                Database.SetInitializer<TestContext>(new DatabaseInitializer());
            }
            public DbSet<Art> Arts { get; set; }
        }

        public class DatabaseInitializer : CreateDatabaseIfNotExists<TestContext>
        {
            protected override void Seed(TestContext context)
            {
                context.Arts.AddRange(new[] {
                    new Art { AName = "art1", Sem = new Sem { Name = "sem1" } },
                    new Art { AName = "art2", Sem = new Sem { Name = "sem2" } },
                    new Art { AName = "art3", Sem = new Sem { Name = "sem3" } },
                });

                base.Seed(context);
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Sem, SemDto>().MaxDepth(1).ConstructProjectionUsing(s => new SemDto());
            cfg.CreateMap<Art, ArtDto>().MaxDepth(1).ConstructProjectionUsing(s => new ArtDto());
        });

        protected override void Because_of()
        {
            using(var context = new TestContext())
            {
                _destination = context.Arts.ProjectTo<ArtDto>(Configuration).FirstOrDefault();
            }
        }

        [Fact]
        public void Should_project_nested_dto()
        {
            _destination.AName.ShouldBe("art1");
            _destination.Sem.Name.ShouldBe("sem1");
        }
    }
}