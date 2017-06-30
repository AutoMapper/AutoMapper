using System.Data.Entity;
using System.Linq;
using AutoMapper.QueryableExtensions;
using AutoMapper.UnitTests;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.Net4
{
    public class ProjectToAbstractType : AutoMapperSpecBase
    {
        ITypeA[] _destinations;

        public interface ITypeA
        {
            int ID { get; set; }
            string Name { get; set; }
        }

        public class ConcreteTypeA : ITypeA
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class DbEntityA
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
        {
            protected override void Seed(Context context)
            {
                context.EntityA.AddRange(new[]
                {
                    new DbEntityA { ID = 1, Name = "Alain Brito"},
                    new DbEntityA { ID = 2, Name = "Jimmy Bogard"},
                    new DbEntityA { ID = 3, Name = "Bill Gates"}
                });
                base.Seed(context);
            }
        }
        public class Context : DbContext
        {
            public Context()
            {
                Database.SetInitializer<Context>(new DatabaseInitializer());
            }

            public DbSet<DbEntityA> EntityA { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<DbEntityA, ITypeA>().As<ConcreteTypeA>();
            cfg.CreateMap<DbEntityA, ConcreteTypeA>();
        });

        [Fact]
        public void Should_project_to_abstract_type()
        {
            using(var context = new Context())
            {
                _destinations = context.EntityA.ProjectTo<ITypeA>(Configuration).ToArray();
            }
            _destinations.Length.ShouldBe(3);
            _destinations[2].Name.ShouldBe("Bill Gates");
        }
    }
}